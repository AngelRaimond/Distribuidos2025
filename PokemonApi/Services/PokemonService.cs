using PokemonApi.Dtos;

namespace PokemonApi.Services
{
    public class PokemonService : IPokemonService
    {
        public Task<PokemonResponseDto> CreatePokemon(CreatePokemonDto pokemon, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }
    }
}
