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
}
