using System.ServiceModel;
using PokemonApi.Dtos;

namespace PokemonApi.Services;

[ServiceContract(Name = "PokemonService", Namespace = "http://pokemon-api/pokemon-service")]
public interface IPokemonServices
{
    [OperationContract]
    Task<PokemonResponseDto> CreatePokemon(CreatePokemonDto pokemon, CancellationToken cancellationToken);

    [OperationContract]
    Task<PokemonResponseDto> GetPokemonById(Guid id, CancellationToken cancellationToken);

    [OperationContract]
    Task<IList<PokemonResponseDto>> GetPokemonsByName(string name, CancellationToken cancellationToken);
}