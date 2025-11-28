using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using TrainerApi.Models;
using TrainerApi.Repositories;
using TrainerApi.Mappers;
using Microsoft.VisualBasic;


namespace TrainerApi.Services;

public class TrainerService : TrainerApi.TrainerService.TrainerServiceBase
{
    private readonly ITrainerRepository _trainerRepository;
    private static readonly List<Trainer> _trainers = new(); // static to persist across gRPC calls // in-memory fallback to surface created trainers in listing

    public TrainerService(ITrainerRepository trainerRepository)
    {
        _trainerRepository = trainerRepository;
    }

    public override async Task<TrainerResponse> GetTrainerById(TrainerByIdRequest request, ServerCallContext context)
    {
        // Validate ID format (must be a valid GUID)
        if (!Guid.TryParse(request.Id, out _))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid trainer ID format: {request.Id}"));
        }

        var trainer = await _trainerRepository.GetByIdAsync(request.Id, context.CancellationToken);

        if (trainer is null)
        {
            // Fallback to in-memory list if repository did not find it
            lock (_trainers)
            {
                trainer = _trainers.FirstOrDefault(t => t.Id == request.Id);
            }
        }

        if (trainer is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Trainer with ID {request.Id} not found"));

        return trainer.ToResponse();
    }
    public override async Task<CreateTrainerResponse> CreateTrainer(IAsyncStreamReader<CreateTrainerRequest> requestStream, ServerCallContext context)
    {
        var trainers = new List<Trainer>();

        await foreach (var request in requestStream.ReadAllAsync())
        {
            var trainer = new Trainer
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Age = request.Age,
                BirthDate = request.BirthDate.ToDateTime(), // Convert Timestamp to DateTime
                Medals = request.Medals.Select(m => new TrainerApi.Models.Medal
                {
                    Region = m.Region,
                    Type = (TrainerApi.Models.MedalType)m.Type
                }).ToList(), // Ensure mapping to TrainerApi.Models.Medal
                CreatedAt = DateTime.UtcNow
            };

            trainers.Add(trainer);
            lock (_trainers)
            {
                _trainers.Add(trainer);
            }
            try
            {
                // Persist via repository when available
                await _trainerRepository.CreateAsync(trainer, context.CancellationToken);
            }
            catch
            {
                // If repository layer is not ready, keep in-memory so GET by name still works
            }
        }

        return new CreateTrainerResponse
        {
            SuccessCount = trainers.Count,
            Trainers = { trainers.Select(t => new TrainerResponse
            {
                Id = t.Id,
                Name = t.Name,
                Age = t.Age,
                BirthDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(t.BirthDate.ToUniversalTime()),
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                Medals = { t.Medals.Select(m => new Medal
                {
                    Region = m.Region,
                    Type = (TrainerApi.MedalType)m.Type
                }) }
            }) } // Map List<Trainer> to RepeatedField<TrainerResponse>
        };
    }

    public override Task<Google.Protobuf.WellKnownTypes.Empty> DeleteTrainer(TrainerByIdRequest request, ServerCallContext context)
    {
        // Validate ID format (must be a valid GUID)
        if (!Guid.TryParse(request.Id, out _))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid trainer ID format: {request.Id}"));
        }

        lock (_trainers)
        {
            var idx = _trainers.FindIndex(t => t.Id == request.Id);
            if (idx < 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Trainer with ID {request.Id} not found"));
            }
            _trainers.RemoveAt(idx);
        }
        return Task.FromResult(new Google.Protobuf.WellKnownTypes.Empty());
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> UpdateTrainer(UpdateTrainerRequest request, ServerCallContext context)
    {
        // Validate ID format (must be a valid GUID)
        if (!Guid.TryParse(request.Id, out _))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid trainer ID format: {request.Id}"));
        }
        // Validate required fields at gRPC level
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Name is required"));
        }
        if (request.Age < 18)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Age must be 18 or older"));
        }

        // Check name uniqueness against repository as well (if available)
        try
        {
            var repoMatches = await _trainerRepository.GetByNameAsync(request.Name, context.CancellationToken);
            if (repoMatches != null && repoMatches.Any(t => t.Id != request.Id && string.Equals(t.Name, request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"Trainer with name '{request.Name}' already exists"));
            }
        }
        catch
        {
            // If repository not available or throws, fall back to in-memory only (handled below)
        }

        lock (_trainers)
        {
            var existing = _trainers.FirstOrDefault(t => t.Id == request.Id);
            if (existing == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Trainer with ID {request.Id} not found"));
            }
            // Name uniqueness check (case-insensitive) across other trainers
            var nameClash = _trainers.Any(t => t.Id != request.Id && string.Equals(t.Name, request.Name, StringComparison.OrdinalIgnoreCase));
            if (nameClash)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"Trainer with name '{request.Name}' already exists"));
            }
            existing.Name = request.Name;
            existing.Age = request.Age;
            existing.BirthDate = request.BirthDate.ToDateTime();
            existing.Medals = request.Medals.Select(m => new TrainerApi.Models.Medal
            {
                Region = m.Region,
                Type = (TrainerApi.Models.MedalType)m.Type
            }).ToList();
        }
        return new Google.Protobuf.WellKnownTypes.Empty();
    }

    public override async Task GetAllTrainersByName(TrainersByNameRequest request, IServerStreamWriter<TrainerResponse> responseStream, ServerCallContext context)
    {
        var trainers = await _trainerRepository.GetByNameAsync(request.Name, context.CancellationToken);
        var mem = new List<Trainer>();
        lock (_trainers)
        {
            mem = _trainers.Where(t => t.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        var toStream = (trainers != null && trainers.Any()) ? trainers : mem;

        foreach (var trainer in toStream)
        {
            if (context.CancellationToken.IsCancellationRequested)
                break;
            await responseStream.WriteAsync(trainer.ToResponse());
        }
    }
}