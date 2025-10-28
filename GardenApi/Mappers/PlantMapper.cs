using GardenApi.Dtos;
using GardenApi.Models;
using GardenApi.Infrastructure.Soap;

namespace GardenApi.Mappers;

public static class PlantMapper
{
    public static PlantResponse ToResponse(this Plant model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        ScientificName = model.ScientificName,
        Family = model.Family,
        Data = new PlantDataResponse
        {
            MaxHeight = model.Data.MaxHeight,
            MaxAge = model.Data.MaxAge,
            ConservationLevel = model.Data.ConservationLevel
        }
    };

    // Del SOAP (PLANO) -> Dominio (anidado)
    public static Plant ToModel(this PlantSoapResponse dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        ScientificName = dto.ScientificName,
        Family = dto.Family,
        Data = new PlantData
        {
            MaxHeight = dto.MaxHeight,
            MaxAge = dto.MaxAge,
            ConservationLevel = dto.ConservationLevel
        }
    };

    // Del REST (anidado) -> SOAP (PLANO)
    public static CreatePlantSoapRequest ToCreateSoap(this CreatePlantRequest req) => new()
    {
        Name = req.Name,
        ScientificName = req.ScientificName,
        Family = req.Family,
        MaxHeight = req.Data.MaxHeight,
        MaxAge = req.Data.MaxAge,
        ConservationLevel = req.Data.ConservationLevel
    };

    public static UpdatePlantSoapRequest ToUpdateSoap(this UpdatePlantRequest req) => new()
    {
        Id = req.Id,
        Name = req.Name,
        ScientificName = req.ScientificName,
        Family = req.Family,
        MaxHeight = req.Data.MaxHeight,
        MaxAge = req.Data.MaxAge,
        ConservationLevel = req.Data.ConservationLevel
    };
}
