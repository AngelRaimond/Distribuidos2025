namespace PokedexApi.Models;

public class Pokemon
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; }
    public Stats Stats { get; set; } = new Stats();
}

public class Stats
{
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
}
