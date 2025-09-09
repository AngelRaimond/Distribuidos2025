using Microsoft.AspNetCore.Mvc;
using PokedexApi.Dtos;

namespace PokedexApi.Controllers;


[ApiController]
[Route("api/v1[controller]")]
public class PokemonsControllers : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult <PokemonResponse>> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok();
    }
        
    
}

