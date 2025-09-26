using System.ServiceModel;
using System.Runtime.Serialization;

namespace GardenApi.Infrastructure.Soap;

[ServiceContract(Name = "PlantService", Namespace = "http://plant-api/plant-service")]
public interface IPlantSoapClient
{
    [OperationContract]
    Task<PlantSoapResponse> GetPlantById(Guid id, CancellationToken cancellationToken);
}

[DataContract(Name = "PlantResponseDto", Namespace = "http://plant-api/plant-service")]
public class PlantSoapResponse
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
    public required DataSoapResponse Data { get; set; }
}

[DataContract(Name = "DataDto", Namespace = "http://plant-api/plant-service")]
public class DataSoapResponse
{
    [DataMember(Name = "MaxHeight", Order = 1)]
    public int MaxHeight { get; set; }

    [DataMember(Name = "MaxAge", Order = 2)]
    public int MaxAge { get; set; }

    [DataMember(Name = "ConservationLevel", Order = 3)]
    public int ConservationLevel { get; set; }
}
