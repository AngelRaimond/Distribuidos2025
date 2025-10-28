using System;
using System.Runtime.Serialization;

namespace GardenApi.Infrastructure.Soap.Dtos;

[DataContract(Name = "PlantResponseDto", Namespace = "http://plant-api/plant-service")]
public class PlantResponseDto
{
    [DataMember(Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Order = 2)]
    public string Name { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string ScientificName { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public string Family { get; set; } = string.Empty;

    [DataMember(Order = 5)]
    public DataDto Data { get; set; } = new();
}
