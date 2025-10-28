using System.ServiceModel;
using PokedexApi.Infrastructure.Soap.Contracts;
using PokedexApi.Gateways;
using PokedexApi.Services;

using PokedexApi.Gateways;
using PokedexApi.Infrastructure.Soap.Contracts;
using PokedexApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 
builder.Services.AddControllers();

// SOAP contract registration via ChannelFactory
builder.Services.AddScoped<IPokemonContract>(provider =>
{
    var endpoint = new EndpointAddress("http://host.docker.internal:8082/PokemonService.svc");
    var binding = new BasicHttpBinding();
    var channelFactory = new ChannelFactory<IPokemonContract>(binding, endpoint);
    return channelFactory.CreateChannel();
});

// Register gateway and service
builder.Services.AddScoped<IPokemonGateway, PokemonGateway>();
builder.Services.AddScoped<IPokemonService, PokemonService>();

builder.Services.AddScoped<IPokemonService, PokemonService>();
builder.Services.AddScoped<IPokemonGateway, PokemonGateway>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection(); 
app.MapControllers();

app.Run();