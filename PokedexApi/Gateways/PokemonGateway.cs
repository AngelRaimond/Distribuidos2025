using System.ServiceModel;
using PokedexApi.Infrastructure.Soap.Contracts;
using PokedexApi.Models;
using PokedexApi.Mappers;
using PokedexApi.Exceptions;
using PokedexApi.Infrastructure.Soap.Dtos;
using System.ServiceModel.Channels;

namespace PokedexApi.Gateways;

public class PokemonGateway : IPokemonGateway
{
    public readonly IPokemonContract _pokemonContract;
    private readonly ILogger<PokemonGateway> _logger;

    public PokemonGateway(IConfiguration configuration, ILogger<PokemonGateway> logger)
    {
        var binding = new BasicHttpBinding();
        var endpoint = new EndpointAddress(configuration.GetValue<string>("PokemonService:Url"));
        _pokemonContract = new ChannelFactory<IPokemonContract>(binding, endpoint).CreateChannel();
        _logger = logger;
    }
    public async Task<Pokemon> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var pokemon = await _pokemonContract.GetPokemonById(id, cancellationToken);
            return pokemon.ToModel();
        }
        catch (FaultException ex) when (ex.Message == "Pokemon not found")
        {
            _logger.LogWarning(ex, "Pokemon not found");
            return null;
        }
    }

    public async Task<IList<Pokemon>> GetPokemonByNameAsync(string name, CancellationToken cancellationToken)
    {
        var pokemons = await _pokemonContract.GetPokemonByName(name, cancellationToken);
        return pokemons.ToModel();
    }

    public async Task<Pokemon> CreatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending request to SOAP API, with pokemon: {name}", pokemon.Name);

            var createdPokemon = await _pokemonContract.CreatePokemon(pokemon.ToRequest(), cancellationToken);
            return createdPokemon.ToModel();

        }
        catch (Exception e)
        {
            throw new Exception("Error creating pokemon", e);
        }
    }
}