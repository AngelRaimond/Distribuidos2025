namespace PokedexApi.Dtos;

public class PokemonResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Attack { get; set; }
}
