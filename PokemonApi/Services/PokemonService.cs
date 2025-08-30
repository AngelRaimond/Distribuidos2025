using System.ServiceModel;
using PokemonApi.Dtos;
using PokemonApi.Repositories;
using PokemonApi.Mappers;
using PokemonApi.Validators;
using PokemonApi.Models;

namespace PokemonApi.Services;

public class PokemonService : IPokemonServices
{
    private readonly IPokemonRepository _pokemonRepository;
    public PokemonService(IPokemonRepository pokemonRepository)
    {
        _pokemonRepository = pokemonRepository;
    }

    public async Task<IList<PokemonResponseDto>> GetPokemonsByName(string name, CancellationToken cancellationToken)
    {
        var pokemons = await _pokemonRepository.GetPokemonsByNameAsync(name, cancellationToken);
        return pokemons.ToResponseDto();
    }

    public async Task<PokemonResponseDto> GetPokemonById(Guid id, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonRepository.GetPokemonByIdAsync(id, cancellationToken);
        return PokemonExists(pokemon) ? pokemon.ToReponseDto() : throw new FaultException("Pokemon not found");
    }
    public async Task<PokemonResponseDto> CreatePokemon(CreatePokemonDto pokemonRequest, CancellationToken cancellationToken)
    {
        pokemonRequest.ValidateName().ValidateLevel().ValidateType();
        if (await PokemonAlreadyExists(pokemonRequest.Name, cancellationToken))
        {
            throw new FaultException("Pokemon already exists");
        }

        var pokemon = await _pokemonRepository.CreateAsync(pokemonRequest.ToModel(), cancellationToken);


        return pokemon.ToReponseDto();
    }

    private static bool PokemonExists(Pokemon? pokemon)
    {
        return pokemon is not null; 
    }

    private async Task<bool> PokemonAlreadyExists(string name, CancellationToken cancellationToken)
    {
        var pokemons = await _pokemonRepository.GetByNameAsync(name, cancellationToken);
        return pokemons is not null;
    }
}