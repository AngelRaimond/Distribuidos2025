using MongoDB.Bson.Serialization.Attributes;

namespace BiologistApi.Infrastructure.Documents;

public class AreaDocument
{
    [BsonElement("area")]
    public string Area { get; set; } = null!;

    [BsonElement("subArea")]
    public SubAreaMongo SubArea { get; set; }
}
