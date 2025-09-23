using PokedexApi.Models;

namespace PokedexApi.Dtos;

public class PokemonResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Attack { get; set; }
}