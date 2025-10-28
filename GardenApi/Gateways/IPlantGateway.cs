using GardenApi.Dtos;
using GardenApi.Models;

namespace GardenApi.Gateways;

public interface IPlantGateway
{
    Task<Plant?> GetPlantByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IList<Plant>> GetPlantsByNameAsync(string name, CancellationToken cancellationToken);
    Task<Plant> CreatePlantAsync(CreatePlantRequest request, CancellationToken cancellationToken);
    Task<Plant> UpdatePlantAsync(UpdatePlantRequest request, CancellationToken cancellationToken);
    Task DeletePlantAsync(Guid id, CancellationToken cancellationToken);
}
