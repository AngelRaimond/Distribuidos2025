using System.ServiceModel;
using PokedexApi.Models;
using PokedexApi.Mappers;
using PokedexApi.Infrastructure.Soap.Contracts;
using PokedexApi.Expections;


namespace PokedexApi.Gateways;

public class PokemonGateway : IPokemonGateway
{
    private readonly IPokemonContract _pokemonContract;
    private readonly ILogger<PokemonGateway> _logger;

    public PokemonGateway(IConfiguration configuration, ILogger<PokemonGateway> logger)
    {
        var binding = new BasicHttpBinding();
        var endopoint = new EndpointAddress(configuration.GetValue<string>("PokemonService:Url"));
        _pokemonContract = new ChannelFactory<IPokemonContract>(binding, endopoint).CreateChannel();
        _logger = logger;
    }

    public async Task DeletePokemonAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _pokemonContract.DeletePokemon(id, cancellationToken);
        }
        catch (FaultException ex) when (ex.Message == "Pokemon not found")
        {
            _logger.LogWarning(ex, "Pokemon not found");
            throw new PokemonNotFoundException(id);    
        }
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
            _logger.LogWarning(ex, "Pokemon Not Found");
            return null;
        }
    }

    public async Task<IList<Pokemon>> GetPokemonsByNameAsync(string name, CancellationToken cancellationToken)
    {
        _logger.LogDebug(":(");
        var pokemons = await _pokemonContract.GetPokemonsByName(name, cancellationToken);
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
            _logger.LogError(e, "Algo trono en el create pokemon a soap");
            throw;
        }
    }
}