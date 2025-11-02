using PlantApi.Dtos;
using PlantApi.Repositories;
using System.ServiceModel;
using PlantApi.Mappers;
using PlantApi.Validators;
using PlantApi.Models;


namespace PlantApi.Services;

public class PlantService : IPlantService
{
    private readonly IPlantRepository _plantRepository;

    public PlantService(IPlantRepository plantRepository)
    {
        _plantRepository = plantRepository;
    }

    public async Task<PlantResponseDto> UpdatePlant(UpdatePlantDto plantToUpdate, CancellationToken cancellationToken)
    {
        var plant = await _plantRepository.GetPlantByIdAsync(plantToUpdate.Id, cancellationToken);
        if (!PlantExists(plant))
        {
            throw new FaultException(reason: "Plant not found");
        }

        if (!await IsPlantAllowedToBeUpdated(plantToUpdate, cancellationToken))
        {
            throw new FaultException("Plant already exists");
        }

        plant.Name = plantToUpdate.Name;
        plant.ScientificName = plantToUpdate.ScientificName;
        plant.Data.MaxHeight = plantToUpdate.Data.MaxHeight;
        plant.Data.MaxAge = plantToUpdate.Data.MaxAge;
        plant.Data.ConservationLevel = plantToUpdate.Data.ConservationLevel;

        await _plantRepository.UpdatePlantAsync(plant, cancellationToken);
        return plant.ToResponseDto();
    }

    private async Task<bool> IsPlantAllowedToBeUpdated(UpdatePlantDto plantToUpdate, CancellationToken cancellationToken)
    {
        var duplicatedPlant = await _plantRepository.GetByNameAsync(plantToUpdate.Name, cancellationToken);
        return duplicatedPlant is null || IsTheSamePlant(duplicatedPlant, plantToUpdate);
    }

    private static bool IsTheSamePlant(Plant plant, UpdatePlantDto plantToUpdate)
    {
        return plant.Id == plantToUpdate.Id;
    }

    public async Task<DeletePlantResponseDto> DeletePlant(Guid id, CancellationToken cancellationToken)
    {
        var plant = await _plantRepository.GetPlantByIdAsync(id, cancellationToken);
        if (!PlantExists(plant))
        {
            throw new FaultException(reason: "Plant not found");
        }
        await _plantRepository.DeletePlantAsync(plant, cancellationToken);
        return new DeletePlantResponseDto { Success = true };
    }

    public async Task<IList<PlantResponseDto>> GetPlantByName(string name, CancellationToken cancellationToken)
    {
        var plants = await _plantRepository.GetPlantsByNameAsync(name, cancellationToken);
        return plants.ToResponseDto();
    }

    public async Task<PlantResponseDto> GetPlantById(Guid id, CancellationToken cancellationToken)
    {
        var plant = await _plantRepository.GetPlantByIdAsync(id, cancellationToken);
        return PlantExists(plant) ? plant.ToResponseDto() : throw new FaultException("Plant not found");

    }
    
    public async Task<PlantResponseDto> GetPlantByFamily(string family, CancellationToken cancellationToken)
    {
        var plant = await _plantRepository.GetPlantsByFamilyAsync(family, cancellationToken);
        return PlantExists(plant) ? plant.ToResponseDto() : throw new FaultException("Plant not found");

    }
    public async Task<PlantResponseDto> CreatePlant(CreatePlantDto plantRequest, CancellationToken cancellationToken)
    {

        plantRequest
            .ValidateName()
            .ValidateScientificName()
            .ValidateFamily();

        if (await IsPlantDuplicated(plantRequest.Name, cancellationToken))
        {
            throw new FaultException("Plant already exists");
        }

        var plant = await _plantRepository.CreateAsync(plantRequest.ToModel(), cancellationToken);

        return plant.ToResponseDto();
    }

    private static bool PlantExists(Plant? plant)
    {
        return plant is not null;
    }

    private async Task<bool> IsPlantDuplicated(string name, CancellationToken cancellationToken)
    {
        var plant = await _plantRepository.GetByNameAsync(name, cancellationToken);
        return plant is not null;
    }
}