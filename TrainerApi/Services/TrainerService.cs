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

    public TrainerService(ITrainerRepository trainerRepository)
    {
        _trainerRepository = trainerRepository;
    }

    public override async Task<TrainerResponse> GetTrainerById(TrainerByIdRequest request, ServerCallContext context)
    {
        var trainer = await _trainerRepository.GetByIdAsync(request.Id, context.CancellationToken);

        if (trainer is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Trainer with ID {request.Id} not found"));

        return trainer.ToResponse();
    }
    public override async Task<CreateTrainerResponse> CreateTrainer(IAsyncStreamReader<CreateTrainerRequest> requestStream, ServerCallContext context)
    {
        var createdTrainers = new List<TrainerResponse>();
        while (await requestStream.MoveNext(context.CancellationToken))
        {
            var request = requestStream.Current;
            var trainer = request.ToModel();
            var trainerExist = await _trainerRepository.GetByIdAsync(trainer.Id, context.CancellationToken);
            if (trainerExist != null)
                continue;

            var createdTrainer = await _trainerRepository.CreateAsync(trainer, context.CancellationToken);
            createdTrainers.Add(createdTrainer.ToResponse());
        }

        return new CreateTrainerResponse
        {
            SuccessCount = createdTrainers.Count,
            Trainers = { createdTrainers },
        };
    }

    public override async Task GetAllTrainersByName(TrainersByNameRequest request, IServerStreamWriter<TrainerResponse> responseStream, ServerCallContext context)
    {
        var trainers = await _trainerRepository.GetByNameAsync(request.Name, context.CancellationToken);
        if (trainers != null)
        {
            foreach (var trainer in trainers)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    break;
                await responseStream.WriteAsync(trainer.ToResponse());
                await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);
            }
        }
    }
}