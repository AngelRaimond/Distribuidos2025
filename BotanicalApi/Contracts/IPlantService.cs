using System.ServiceModel;
using PlantApi.Dtos;

namespace PlantApi.Services;

[ServiceContract(Name = "PlantService", Namespace = "http://plant-api/plant-service")]
public interface IPlantService
{
    [OperationContract]
    Task<PlantResponseDto> CreatePlant(CreatePlantDto plant, CancellationToken cancellationToken);

    [OperationContract]
    Task<PlantResponseDto> GetPlantById(Guid id, CancellationToken cancellationToken);

    [OperationContract]
    Task<PlantResponseDto> GetPlantByFamily(string family, CancellationToken cancellationToken);

    [OperationContract]
    Task<IList<PlantResponseDto>> GetPlantByName(string name, CancellationToken cancellationToken);

    [OperationContract]
    Task<DeletePlantResponseDto> DeletePlant(Guid id, CancellationToken cancellationToken);

    [OperationContract]
    Task<PlantResponseDto> UpdatePlant(UpdatePlantDto plant, CancellationToken cancellationToken);
}