using System.CodeDom;
using Microsoft.AspNetCore.StaticAssets;
using PokedexApi.Dtos;
using PokedexApi.Infrastructure.Soap.Dtos;
using PokedexApi.Models;

namespace PokedexApi.Mappers;

public static class PokemonMapper
{
    public static Pokemon ToModel(this PokemonResponseDto pokemonResposeDto)
    {
        return new Pokemon
        {
            Id = pokemonResposeDto.Id,
            Name = pokemonResposeDto.Name,
            Type = pokemonResposeDto.Type,
            Level = pokemonResposeDto.Level,
            Stats = new Stats
            {
                Attack = pokemonResposeDto.Stats.Attack,
                Defene = pokemonResposeDto.Stats.Defense,
                Speed = pokemonResposeDto.Stats.Speed
            }
        };
    }

    public static PokemonResponse ToResponse(this Pokemon pokemon)
    {
        return new PokemonResponse
        {
            Id = pokemon.Id,
            Name = pokemon.Name,
            Type = pokemon.Type,
            Attack = pokemon.Stats.Attack
        };
    }

    public static Pokemon ToModel(this CreatePokemonRequest createPokemonRequest)
    {
        return new Pokemon
        {
            Name = createPokemonRequest.Name,
            Type = createPokemonRequest.Type,
            Level = createPokemonRequest.Level,
            Stats = new Stats
            {
                Attack = createPokemonRequest.Stats.Attack,
                Defene = createPokemonRequest.Stats.Defense,
                Speed = createPokemonRequest.Stats.Speed
            }
        };
    }

    public static IList<Pokemon> ToModel(this IList<PokemonResponseDto> pokemonResponseDtos)
    {
        return pokemonResponseDtos.Select(s => s.ToModel()).ToList();
    }

    public static CreatePokemonDto ToRequest(this Pokemon pokemon)
    {
        return new CreatePokemonDto
        {
            Name = pokemon.Name,
            Type = pokemon.Type,
            Level = pokemon.Level,
            Stats = new StatsDto
            {
                Attack = pokemon.Stats.Attack,
                Defense = pokemon.Stats.Defene,
                Speed = pokemon.Stats.Speed
            }
        };
    }

    public static IList<PokemonResponse> ToResponse(this IList<Pokemon> pokemons)
    {
        return pokemons.Select(s => s.ToResponse()).ToList();
    }
}