using PlantApi.Models;

namespace PlantApi.Repositories;

public interface IPlantRepository
{
    Task<Plant> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<Plant> CreateAsync(Plant plant, CancellationToken cancellationToken);

    Task<Plant?> GetPlantByIdAsync(Guid id, CancellationToken cancellationToken);

    
    Task<Plant?> GetPlantsByFamilyAsync(string family, CancellationToken cancellationToken);

    Task<IReadOnlyList<Plant>> GetPlantsByNameAsync(string name, CancellationToken cancellationToken);

    Task DeletePlantAsync(Plant plant, CancellationToken cancellationToken);

    Task UpdatePlantAsync(Plant plant, CancellationToken cancellationToken);
}