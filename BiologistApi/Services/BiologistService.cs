using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using BiologistApi.Models;
using BiologistApi.Repositories;
using BiologistApi.Mappers;
using Microsoft.VisualBasic;


namespace BiologistApi.Services;

public class BiologistService : BiologistApi.BiologistService.BiologistServiceBase
{
    private readonly IBiologistRepository _biologistRepository;
    private readonly ILogger<BiologistService> _logger;
    private static readonly List<Biologist> _biologists = new();

    public BiologistService(
        IBiologistRepository biologistRepository,
        ILogger<BiologistService> logger)
    {
        _biologistRepository = biologistRepository;
        _logger = logger;
    }

    public override async Task<BiologistResponse> GetBiologistById(BiologistByIdRequest request, ServerCallContext context)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(request.Id, out _))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid biologist ID format: {request.Id}"));
        }

        var biologist = await _biologistRepository.GetByIdAsync(request.Id, context.CancellationToken);

        if (biologist is null)
        {
            lock (_biologists)
            {
                biologist = _biologists.FirstOrDefault(t => t.Id == request.Id);
            }
        }

        if (biologist is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Biologist with ID {request.Id} not found"));

        return biologist.ToResponse();
    }

    public override async Task<CreateBiologistResponse> CreateBiologist(IAsyncStreamReader<CreateBiologistRequest> requestStream, ServerCallContext context)
    {
        var biologists = new List<Biologist>();

        await foreach (var request in requestStream.ReadAllAsync())
        {
            var biologist = new Biologist
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Name = request.Name,
                Age = request.Age,
                BirthDate = request.BirthDate != null ? request.BirthDate.ToDateTime() : DateTime.MinValue,
                Areas = request.Areas != null ? request.Areas.Select(a => new BiologistApi.Models.AreaModel
                {
                    Area = a.Area_,
                    SubArea = (BiologistApi.Models.SubArea)a.SubArea
                }).ToList() : new List<BiologistApi.Models.AreaModel>(),
                CreatedAt = DateTime.UtcNow
            };

            biologists.Add(biologist);
            lock (_biologists)
            {
                _biologists.Add(biologist);
            }
            try
            {
                await _biologistRepository.CreateAsync(biologist, context.CancellationToken);
            }
            catch
            {
            }
        }

        return new CreateBiologistResponse
        {
            SuccessCount = biologists.Count,
            Biologists = { biologists.Select(t => new BiologistResponse
            {
                Id = t.Id,
                Name = t.Name,
                Age = t.Age,
                BirthDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(t.BirthDate.ToUniversalTime()),
                Areas = { t.Areas.Select(m => new Area
                {
                    Area_ = m.Area,
                    SubArea = (BiologistApi.SubArea)m.SubArea
                }) }
            }) }
        };
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> DeleteBiologist(BiologistByIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("DeleteBiologist called for Id={Id}", request.Id);
        if (!MongoDB.Bson.ObjectId.TryParse(request.Id, out _))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid biologist ID format: {request.Id}"));
        }

        Biologist? deleted = null;
        lock (_biologists)
        {
            var idx = _biologists.FindIndex(t => t.Id == request.Id);
            if (idx >= 0)
            {
                deleted = _biologists[idx];
                _biologists.RemoveAt(idx);
            }
        }
        if (deleted is null)
        {
            try
            {
                deleted = await _biologistRepository.GetByIdAsync(request.Id, context.CancellationToken);
            }
            catch { }
            if (deleted is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Biologist with ID {request.Id} not found"));
            }
        }

        try
        {
            await _biologistRepository.DeleteAsync(request.Id, context.CancellationToken);
            _logger.LogInformation("Deleted biologist {BiologistId} from repository", request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete biologist {BiologistId} from repository", request.Id);
        }

        return new Google.Protobuf.WellKnownTypes.Empty();
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> UpdateBiologist(UpdateBiologistRequest request, ServerCallContext context)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(request.Id, out _))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid biologist ID format: {request.Id}"));
        }
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Name is required"));
        }
        if (request.Age < 18)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Age must be 18 or older"));
        }

        _logger.LogInformation("UpdateBiologist called for Id={Id}", request.Id);
        try
        {
            var repoMatches = await _biologistRepository.GetByNameAsync(request.Name, context.CancellationToken);
            if (repoMatches != null && repoMatches.Any(t => t.Id != request.Id && string.Equals(t.Name, request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"Biologist with name '{request.Name}' already exists"));
            }
        }
        catch
        {
        }

        Biologist? updated = null;
        lock (_biologists)
        {
            updated = _biologists.FirstOrDefault(t => t.Id == request.Id);
        }
        if (updated is null)
        {
            try
            {
                updated = await _biologistRepository.GetByIdAsync(request.Id, context.CancellationToken);
                if (updated != null)
                {
                    lock (_biologists)
                    {
                        if (_biologists.All(t => t.Id != updated.Id))
                        {
                            _biologists.Add(updated);
                        }
                    }
                }
            }
            catch { }
        }
        if (updated is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Biologist with ID {request.Id} not found"));
        }
        lock (_biologists)
        {
            var nameClash = _biologists.Any(t => t.Id != request.Id && string.Equals(t.Name, request.Name, StringComparison.OrdinalIgnoreCase));
            if (nameClash)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"Biologist with name '{request.Name}' already exists"));
            }
            updated.Name = request.Name;
            updated.Age = request.Age;
            updated.BirthDate = request.BirthDate != null ? request.BirthDate.ToDateTime() : DateTime.MinValue;
            updated.Areas = request.Areas != null ? request.Areas.Select(a => new BiologistApi.Models.AreaModel
            {
                Area = a.Area_,
                SubArea = (BiologistApi.Models.SubArea)a.SubArea
            }).ToList() : new List<BiologistApi.Models.AreaModel>();
        }

        try
        {
            await _biologistRepository.UpdateAsync(updated, context.CancellationToken);
            _logger.LogInformation("Updated biologist {BiologistId} in repository", request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update biologist {BiologistId} in repository", request.Id);
        }

        return new Google.Protobuf.WellKnownTypes.Empty();
    }

    public override async Task GetAllBiologistsByName(BiologistsByNameRequest request, IServerStreamWriter<BiologistResponse> responseStream, ServerCallContext context)
    {
        var biologists = await _biologistRepository.GetByNameAsync(request.Name, context.CancellationToken);
        var mem = new List<Biologist>();
        lock (_biologists)
        {
            mem = _biologists.Where(t => t.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        var toStream = (biologists != null && biologists.Any()) ? biologists : mem;

        foreach (var biologist in toStream)
        {
            if (context.CancellationToken.IsCancellationRequested)
                break;
            await responseStream.WriteAsync(biologist.ToResponse());
        }
    }
}