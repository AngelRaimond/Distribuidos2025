using Microsoft.AspNetCore.Mvc;
using PokedexApi.Dtos;
using PokedexApi.Services;
using PokedexApi.Mappers;

namespace PokedexApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")] //api/v1/Pokemons
public class PokemonsController : ControllerBase
{
    private readonly IPokemonService _pokemonService;
    public PokemonsController(IPokemonService pokemonService)
    {
        _pokemonService = pokemonService;
    }
    //localhost:PORT/api/v1/pokemons/ID
    [HttpGet("{id}")]

    public async Task<ActionResult<PokemonResponse>> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonService.GetPokemonByIdAsync(id, cancellationToken);
        return pokemon is null ? NotFound() : Ok(pokemon.ToResponse());
    }
}