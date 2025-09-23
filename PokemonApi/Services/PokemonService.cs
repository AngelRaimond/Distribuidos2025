using PokemonApi.Dtos;
using PokemonApi.Repositories;
using System.ServiceModel;
using PokemonApi.Mappers;
using PokemonApi.Validators;
using PokemonApi.Models;


namespace PokemonApi.Services;

public class PokemonService : IPokemonService
{
    private readonly IPokemonRepository _pokemonRepository;

    public PokemonService(IPokemonRepository pokemonRepository)
    {
        _pokemonRepository = pokemonRepository;
    }

    public async Task<PokemonResponseDto> UpdatePokemon(UpdatePokemonDto pokemonToUpdate, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonRepository.GetPokemonByIdAsync(pokemonToUpdate.Id, cancellationToken);
        if (!PokemonExists(pokemon))
        {
            throw new FaultException(reason: "Pokemon not found");
        }

        if (!await IsPokemonAllowedToBeUpdated(pokemonToUpdate, cancellationToken))
        {
            throw new FaultException("Pokemon already exists");
        }

        pokemon.Name = pokemonToUpdate.Name;
        pokemon.Type = pokemonToUpdate.Type;
        pokemon.Stats.Attack = pokemonToUpdate.Stats.Attack;
        pokemon.Stats.Defense = pokemonToUpdate.Stats.Defense;
        pokemon.Stats.Speed = pokemonToUpdate.Stats.Speed;

        await _pokemonRepository.UpdatePokemonAsync(pokemon, cancellationToken);
        return pokemon.ToResponseDto();
    }

    private async Task<bool> IsPokemonAllowedToBeUpdated(UpdatePokemonDto pokemonToUpdate, CancellationToken cancellationToken)
    {
        var duplicatedPokemon = await _pokemonRepository.GetByNameAsync(pokemonToUpdate.Name, cancellationToken);
        return duplicatedPokemon is null || IsTheSamePokemon(duplicatedPokemon, pokemonToUpdate);
    }

    private static bool IsTheSamePokemon(Pokemon pokemon, UpdatePokemonDto pokemonToUpdate)
    {
        return pokemon.Id == pokemonToUpdate.Id;
    }

    public async Task<DeletePokemonResponseDto> DeletePokemon(Guid id, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonRepository.GetPokemonByIdAsync(id, cancellationToken);
        if (!PokemonExists(pokemon))
        {
            throw new FaultException(reason: "Pokemon not found");
        }
        await _pokemonRepository.DeletePokemonAsync(pokemon, cancellationToken);
        return new DeletePokemonResponseDto { Success = true };
    }

    public async Task<IList<PokemonResponseDto>> GetPokemonByName(string name, CancellationToken cancellationToken)
    {
        var pokemons = await _pokemonRepository.GetPokemonsByNameAsync(name, cancellationToken);
        return pokemons.ToResponseDto();
    }

    public async Task<PokemonResponseDto> GetPokemonById(Guid id, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonRepository.GetPokemonByIdAsync(id, cancellationToken);
        return PokemonExists(pokemon) ? pokemon.ToResponseDto() : throw new FaultException("Pokemon not found");

    }
    public async Task<PokemonResponseDto> CreatePokemon(CreatePokemonDto pokemonRequest, CancellationToken cancellationToken)
    {
        //Fluent Methods
        pokemonRequest
            .ValidateName()
            .ValidateType()
            .ValidateLevel();

        if (await IsPokemonDuplicated(pokemonRequest.Name, cancellationToken))
        {
            throw new FaultException("Pokemon already exists");
        }

        var pokemon = await _pokemonRepository.CreateAsync(pokemonRequest.ToModel(), cancellationToken);

        return pokemon.ToResponseDto();
    }

    private static bool PokemonExists(Pokemon? pokemon)
    {
        return pokemon is not null;
    }

    private async Task<bool> IsPokemonDuplicated(string name, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonRepository.GetByNameAsync(name, cancellationToken);
        return pokemon is not null;
    }
}