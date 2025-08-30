using PokemonApi.Dtos;
using PokemonApi.Infrastructure.Entities;
using PokemonApi.Models;

namespace PokemonApi.Mappers;

public static class PokemonMapper
{
    public static Pokemon ToModel(this PokemonEntity pokemonEntity)
    {
        if (pokemonEntity is null) return null!;

        return new Pokemon
        {
            Id = pokemonEntity.Id,
            Name = pokemonEntity.Name,
            Type = pokemonEntity.Type,
            Level = pokemonEntity.Level,
            Stats = new Stats
            {
                Attack = pokemonEntity.Attack,
                Defense = pokemonEntity.Defense,
                Speed = pokemonEntity.Speed,
                Weight = pokemonEntity.Weight
            }
        };
    }

    public static PokemonEntity ToEntity(this Pokemon pokemon)
    {
        return new PokemonEntity
        {
            Id = pokemon.Id,
            Level = pokemon.Level,
            Type = pokemon.Type,
            Name = pokemon.Name,
            Attack = pokemon.Stats.Attack,
            Speed = pokemon.Stats.Speed,
            Defense = pokemon.Stats.Defense,
            Weight = pokemon.Stats.Weight
        };
    }

    public static PokemonResponseDto ToReponseDto(this Pokemon pokemon)
    {
        return new PokemonResponseDto
        {
            Id = pokemon.Id,
            Level = pokemon.Level,
            Type = pokemon.Type,
            Name = pokemon.Name,
            Stats = new StatsDto
            {
                Attack = pokemon.Stats.Attack,
                Defense = pokemon.Stats.Defense,
                Speed = pokemon.Stats.Speed,
                Weight = pokemon.Stats.Weight
            }
        };
    }

    public static Pokemon ToModel(this CreatePokemonDto requestPokemonDto)
    {
        return new Pokemon
        {
            Level = requestPokemonDto.Level,
            Type = requestPokemonDto.Type,
            Name = requestPokemonDto.Name,
            Stats = new Stats
            {
                Attack = requestPokemonDto.Stats.Attack,
                Defense = requestPokemonDto.Stats.Defense,
                Speed = requestPokemonDto.Stats.Speed,
                Weight = requestPokemonDto.Stats.Weight
            }
        };
    }
    public static IList<PokemonResponseDto> ToResponseDto(this IReadOnlyList<Pokemon> pokemons)
    {
        return pokemons.Select(s => s.ToReponseDto()).ToList();
    }

    public static IReadOnlyList<Pokemon> ToModel(this IReadOnlyList<PokemonEntity> pokemons)
    {
        return pokemons.Select(s => s.ToModel()).ToList();
    }

}