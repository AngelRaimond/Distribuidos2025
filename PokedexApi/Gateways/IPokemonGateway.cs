namespace PokedexApi.Gateways;

using PokedexApi.Models;
//Como si fuera un repositorio

public interface IPokemonGateway
{
    Task<Pokemon> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken);
}