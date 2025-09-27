using PokedexApi.Expections;
using PokedexApi.Gateways;
using PokedexApi.Models;

namespace PokedexApi.Services;


public class PokemonService : IPokemonService
{
    private readonly IPokemonGateway _pokemonGateway;

    public PokemonService(IPokemonGateway pokemonGateway)
    {
        _pokemonGateway = pokemonGateway;
    }

    public async Task DeletePokemonAsync(Guid id, CancellationToken cancellationToken)
    {
        await _pokemonGateway.DeletePokemonAsync(id, cancellationToken);
    }
    
    public async Task<Pokemon> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _pokemonGateway.GetPokemonByIdAsync(id, cancellationToken);
    }

    public async Task<Pokemon> CreatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        var pokemons = await _pokemonGateway.GetPokemonsByNameAsync(pokemon.Name, cancellationToken);
        if (PokemonExists(pokemons, pokemon.Name))
        {
            throw new PokemonAlreadyExistsException(pokemon.Name);
        }

        return await _pokemonGateway.CreatePokemonAsync(pokemon, cancellationToken);
    }

    private static bool PokemonExists(IList<Pokemon> pokemons, string pokemonNameToSearch)
    {
        return pokemons.Any(s => s.Name.ToLower().Equals(pokemonNameToSearch.ToLower()));
    }

    public async Task<IList<Pokemon>> GetPokemonsAsync(string name, string type, CancellationToken cancellationToken)
    {
        var pokemons = await _pokemonGateway.GetPokemonsByNameAsync(name, cancellationToken);
        return pokemons.Where(s => s.Type.ToLower().Contains(type.ToLower())).ToList();
    }
}