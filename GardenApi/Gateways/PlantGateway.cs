using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// Ajusta estos namespaces a tu solución
using GardenApi.Gateways;
using GardenApi.Models;                 // Plant (modelo REST si lo usas en el retorno)
using GardenApi.Contracts;              // CreatePlantRequest, UpdatePlantRequest, etc. (REST DTOs)
using GardenApi.Infrastructure.Soap;    // IPlantService, CreatePlantDto, UpdatePlantDto, PlantDataDto, PlantSoapResponse, etc.

namespace GardenApi.Gateways
{
    public interface IPlantGateway
    {
        Task<Plant> GetPlantByIdAsync(Guid id, CancellationToken ct);
        Task<IList<Plant>> GetPlantsByNameAsync(string name, CancellationToken ct);
        Task<Plant> CreatePlantAsync(CreatePlantRequest request, CancellationToken ct);
        Task<Plant> UpdatePlantAsync(UpdatePlantRequest request, CancellationToken ct);
        Task DeletePlantAsync(Guid id, CancellationToken ct);
    }

    public class PlantGateway : IPlantGateway
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PlantGateway> _logger;
        private readonly string _soapUrl;

        public PlantGateway(IConfiguration configuration, ILogger<PlantGateway> logger)
        {
            _config = configuration;
            _logger = logger;

            // Orden de resolución del endpoint:
            // 1) config["PlantService:Url"]
            // 2) Environment.GetEnvironmentVariable("PLANT_SOAP_URL")
            // 3) fallback dev
            _soapUrl =
                _config["PlantService:Url"]
                ?? Environment.GetEnvironmentVariable("PLANT_SOAP_URL")
                ?? "http://localhost:8082/PlantService.svc";

            _logger.LogInformation("g      PlantGateway: usando endpoint SOAP explícito (sin WSDL): {url}", _soapUrl);
        }

        // ------------------------
        // Cliente SOAP (sin WSDL)
        // ------------------------
        private IPlantService CreateClient()
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
            {
                MaxReceivedMessageSize = 10 * 1024 * 1024,
                ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max,
                MessageEncoding = WSMessageEncoding.Text,
                TextEncoding = System.Text.Encoding.UTF8
            };

            // SOAP 1.1 (compatible con SoapCore BasicHttpBinding)
            binding.MessageVersion = MessageVersion.Soap11;

            var endpoint = new EndpointAddress(_soapUrl);
            var factory = new ChannelFactory<IPlantService>(binding, endpoint);

            // (Opcional) timeouts
            factory.OpenTimeout = TimeSpan.FromSeconds(15);
            factory.CloseTimeout = TimeSpan.FromSeconds(15);
            factory.Endpoint.Binding.SendTimeout = TimeSpan.FromSeconds(30);
            factory.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromSeconds(30);

            return factory.CreateChannel();
        }

        // ------------------------
        // Helpers de mapeo
        // ------------------------
        private static Plant MapSoapToRest(PlantSoapResponse s)
        {
            if (s == null) return null;

            return new Plant
            {
                Id = s.Id,
                Name = s.Name,
                ScientificName = s.ScientificName,
                Family = s.Family,
                Data = new PlantData
                {
                    MaxHeight = s.Data?.MaxHeight ?? s.MaxHeight,
                    MaxAge = s.Data?.MaxAge ?? s.MaxAge,
                    ConservationLevel = s.Data?.ConservationLevel ?? s.ConservationLevel
                }
            };
        }

        private static IList<Plant> MapSoapListToRest(IList<PlantSoapResponse> list)
        {
            var result = new List<Plant>();
            if (list == null) return result;
            foreach (var s in list)
                result.Add(MapSoapToRest(s));
            return result;
        }

        // ------------------------
        // Implementación IPlantGateway
        // ------------------------
        public async Task<Plant> GetPlantByIdAsync(Guid id, CancellationToken ct)
        {
            var client = CreateClient();
            var soap = await client.GetPlantByIdAsync(id.ToString());
            return MapSoapToRest(soap);
        }

        public async Task<IList<Plant>> GetPlantsByNameAsync(string name, CancellationToken ct)
        {
            var client = CreateClient();
            var soapList = await client.GetPlantByNameAsync(name ?? string.Empty);
            return MapSoapListToRest(soapList);
        }

        public async Task<Plant> CreatePlantAsync(CreatePlantRequest req, CancellationToken ct)
        {
            var client = CreateClient();

            // ⚠️ Siempre inicializa Data para no mandar null al SOAP
            var dto = new CreatePlantDto
            {
                Name = req.Name,
                ScientificName = req.ScientificName,
                Family = req.Family,

                // Si tu contrato SOAP además tiene planos (MaxHeight/MaxAge/ConservationLevel) mantenemos compatibilidad
                MaxHeight = req.Data?.MaxHeight ?? req.MaxHeight,
                MaxAge = req.Data?.MaxAge ?? req.MaxAge,
                ConservationLevel = req.Data?.ConservationLevel ?? req.ConservationLevel,

                Data = new PlantDataDto
                {
                    MaxHeight = req.Data?.MaxHeight ?? req.MaxHeight,
                    MaxAge = req.Data?.MaxAge ?? req.MaxAge,
                    ConservationLevel = req.Data?.ConservationLevel ?? req.ConservationLevel
                }
            };

            try
            {
                var created = await client.CreatePlantAsync(dto);
                return MapSoapToRest(created);
            }
            catch (FaultException ex)
            {
                _logger.LogWarning(ex, "SOAP Fault al crear planta: {msg}", ex.Message);
                // Deja que tu controller traduzca FaultException a 409/400/404 según texto
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en CreatePlant contra {url}", _soapUrl);
                throw;
            }
        }

        public async Task<Plant> UpdatePlantAsync(UpdatePlantRequest req, CancellationToken ct)
        {
            var client = CreateClient();

            var dto = new UpdatePlantDto
            {
                Id = req.Id?.ToString(),
                Name = req.Name,
                ScientificName = req.ScientificName,
                Family = req.Family,

                MaxHeight = req.Data?.MaxHeight ?? req.MaxHeight,
                MaxAge = req.Data?.MaxAge ?? req.MaxAge,
                ConservationLevel = req.Data?.ConservationLevel ?? req.ConservationLevel,

                Data = new PlantDataDto
                {
                    MaxHeight = req.Data?.MaxHeight ?? req.MaxHeight,
                    MaxAge = req.Data?.MaxAge ?? req.MaxAge,
                    ConservationLevel = req.Data?.ConservationLevel ?? req.ConservationLevel
                }
            };

            try
            {
                var updated = await client.UpdatePlantAsync(dto);
                return MapSoapToRest(updated);
            }
            catch (FaultException ex)
            {
                _logger.LogWarning(ex, "SOAP Fault en UpdatePlant: {msg}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en UpdatePlant contra {url}", _soapUrl);
                throw;
            }
        }

        public async Task DeletePlantAsync(Guid id, CancellationToken ct)
        {
            var client = CreateClient();

            try
            {
                await client.DeletePlantAsync(id.ToString());
            }
            catch (FaultException ex)
            {
                _logger.LogWarning(ex, "SOAP Fault en DeletePlant: {msg}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en DeletePlant contra {url}", _soapUrl);
                throw;
            }
        }
    }
}
