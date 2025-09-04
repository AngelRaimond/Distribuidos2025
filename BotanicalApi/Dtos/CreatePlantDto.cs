using System.Runtime.Serialization;

namespace PlantApi.Dtos;

[DataContract(Name = "CreatePlantDto", Namespace = "http://plant-api/plant-service")]
public class CreatePlantDto
{
    [DataMember(Name = "Name", Order = 1)]
    public string? Name { get; set; }

    [DataMember(Name = "ScientificName", Order = 2)]
    public string? ScientificName { get; set; }

    [DataMember(Name = "Family", Order = 3)]
    public string Family { get; set; }

    [DataMember(Name = "Data", Order = 4)]
    public required DataDto Data { get; set; }
}