using System.ServiceModel;
using PlantApi.Dtos;

namespace PlantApi.Validators;

public static class PlantValidator
{

    public static CreatePlantDto ValidateName(this CreatePlantDto plant) =>
    string.IsNullOrEmpty(plant.Name)
        ? throw new FaultException("Plant name is required")
        : plant;

    public static CreatePlantDto ValidateScientificName(this CreatePlantDto plant) =>
    string.IsNullOrEmpty(plant.Family)
        ? throw new FaultException("Plant Scientific Name is required")
        : plant;

    public static CreatePlantDto ValidateFamily(this CreatePlantDto plant) =>
    string.IsNullOrEmpty(plant.Family)
        ? throw new FaultException("Plant Family is required")
        : plant;
}
