using PokedexApi.Infrastructure.Soap.Contracts;
using PokedexApi.Models;
using System.ServiceModel;
using PokedexApi.Mappers;

namespace PokedexApi.Gateways;

public class PokemonGateway : IPokemonGateway
{
    private readonly IPokemonContract _pokemonContract;

    public PokemonGateway(IConfiguration configuration)
    {
        var binging = new BasicHttpBinding();
        var endpoint = new EndpointAddress(configuration.GetValue<string>("PokemonService:Url"));
        _pokemonContract = new ChannelFactory<IPokemonContract>(binging, endpoint).CreateChannel();
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
            return null;
        }
    }
}