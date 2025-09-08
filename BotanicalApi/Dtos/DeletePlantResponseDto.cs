using System.Runtime.Serialization;

namespace PlantApi.Dtos;

[DataContract(Name = "DeletePlantResponseDto", Namespace = "http://plant-api/plant-dto")]
public class DeletePlantResponseDto
{
    [DataMember(Name = "Success", Order = 1)]
    public bool Success { get; set; }
}