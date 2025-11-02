namespace PokedexApi.Expections;

public class PokemonAlreadyExistsException : Exception
{
    public PokemonAlreadyExistsException(string pokemonName) : base($"Pokemon {pokemonName} already exists")
    {
        
    }
}