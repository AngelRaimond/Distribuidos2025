using System.ServiceModel;
using PokedexApi.Models;
using PokedexApi.Mappers;
using PokedexApi.Infrastructure.Soap.Contracts;
using PokedexApi.Infrastructure.Soap.Dtos;
using PokedexApi.Expections;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace PokedexApi.Gateways;

public class PokemonGateway : IPokemonGateway
{
    private readonly IPokemonContract _pokemonContract;
    private readonly ILogger<PokemonGateway> _logger;

    public PokemonGateway(IPokemonContract pokemonContract, ILogger<PokemonGateway> logger)
    {
        _pokemonContract = pokemonContract;
        _logger = logger;
    }

    public async Task<Pokemon> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var pokemonDto = await _pokemonContract.GetPokemonById(id, cancellationToken);
            if (pokemonDto == null) throw new PokemonNotFoundException(id);
            return pokemonDto.ToModel();
        }
        catch (PokemonNotFoundException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching pokemon by id from SOAP");
            throw;
        }
    }

    public async Task<IList<Pokemon>> GetPokemonsByNameAsync(string name, CancellationToken cancellationToken)
    {
        try
        {
            var pokemonsDto = await _pokemonContract.GetPokemonByName(name ?? string.Empty, cancellationToken);
            return pokemonsDto?.Select(d => d.ToModel()).ToList() ?? new List<Pokemon>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching pokemons by name from SOAP");
            throw;
        }
    }

    public async Task<Pokemon> CreatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending request to SOAP API, with pokemon: {name}", pokemon.Name);
            var createdPokemon = await _pokemonContract.CreatePokemon(pokemon.ToRequest(), cancellationToken);
            return createdPokemon.ToModel();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating pokemon via SOAP");
            throw;
        }
    }

    public async Task DeletePokemonAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var resp = await _pokemonContract.DeletePokemon(id, cancellationToken);
            // optionally check resp for success
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting pokemon via SOAP");
            throw;
        }
    }

    public async Task<Pokemon> UpdatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _pokemonContract.UpdatePokemon(pokemon.ToUpdateRequest(), cancellationToken);
            return updated.ToModel();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating pokemon via SOAP");
            throw;
        }
    }
}
