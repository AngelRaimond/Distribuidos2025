using Microsoft.AspNetCore.Mvc;
using PokedexApi.Dtos;
using PokedexApi.Models;
using PokedexApi.Services;
using PokedexApi.Mappers;
using PokedexApi.Exceptions;

namespace PokedexApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PokemonsController : ControllerBase
{
    private readonly IPokemonService _pokemonService;
    public PokemonsController(IPokemonService pokemonService)
    {
        _pokemonService = pokemonService;
    }

    //Http Status
    // 200 OK (Si existe el pokemon)
    //400 Bad Request (Id invalido) --- Casi no se usa
    // 404 Not Found (no existe el pokemon)
    // 500 Internal Server Error (Error del servidor)

    //Http Verb -Get
    [HttpGet("{id}", Name = "GetPokemonByIdAsync")] // api/v1/pokemons/{id}
    public async Task<ActionResult<PokemonResponse>> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonService.GetPokemonByIdAsync(id, cancellationToken);
        return pokemon is null ? NotFound() : Ok(pokemon.ToResponse());
    }

    //Http Verb - Post
    //Http Status
    // 400 Bad Request (Si usuario manda informacion erronea)
    // 409 Conflict (Si el pokemon ya existe)
    // 422 Unprocessable Entity (Si el pokemon no cumple con las reglas de negocio interna)
    // 500 Internal Server Error (Error del servidor)
    // 200 OK (El recurso creado + id) no sigue muchos para buenas practicas de RESTFul
    // 201 Created (El recurso creado + id) href = hace referencia al get para obtener el recurso
    // 202 Accepted (Si la creacion del recurso es asincrona y toma tiempo)

    [HttpPost]
    public async Task<ActionResult<PokemonResponse>> CreatePokemonAsync([FromBody] CreatePokemonRequest createPokemon, CancellationToken cancellationToken)
    {
        try
        {
            if (!IsValidAttack(createPokemon))
            {
                return BadRequest(new { Message = "Attack does not have a valid value" });
            }

            var pokemon = await _pokemonService.CreatePokemonAsync(createPokemon.ToModel(), cancellationToken);

            // 201
            return CreatedAtRoute(nameof(GetPokemonByIdAsync), routeValues: new { id = pokemon.Id }, pokemon.ToResponse());
        }
        catch (PokemonAlreadyExistsException e)
        {
            // 409 Conflict - { Message = "Pokemon NAME already exists" }
            return Conflict(new { Message = e.Message });
        }
    }
    private static bool IsValidAttack(CreatePokemonRequest createPokemon)
    {
        return createPokemon.Stats.Attack > 0;
    }
}