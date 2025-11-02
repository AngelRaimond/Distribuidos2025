using Microsoft.AspNetCore.Mvc;
using GardenApi.Services;
using GardenApi.Dtos;
using System.ComponentModel.DataAnnotations;

namespace GardenApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PlantsController : ControllerBase
{
    private readonly IPlantService _plantService;

    public PlantsController(IPlantService plantService)
    {
        _plantService = plantService;
    }

    /// <summary>
    /// Obtiene una planta por su ID
    /// </summary>
    /// <param name="id">ID único de la planta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Planta encontrada</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PlantResponse>> GetByIdAsync(
        [FromRoute] Guid id, 
        CancellationToken cancellationToken = default)
    {
        // Validación de ID vacío
        if (id == Guid.Empty)
        {
            return BadRequest(new ErrorResponse 
            { 
                Message = "Invalid plant ID. ID cannot be empty.", 
                StatusCode = 400 
            });
        }

        var plant = await _plantService.GetPlantByIdAsync(id, cancellationToken);
        
        if (plant == null)
        {
            return NotFound(new ErrorResponse 
            { 
                Message = $"Plant with ID {id} not found.", 
                StatusCode = 404 
            });
        }

        return Ok(plant);
    }
}