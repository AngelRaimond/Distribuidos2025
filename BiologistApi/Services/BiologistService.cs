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

    public BiologistService(IBiologistRepository biologistRepository)
    {
        _biologistRepository = biologistRepository;
    }

    public override async Task<BiologistResponse> GetBiologistById(BiologistByIdRequest request, ServerCallContext context)
    {
        var biologist = await _biologistRepository.GetByIdAsync(request.Id, context.CancellationToken);

        if (biologist is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Biologist with ID {request.Id} not found"));

        return biologist.ToResponse();
    }
    public override async Task<CreateBiologistResponse> CreateBiologist(IAsyncStreamReader<CreateBiologistRequest> requestStream, ServerCallContext context)
    {
        var createdBiologists = new List<BiologistResponse>();
        while (await requestStream.MoveNext(context.CancellationToken))
        {
            var request = requestStream.Current;
            var biologist = request.ToModel();
            var biologistExist = await _biologistRepository.GetByIdAsync(biologist.Id, context.CancellationToken);
            if (biologistExist != null)
                continue;

            var createdBiologist = await _biologistRepository.CreateAsync(biologist, context.CancellationToken);
            createdBiologists.Add(createdBiologist.ToResponse());
        }

        return new CreateBiologistResponse
        {
            SuccessCount = createdBiologists.Count,
            Biologists = { createdBiologists },
        };
    }

    public override async Task GetAllBiologistsByName(BiologistsByNameRequest request, IServerStreamWriter<BiologistResponse> responseStream, ServerCallContext context)
    {
        var biologists = await _biologistRepository.GetByNameAsync(request.Name, context.CancellationToken);
        if (biologists != null)
        {
            foreach (var biologist in biologists)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    break;
                await responseStream.WriteAsync(biologist.ToResponse());
                await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);
            }
        }
    }
}