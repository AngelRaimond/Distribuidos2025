using Microsoft.AspNetCore.Mvc;
using PokedexApi.Dtos;
using PokedexApi.Expections;
using PokedexApi.Mappers;
using PokedexApi.Services;

namespace PokedexApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PokemonsController : ControllerBase // Nos va a dar status code verbos HTTP etc.
{
    private readonly IPokemonService _pokemonService;

    public PokemonsController(IPokemonService pokemonsService)
    {
        _pokemonService = pokemonsService;
    }

    // localhost:PORT/api/v1/pokemons/ID(<- Es el ID del recurso)
    [HttpGet("{id}", Name = "GetPokemonByIdAsync")] // Es un endopoint del controlador y escucha un metodo Get, {id} es el recurso que va a viajar en la peticion
    public async Task<ActionResult<PokemonResponse>> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonService.GetPokemonByIdAsync(id, cancellationToken);
        // HTTP verb - GET
        return pokemon is null ? NotFound() : Ok(pokemon.ToResponse()); // Regresa un status code en caso de que existe el pokemon
        // En caso de que no existe se regresa un 404 (NotFound)
    }

    // Pagination
    // localhost:PORT/api/v1/pokemons?name=pikachu&type=fire
    // HTTP VERB - GET
    // 200 - OK (si existe o no pokemon(si no hay nada se regresa vacio)) 
    // 400 - BadRequest (Si alguno de los quert parameter son incorrectos)
    [HttpGet]
    public async Task<ActionResult<IList<PokemonResponse>>> GetPokemonsAsync([FromQuery] string name, [FromQuery] string type, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(type))
        {
            return BadRequest(new { Message = "Type query parameter is required" });
        }

        var pokemons = await _pokemonService.GetPokemonsAsync(name, type, cancellationToken);
        return Ok(pokemons.ToResponse());
    }

    // localhost:port/api/v1/pokemons
    // Body request - JSON
    // HTTP verb POST
    // HTTP Status
    // 400 Bad request (Si el usuario manda informacion incorrecta)
    // 409 Conlict (Ya existe esa entidad)
    // 422 Entidad no procesable (Por alguna regla de negocio interna)
    // 500 Internal Error
    // 200 - Ok (El recurso creado + id) -- no sigue bien las buenas practicas de RESTFUL
    // 201 - Created (Recurso creado + id) -- Response header retorna un href donde referencia al GET para obtener el recurso
    // 202 - Accepted (Procesamiento async)
    [HttpPost]
    public async Task<ActionResult<PokemonResponse>> CreatePokemonAsync([FromBody] CreatePokemonRequest createPokemon, CancellationToken cancellationToken)
    {
        try
        {
            if (!IsValidAttack(createPokemon))
            {
                // {"message"} : ""
                return BadRequest(new { Message = "Attack does not have a valid value" });
            }

            var pokemon = await _pokemonService.CreatePokemonAsync(createPokemon.ToModel(), cancellationToken);

            // 201
            return CreatedAtRoute(nameof(GetPokemonByIdAsync), new { id = pokemon.Id }, pokemon.ToResponse());
        }
        catch(PokemonAlreadyExistsException e)
        {
            // 409 Conflict - JSON {"Message": "......"}
            return Conflict(new {Message = e.Message});
        }
        
    }

    //localhost:port/api/v1/pokemons/ID
    // HTTP VERB - DELETE
    // 204 - No Content (si se borro correctamente)
    // 200 - Ok (Si se borro correctamente) - no sigue buenas practicas RESTFUL
    // {"message": "pokemon borrado correctamente"} - a veces regresan un JSON como respuesta
    // 404 - Not Found (Si el pokemon no existe)
    // 500 - Internal server error

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePokemonAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _pokemonService.DeletePokemonAsync(id, cancellationToken);
            return NoContent(); // 204
        }
        catch (PokemonNotFoundException)
        {
            return NotFound(); // 404
        }
    }

    private static bool IsValidAttack(CreatePokemonRequest createPokemon)
    {
        return createPokemon.Stats.Attack > 0;
    }
}