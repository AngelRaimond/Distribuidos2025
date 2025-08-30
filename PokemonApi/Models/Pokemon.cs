namespace PokemonApi.Models;

public class Pokemon
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public int Level { get; set; }

    public Stats Stats { get; set; } 
}