using MongoDB.Bson.Serialization.Attributes;

namespace TrainerApi.Infrastructure.Documents;

public class MedalDocument
{
    [BsonElement("region")]
    public string Region { get; set; } = null!;
    [BsonElement("type")]
    public MedalType Type { get; set; } = MedalType.Unknown;
}
