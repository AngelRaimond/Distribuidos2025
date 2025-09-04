using PlantApi.Models;
using PlantApi.Infrastructure;
using PlantApi.Dtos;
using PlantApi.Infrastructure.Entities;


namespace PlantApi.Mappers;

public static class PlantMapper
{
    //Extension method
    public static Plant ToModel(this PlantEntity plantEntity)
    {
        if (plantEntity is null)
        {
            return null;
        }

        return new Plant
        {
            Id = plantEntity.Id,
            Name = plantEntity.Name,
            ScientificName = plantEntity.ScientificName,
            Family = plantEntity.Family,
            Data = new Data
            {
                MaxHeight = plantEntity.MaxHeight,
                MaxAge = plantEntity.MaxAge,
                ConservationLevel = plantEntity.ConservationLevel
            }
        };
    }


    public static PlantEntity ToEntity(this Plant plant)
    {
        return new PlantEntity
        {
            Id = plant.Id,
            Name = plant.Name,
            ScientificName = plant.ScientificName,
            Family = plant.Family,
            MaxHeight = plant.Data.MaxHeight,
            MaxAge = plant.Data.MaxAge,
            ConservationLevel = plant.Data.ConservationLevel
        };
    }

    public static Plant ToModel(this CreatePlantDto requestPlantDto)
    {
        return new Plant
        {
            Name = requestPlantDto.Name,
            ScientificName = requestPlantDto.ScientificName,
            Family = requestPlantDto.Family,
            Data = new Data
            {
                MaxHeight = requestPlantDto.Data.MaxHeight,
                MaxAge = requestPlantDto.Data.MaxAge,
                ConservationLevel = requestPlantDto.Data.ConservationLevel
            }
        };
    }

    public static PlantResponseDto ToResponseDto(this Plant plant)
    {
        return new PlantResponseDto
        {
            Id = plant.Id,
            Name = plant.Name,
            ScientificName = plant.ScientificName,
            Family = plant.Family,
            Data = new DataDto
            {
                MaxHeight = plant.Data.MaxHeight,
                MaxAge = plant.Data.MaxAge,
                ConservationLevel = plant.Data.ConservationLevel
            }

        };
    }

    public static IList<PlantResponseDto> ToResponseDto(this IReadOnlyList<Plant> plants)
    {
        return plants.Select(s => s.ToResponseDto()).ToList();
    }

    public static IReadOnlyList<Plant> ToModel(this IReadOnlyList<PlantEntity> plants)
    {
        return plants.Select(s => s.ToModel()).ToList();
    }
}

