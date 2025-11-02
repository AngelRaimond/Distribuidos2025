using GardenApi.Gateways;
using GardenApi.Dtos;

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
        return await _plantGateway.GetPlantByIdAsync(id, cancellationToken);
    }
}