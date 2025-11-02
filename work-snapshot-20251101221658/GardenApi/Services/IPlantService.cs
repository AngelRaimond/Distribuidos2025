using GardenApi.Dtos;

namespace GardenApi.Services;

public interface IPlantService
{
    Task<PlantResponse?> GetPlantByIdAsync(Guid id, CancellationToken cancellationToken);
}