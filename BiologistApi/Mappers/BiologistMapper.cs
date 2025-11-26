using System.Linq;
using BiologistApi.Infrastructure.Documents;
using BiologistApi.Models;
using Google.Protobuf.WellKnownTypes;
// Aliases to disambiguate generated protobuf types vs domain models
using DomainArea = BiologistApi.Models.AreaModel;
using DomainSubArea = BiologistApi.Models.SubArea;
using ProtoArea = global::BiologistApi.Area;
using ProtoSubArea = global::BiologistApi.SubArea;

namespace BiologistApi.Mappers;

public static class BiologistMapper
{
    public static Biologist ToModel(this BiologistDocument document)
    {
        return new Biologist
        {
            Id = document.Id,
            Name = document.Name,
            Age = document.Age,
            BirthDate = document.BirthDate,
            CreatedAt = document.CreatedAt,
            Areas = document.Areas.Select(a => a.ToDomain()).ToList()
        };
    }

    public static BiologistDocument ToDocument(this Biologist model)
    {
        return new BiologistDocument
        {
            Id = model.Id,
            Name = model.Name,
            Age = model.Age,
            BirthDate = model.BirthDate,
            CreatedAt = model.CreatedAt,
            Areas = model.Areas.Select(a => a.ToDocument()).ToList()
        };
    }

    // Document <-> Domain mappings
    public static DomainArea ToDomain(this AreaDocument document)
    {
        return new DomainArea
        {
            Area = document.Area,
            SubArea = (DomainSubArea)(int)document.SubArea
        };
    }

    public static AreaDocument ToDocument(this DomainArea model)
    {
        return new AreaDocument
        {
            Area = model.Area,
            SubArea = (SubAreaMongo)(int)model.SubArea
        };
    }

    public static BiologistResponse ToResponse(this Biologist model)
    {
        return new BiologistResponse
        {
            Id = model.Id,
            Name = model.Name,
            Age = model.Age,
            BirthDate = Timestamp.FromDateTime(DateTime.SpecifyKind(model.BirthDate, DateTimeKind.Utc)),
            Areas = { model.Areas.Select(a => a.ToProto()) }
        };
    }

    public static Biologist ToModel(this CreateBiologistRequest request)
    {
        return new Biologist
        {
            Id = string.Empty,
            Name = request.Name,
            Age = request.Age,
            BirthDate = request.BirthDate != null ? request.BirthDate.ToDateTime() : DateTime.MinValue,
            CreatedAt = DateTime.UtcNow,
            Areas = request.Areas != null ? request.Areas.Select(a => a.ToDomain()).ToList() : new System.Collections.Generic.List<DomainArea>()
        };
    }

    // Proto <-> Domain mappings
    public static DomainArea ToDomain(this ProtoArea request)
    {
        return new DomainArea
        {
            Area = request.Area_,
            SubArea = (DomainSubArea)(int)request.SubArea
        };
    }

    public static ProtoArea ToProto(this DomainArea model)
    {
        return new ProtoArea
        {
            Area_ = model.Area,
            SubArea = (ProtoSubArea)(int)model.SubArea
        };
    }

    public static Biologist ToModel(this UpdateBiologistRequest request)
    {
        return new Biologist
        {
            Id = request.Id,
            Name = request.Name,
            Age = request.Age,
            BirthDate = request.BirthDate != null ? request.BirthDate.ToDateTime() : DateTime.MinValue,
            CreatedAt = DateTime.UtcNow,
            Areas = request.Areas != null ? request.Areas.Select(a => a.ToDomain()).ToList() : new System.Collections.Generic.List<DomainArea>()
        };
    }
}
