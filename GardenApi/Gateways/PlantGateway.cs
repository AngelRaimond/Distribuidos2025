using System.ServiceModel;
using GardenApi.Dtos;
using GardenApi.Infrastructure.Soap;

namespace GardenApi.Gateways;

public class PlantGateway : IPlantGateway
{
    private readonly IPlantSoapClient _soapClient;

    public PlantGateway(IConfiguration configuration)
    {
        var binding = new BasicHttpBinding();
        var endpoint = new EndpointAddress(configuration.GetValue<string>("PlantService:Url")!);
        _soapClient = new ChannelFactory<IPlantSoapClient>(binding, endpoint).CreateChannel();
    }

    public async Task<PlantResponse?> GetPlantByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var soapResponse = await _soapClient.GetPlantById(id, cancellationToken);
            
            return new PlantResponse
            {
                Id = soapResponse.Id,
                Name = soapResponse.Name,
                ScientificName = soapResponse.ScientificName,
                Family = soapResponse.Family,
                Data = new PlantDataResponse
                {
                    MaxHeight = soapResponse.Data.MaxHeight,
                    MaxAge = soapResponse.Data.MaxAge,
                    ConservationLevel = soapResponse.Data.ConservationLevel
                }
            };
        }
        catch (FaultException ex) when (ex.Message.Contains("Plant not found"))
        {
            return null;
        }
        catch (Exception)
        {
            throw;
        }
    }
}