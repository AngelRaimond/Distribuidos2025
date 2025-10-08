using System.Runtime.Serialization;

namespace GardenApi.Infrastructure.Soap.Dtos;

[DataContract(Name = "DeletePlantResponseDto", Namespace = "http://plant-api/plant-service")]
public class DeletePlantResponseDto
{
    [DataMember(Order = 1)]
    public string Message { get; set; } = string.Empty;
}
