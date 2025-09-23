using System.ComponentModel.DataAnnotations;

namespace PokedexApi.Dtos;

public class CreatePokemonRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [MinLength(3)]
    public string Type { get; set; } = string.Empty;

    public int Level { get; set; }

    public StatsRequest Stats { get; set; }
}

public class StatsRequest
{
    public int HP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
}