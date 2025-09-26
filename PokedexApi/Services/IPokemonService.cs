namespace PokedexApi.Services;

using PokedexApi.Models;
public interface IPokemonService
{
    Task<Pokemon> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Pokemon> CreatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken);

    Task<IList<Pokemon>> GetPokemonsAsync(string name, string type, CancellationToken cancellationToken);

    Task DeletePokemonAsync(Guid id, CancellationToken cancellationToken);
}