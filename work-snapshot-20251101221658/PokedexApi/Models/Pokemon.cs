namespace PokedexApi.Models;

public class Pokemon
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int Level { get; set; }
    public Stats Stats { get; set; }
}

public class Stats
{
    public int Attack { get; set; }
    public int Defene { get; set; }
    public int Speed { get; set; }
}