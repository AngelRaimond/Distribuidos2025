using GardenApi.Dtos;
using GardenApi.Gateways;
using GardenApi.Models;
using GardenApi.Mappers;
using GardenApi.Exceptions;

namespace GardenApi.Services;

public class PlantService : IPlantService
{
    private readonly IPlantGateway _plantGateway;

    public PlantService(IPlantGateway plantGateway)
    {
        _plantGateway = plantGateway;
    }

    public async Task<PlantResponse?> GetPlantByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await _plantGateway.GetPlantByIdAsync(id, cancellationToken);
        return model?.ToResponse();
    }

    public async Task<PaginatedResponse<PlantResponse>> GetPlantsAsync(string? name, int page, int pageSize, CancellationToken cancellationToken)
    {
        var list = await _plantGateway.GetPlantsByNameAsync(name ?? string.Empty, cancellationToken);
        var total = list.Count;
        var items = list.Skip((page-1) * pageSize).Take(pageSize).Select(x => x.ToResponse()).ToList();
        return new PaginatedResponse<PlantResponse>{ Page = page, PageSize = pageSize, Total = total, Items = items };
    }

    public async Task<PlantResponse> CreatePlantAsync(CreatePlantRequest request, CancellationToken cancellationToken)
    {
        var created = await _plantGateway.CreatePlantAsync(request, cancellationToken);
        return created.ToResponse();
    }

    public async Task<PlantResponse> UpdatePlantAsync(UpdatePlantRequest request, CancellationToken cancellationToken)
    {
        var model = await _plantGateway.GetPlantByIdAsync(request.Id, cancellationToken);
        if (model is null) throw new PlantNotFoundException();
        var updated = await _plantGateway.UpdatePlantAsync(request, cancellationToken);
        return updated.ToResponse();
    }

    public async Task DeletePlantAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await _plantGateway.GetPlantByIdAsync(id, cancellationToken);
        if (model is null) throw new PlantNotFoundException();
        await _plantGateway.DeletePlantAsync(id, cancellationToken);
    }
}
