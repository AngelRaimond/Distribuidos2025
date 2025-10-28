using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TrainerApi.Infrastructure.Documents;

public class TrainerDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!; 
    [BsonElement("name")]
    public string Name { get; set; } = null!; 
    [BsonElement("age")]
    public int Age { get; set; }
    [BsonElement("birthDate")]
    public DateTime BirthDate { get; set; }
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
    [BsonElement("medals")]
    public List<MedalDocument> Medals { get; set; } = new(); 
}