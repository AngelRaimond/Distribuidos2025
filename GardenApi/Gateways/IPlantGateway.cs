using GardenApi.Dtos;

namespace GardenApi.Gateways;

public interface IPlantGateway
{
    Task<PlantResponse?> GetPlantByIdAsync(Guid id, CancellationToken cancellationToken);
}