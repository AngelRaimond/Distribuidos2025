using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel.Channels;

namespace GardenApi.Infrastructure.Soap;

// Usamos XmlSerializer (más tolerante) y contratos en formato PLANO
[ServiceContract(Name = "PlantService", Namespace = "http://plant-api/plant-service")]
[XmlSerializerFormat(SupportFaults = true)]
public interface IPlantSoapClient
{
    [OperationContract(Action = "http://plant-api/plant-service/PlantService/CreatePlant", ReplyAction = "*")]
    Task<PlantSoapResponse> CreatePlant(CreatePlantSoapRequest plant, CancellationToken cancellationToken);

    [OperationContract(Action = "http://plant-api/plant-service/PlantService/GetPlantById", ReplyAction = "*")]
    Task<PlantSoapResponse> GetPlantById(Guid id, CancellationToken cancellationToken);

    // ⚠️ Cambiado: IList<T> -> T[] para que XmlSerializer pueda reflejar el contrato
    [OperationContract(Action = "http://plant-api/plant-service/PlantService/GetPlantByName", ReplyAction = "*")]
    Task<PlantSoapResponse[]> GetPlantByName(string name, CancellationToken cancellationToken);

    [OperationContract(Action = "http://plant-api/plant-service/PlantService/DeletePlant", ReplyAction = "*")]
    Task<DeletePlantSoapResponse> DeletePlant(Guid id, CancellationToken cancellationToken);

    [OperationContract(Action = "http://plant-api/plant-service/PlantService/UpdatePlant", ReplyAction = "*")]
    Task<PlantSoapResponse> UpdatePlant(UpdatePlantSoapRequest plant, CancellationToken cancellationToken);
}

// ====== DataContracts SOAP (PLANO) ======

[DataContract(Name = "CreatePlantDto", Namespace = "http://plant-api/plant-service")]
public class CreatePlantSoapRequest
{
    [DataMember(Order = 1)] public string Name { get; set; } = string.Empty;
    [DataMember(Order = 2)] public string ScientificName { get; set; } = string.Empty;
    [DataMember(Order = 3)] public string Family { get; set; } = string.Empty;
    [DataMember(Order = 4)] public int MaxHeight { get; set; }
    [DataMember(Order = 5)] public int MaxAge { get; set; }
    [DataMember(Order = 6)] public int ConservationLevel { get; set; }
}

[DataContract(Name = "UpdatePlantDto", Namespace = "http://plant-api/plant-service")]
public class UpdatePlantSoapRequest
{
    [DataMember(Order = 1)] public Guid Id { get; set; }
    [DataMember(Order = 2)] public string Name { get; set; } = string.Empty;
    [DataMember(Order = 3)] public string ScientificName { get; set; } = string.Empty;
    [DataMember(Order = 4)] public string Family { get; set; } = string.Empty;
    [DataMember(Order = 5)] public int MaxHeight { get; set; }
    [DataMember(Order = 6)] public int MaxAge { get; set; }
    [DataMember(Order = 7)] public int ConservationLevel { get; set; }
}

[DataContract(Name = "PlantResponseDto", Namespace = "http://plant-api/plant-service")]
public class PlantSoapResponse
{
    [DataMember(Order = 1)] public Guid Id { get; set; }
    [DataMember(Order = 2)] public string Name { get; set; } = string.Empty;
    [DataMember(Order = 3)] public string ScientificName { get; set; } = string.Empty;
    [DataMember(Order = 4)] public string Family { get; set; } = string.Empty;
    [DataMember(Order = 5)] public int MaxHeight { get; set; }
    [DataMember(Order = 6)] public int MaxAge { get; set; }
    [DataMember(Order = 7)] public int ConservationLevel { get; set; }
}

[DataContract(Name = "DeletePlantResponseDto", Namespace = "http://plant-api/plant-service")]
public class DeletePlantSoapResponse
{
    [DataMember(Order = 1)] public string Message { get; set; } = string.Empty;
}
