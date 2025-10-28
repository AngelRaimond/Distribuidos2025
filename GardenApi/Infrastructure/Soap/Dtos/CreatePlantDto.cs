using System.Runtime.Serialization;

namespace GardenApi.Infrastructure.Soap.Dtos;

[DataContract(Name = "CreatePlantDto", Namespace = "http://plant-api/plant-service")]
public class CreatePlantDto
{
    [DataMember(Order = 1)]
    public string Name { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string ScientificName { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string Family { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public DataDto Data { get; set; } = new();
}

[DataContract(Name = "DataDto", Namespace = "http://plant-api/plant-service")]
public class DataDto
{
    [DataMember(Order = 1)]
    public int MaxHeight { get; set; }

    [DataMember(Order = 2)]
    public int MaxAge { get; set; }

    [DataMember(Order = 3)]
    public int ConservationLevel { get; set; }
}
