using PokedexApi.Exceptions;
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
    public async Task<Pokemon> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _pokemonGateway.GetPokemonByIdAsync(id, cancellationToken);
    }

    public async Task<Pokemon> CreatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        //validat que no exista un pokemon con el mismo nombre
        //Peticion -> PokemonAPI (SOAP) -> ObtenerPokemonsPorNombre
        var pokemons = await _pokemonGateway.GetPokemonByNameAsync(pokemon.Name, cancellationToken);
        if (PokemonExists(pokemons, pokemon.Name))
        {
            throw new PokemonAlreadyExistsException(pokemon.Name);
        }
        //Si no existe, crear el pokemon
        //Peticion -> PokemonAPI (SOAP) -> CrearPokemon
        return await _pokemonGateway.CreatePokemonAsync(pokemon, cancellationToken);

    }
    
    private static bool PokemonExists(IList<Pokemon> pokemons, string pokemonNameToSearch)
    {
        return pokemons.Any(Pokemon => Pokemon.Name.ToLower().Equals(pokemonNameToSearch.ToLower()));
    }
}

//Arquitectura (BFF o Experience API o Aggregator Pattern)
//Cliente (Postman, Insomnia, algun aplicativo WEB) -> Rest API (Pokedex Api) -> PokedexApi (SOAP)