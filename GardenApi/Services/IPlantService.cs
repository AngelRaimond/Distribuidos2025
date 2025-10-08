using GardenApi.Dtos;
using GardenApi.Models;

namespace GardenApi.Services;

public interface IPlantService
{
    Task<PlantResponse?> GetPlantByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PaginatedResponse<PlantResponse>> GetPlantsAsync(string? name, int page, int pageSize, CancellationToken cancellationToken);
    Task<PlantResponse> CreatePlantAsync(CreatePlantRequest request, CancellationToken cancellationToken);
    Task<PlantResponse> UpdatePlantAsync(UpdatePlantRequest request, CancellationToken cancellationToken);
    Task DeletePlantAsync(Guid id, CancellationToken cancellationToken);
}
