using System.Linq;
using TrainerApi.Infrastructure.Documents;
using TrainerApi.Models;
using Google.Protobuf.WellKnownTypes;


namespace TrainerApi.Mappers;

public static class TrainerMapper
{
    public static Trainer ToModel(this TrainerDocument document)
    {

        return new Trainer
        {
            Id = document.Id,
            Name = document.Name,
            Age = document.Age,
            BirthDate = document.BirthDate,
            CreatedAt = document.CreatedAt,
            Medals = document.Medals.Select(m => m.ToDomain()).ToList()
        };
    }

    public static Models.Medal ToDomain(this MedalDocument document)
    {
        if (document == null)
            return new Models.Medal { Region = string.Empty, Type = Models.MedalType.Unknown };

        return new Models.Medal
        {
            Region = document.Region,
            Type = (Models.MedalType)(int)document.Type
        };
    }

    public static TrainerResponse ToResponse(this Trainer trainer)
    {
        return new TrainerResponse
        {
            Id = trainer.Id,
            Name = trainer.Name,
            Age = trainer.Age,
            BirthDate = Timestamp.FromDateTime(trainer.BirthDate.ToUniversalTime()),
            CreatedAt = Timestamp.FromDateTime(trainer.CreatedAt.ToUniversalTime()),
            Medals = { trainer.Medals.Select(m => m.ToResponse()) }
        };
    }

    public static Trainer ToModel(this CreateTrainerRequest request)
    {
        return new Trainer
        {
            Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
            Name = request.Name,
            Age = request.Age,
            BirthDate = request.BirthDate.ToDateTime(),
            CreatedAt = DateTime.UtcNow,
            Medals = request.Medals.Select(m => m.ToDomain()).ToList()
        };
    }

    public static TrainerDocument ToDocument(this Trainer trainer)
    {
        return new TrainerDocument
        {
            Id = trainer.Id,
            Name = trainer.Name,
            Age = trainer.Age,
            BirthDate = trainer.BirthDate,
            CreatedAt = trainer.CreatedAt,
            Medals = trainer.Medals.Select(m => m.ToDocument()).ToList()
        };
    }
    
    private static MedalDocument ToDocument(this Models.Medal medal)
    {
        return new MedalDocument
        {
            Region = medal.Region,
            Type = (Infrastructure.Documents.MedalType)medal.Type
        };
    }
    private static Models.Medal ToDomain(this Medal request)
    {
        return new Models.Medal
        {
            Region = request.Region,
            Type = (Models.MedalType)(int)request.Type
        };
    }
    public static Medal ToResponse(this Models.Medal medal)
    {
        return new Medal
        {
            Region = medal.Region,
            Type = (MedalType)(int)medal.Type
        };
    }
}