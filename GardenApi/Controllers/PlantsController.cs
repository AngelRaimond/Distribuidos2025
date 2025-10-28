using System;
using System.Net.Mime;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using GardenApi.Dtos;
using GardenApi.Gateways;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GardenApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class PlantsController : ControllerBase
    {
        private readonly IPlantGateway _gateway;
        private readonly ILogger<PlantsController> _logger;

        public PlantsController(IPlantGateway gateway, ILogger<PlantsController> logger)
        {
            _gateway = gateway;
            _logger = logger;
        }

        // GET: api/plants/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var plant = await _gateway.GetPlantByIdAsync(id, ct);
            if (plant is null) return NotFound(new { message = "Plant not found.", statusCode = 404 });
            return Ok(plant);
            // Nota: GET ya te funcionaba; lo dejamos igual.
        }

        // GET: api/plants?name=xxx
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByName([FromQuery] string? name, CancellationToken ct)
        {
            var items = await _gateway.GetPlantsByNameAsync(name ?? string.Empty, ct);
            return Ok(items);
        }

        // POST: api/plants
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreatePlantRequest body, CancellationToken ct)
        {
            if (body is null)
                return BadRequest(new { message = "Request body is required.", statusCode = 400 });

            try
            {
                var created = await _gateway.CreatePlantAsync(body, ct);
                // Si tu ruta GET por id es GetById:
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (FaultException fex)
            {
                // Mapeo de Faults conocidas a HTTP correcto
                var msg = (fex.Message ?? string.Empty).Trim();

                // Duplicado -> 409
                if (msg.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { message = "Plant already exists.", statusCode = 409 });

                // Datos inválidos -> 400 (por si el SOAP te regresa validaciones)
                if (msg.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("bad request", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = msg, statusCode = 400 });

                // Otros faults del servicio SOAP -> 502 Bad Gateway
                _logger.LogWarning(fex, "SOAP Fault al crear planta: {Message}", msg);
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Upstream SOAP fault.", detail = msg, statusCode = 502 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating plant.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Unexpected error creating plant.", statusCode = 500 });
            }
        }

        // PUT: api/plants/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlantRequest body, CancellationToken ct)
        {
            if (body is null || body.Id == Guid.Empty || body.Id != id)
                return BadRequest(new { message = "Id in route and body must match.", statusCode = 400 });

            try
            {
                var updated = await _gateway.UpdatePlantAsync(body, ct);
                return Ok(updated);
            }
            catch (FaultException fex)
            {
                var msg = (fex.Message ?? string.Empty).Trim();

                if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = "Plant not found.", statusCode = 404 });

                if (msg.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { message = "Plant already exists.", statusCode = 409 });

                if (msg.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("bad request", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = msg, statusCode = 400 });

                _logger.LogWarning(fex, "SOAP Fault al actualizar planta: {Message}", msg);
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Upstream SOAP fault.", detail = msg, statusCode = 502 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating plant.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Unexpected error updating plant.", statusCode = 500 });
            }
        }

        // PATCH: api/plants/{id}
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Patch(Guid id, [FromBody] UpdatePlantRequest body, CancellationToken ct)
        {
            // Si tu UpdatePlantRequest soporta parciales igual que PUT, reutilizamos misma lógica.
            if (body is null || body.Id == Guid.Empty || body.Id != id)
                return BadRequest(new { message = "Id in route and body must match.", statusCode = 400 });

            try
            {
                var updated = await _gateway.UpdatePlantAsync(body, ct);
                return Ok(updated);
            }
            catch (FaultException fex)
            {
                var msg = (fex.Message ?? string.Empty).Trim();

                if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = "Plant not found.", statusCode = 404 });

                if (msg.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { message = "Plant already exists.", statusCode = 409 });

                if (msg.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("bad request", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = msg, statusCode = 400 });

                _logger.LogWarning(fex, "SOAP Fault al hacer patch de planta: {Message}", msg);
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Upstream SOAP fault.", detail = msg, statusCode = 502 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error patching plant.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Unexpected error patching plant.", statusCode = 500 });
            }
        }

        // DELETE: api/plants/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                await _gateway.DeletePlantAsync(id, ct);
                return NoContent();
            }
            catch (FaultException fex)
            {
                var msg = (fex.Message ?? string.Empty).Trim();

                if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = "Plant not found.", statusCode = 404 });

                _logger.LogWarning(fex, "SOAP Fault al eliminar planta: {Message}", msg);
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Upstream SOAP fault.", detail = msg, statusCode = 502 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting plant.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Unexpected error deleting plant.", statusCode = 500 });
            }
        }
    }
}
