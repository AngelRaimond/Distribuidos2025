using System.Runtime.Serialization;
namespace PlantApi.Dtos;

[DataContract(Name = "PlantResponseDto", Namespace = "http://plant-api/plant-service")]
public class PlantResponseDto
{
    [DataMember(Name = "Id", Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Name = "Name", Order = 2)]
    public required string Name { get; set; }

    [DataMember(Name = "ScientificName", Order = 3)]
    public required string ScientificName { get; set; }

    [DataMember(Name = "Family", Order = 4)]
    public required string Family { get; set; }

    [DataMember(Name = "Data", Order = 5)]
    public required DataDto Data { get; set; }
}
