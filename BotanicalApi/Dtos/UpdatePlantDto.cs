using System.Runtime.Serialization;

namespace PlantApi.Dtos;

[DataContract(Name = "UpdatePlantDto", Namespace = "http://pokemon-api/pokemon-dto")]
public class UpdatePlantDto
{
    [DataMember(Name = "Id", Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Name = "Name", Order = 2)]
    public string Name { get; set; }

    [DataMember(Name = "ScientificName", Order = 3)]
    public string? ScientificName { get; set; }

    [DataMember(Name = "Family", Order = 4)]
    public string Family { get; set; }

    [DataMember(Name = "Data", Order = 5)]
    public DataDto Data { get; set; }
}

