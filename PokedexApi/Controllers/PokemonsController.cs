using Microsoft.AspNetCore.Mvc;
using PokedexApi.Dtos;
using PokedexApi.Expections;
using PokedexApi.Mappers;
using PokedexApi.Services;
using PokedexApi.Shared.Dto;
using PokedexApi.Models;
using System.Linq;

namespace PokedexApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PokemonsController : ControllerBase
{
    private readonly IPokemonService _pokemonService;

    public PokemonsController(IPokemonService pokemonsService)
    {
        _pokemonService = pokemonsService;
    }

    [HttpGet("{id}", Name = "GetPokemonByIdAsync")]
    public async Task<ActionResult<PokemonResponse>> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var pokemon = await _pokemonService.GetPokemonByIdAsync(id, cancellationToken);
            return Ok(pokemon.ToResponse());
        }
        catch (PokemonNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<PokemonResponse>>> Get(
        [FromQuery] string? name,
        [FromQuery] string? type,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string orderBy = "name",
        [FromQuery] string orderDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) return BadRequest(new { Message = "pageNumber must be >= 1" });
        if (pageSize < 1 || pageSize > 200) return BadRequest(new { Message = "pageSize must be between 1 and 200" });

        var pokemons = await _pokemonService.GetPokemonsAsync(name ?? string.Empty, type ?? string.Empty, cancellationToken);

        // ensure type filter
        if (!string.IsNullOrWhiteSpace(type))
            pokemons = pokemons.Where(p => p.Type?.Contains(type, StringComparison.OrdinalIgnoreCase) == true).ToList();

        // Ordering
        bool desc = string.Equals(orderDirection, "desc", StringComparison.OrdinalIgnoreCase);
        IEnumerable<Pokemon> ordered = orderBy?.ToLower() switch
        {
            "name" => desc ? pokemons.OrderByDescending(p => p.Name) : pokemons.OrderBy(p => p.Name),
            "type" => desc ? pokemons.OrderByDescending(p => p.Type) : pokemons.OrderBy(p => p.Type),
            "level" => desc ? pokemons.OrderByDescending(p => p.Level) : pokemons.OrderBy(p => p.Level),
            "attack" => desc ? pokemons.OrderByDescending(p => p.Stats.Attack) : pokemons.OrderBy(p => p.Stats.Attack),
            "id" => desc ? pokemons.OrderByDescending(p => p.Id) : pokemons.OrderBy(p => p.Id),
            _ => desc ? pokemons.OrderByDescending(p => p.Name) : pokemons.OrderBy(p => p.Name),
        };

        var total = ordered.Count();
        var items = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var result = new PagedResponse<PokemonResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Data = items.Select(i => i.ToResponse()).ToList()
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PokemonResponse>> Create([FromBody] CreatePokemonRequest createPokemon, CancellationToken cancellationToken)
    {
        if (!IsValidAttack(createPokemon)) return BadRequest();

        var model = new Models.Pokemon
        {
            Id = Guid.NewGuid(),
            Name = createPokemon.Name,
            Type = createPokemon.Type,
            Level = createPokemon.Level,
            Stats = new Models.Stats
            {
                Attack = createPokemon.Stats.Attack,
                Defense = createPokemon.Stats.Defense,
                Speed = createPokemon.Stats.Speed
            }
        };

        var created = await _pokemonService.CreatePokemonAsync(model, cancellationToken);
        return CreatedAtRoute(nameof(GetPokemonByIdAsync), new { id = created.Id }, created.ToResponse());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _pokemonService.DeletePokemonAsync(id, cancellationToken);
            return NoContent();
        }
        catch (PokemonNotFoundException)
        {
            return NotFound();
        }
    }

    private static bool IsValidAttack(CreatePokemonRequest createPokemon)
    {
        return createPokemon.Stats.Attack > 0;
    }
    
}
