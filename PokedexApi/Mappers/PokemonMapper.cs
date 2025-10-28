using PokedexApi.Dtos;
using PokedexApi.Infrastructure.Soap.Dtos;
using PokedexApi.Models;
using System.Linq;

namespace PokedexApi.Mappers;

public static class PokemonMapper
{
    public static Pokemon ToModel(this PokemonResponseDto dto)
    {
        if (dto == null) return null!;
        return new Pokemon
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            Type = dto.Type ?? string.Empty,
            Level = dto.Level,
            Stats = new Stats
            {
                Attack = dto.Stats?.Attack ?? 0,
                Defense = dto.Stats?.Defense ?? 0,
                Speed = dto.Stats?.Speed ?? 0
            }
        };
    }

    public static PokemonResponse ToResponse(this Pokemon pokemon)
    {
        if (pokemon == null) return null!;
        return new PokemonResponse
        {
            Id = pokemon.Id,
            Name = pokemon.Name,
            Type = pokemon.Type,
            Attack = pokemon.Stats?.Attack ?? 0
        };
    }

    public static IList<PokemonResponse> ToResponse(this IList<Pokemon> pokemons)
    {
        return pokemons?.Select(p => p.ToResponse()).ToList() ?? new List<PokemonResponse>();
    }


    public static CreatePokemonDto ToRequest(this Pokemon pokemon)
    {
        if (pokemon == null) return null!;
        return new CreatePokemonDto
        {
            Name = pokemon.Name,
            Type = pokemon.Type,
            Level = pokemon.Level,
            Stats = new Infrastructure.Soap.Dtos.StatsDto
            {
                Attack = pokemon.Stats?.Attack ?? 0,
                Defense = pokemon.Stats?.Defense ?? 0,
                Speed = pokemon.Stats?.Speed ?? 0
            }
        };
    }

    public static UpdatePokemonDto ToUpdateRequest(this Pokemon pokemon)
    {
        if (pokemon == null) return null!;
        return new UpdatePokemonDto
        {
            Id = pokemon.Id,
            Name = pokemon.Name,
            Type = pokemon.Type,
            Level = pokemon.Level,
            Stats = new Infrastructure.Soap.Dtos.StatsDto
            {
                Attack = pokemon.Stats?.Attack ?? 0,
                Defense = pokemon.Stats?.Defense ?? 0,
                Speed = pokemon.Stats?.Speed ?? 0
            }
        };
    }
}
