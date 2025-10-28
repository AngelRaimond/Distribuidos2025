using System.ServiceModel;
using PokedexApi.Infrastructure.Soap.Contracts;
using PokedexApi.Gateways;
using PokedexApi.Services;

using PokedexApi.Gateways;
using PokedexApi.Infrastructure.Soap.Contracts;
using PokedexApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


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
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration.GetValue<string>("Authentication:Authority");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidateActor = false,
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidAudience = "pokedex-api",
            ValidateIssuerSigningKey = true
        };
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Read", policy => policy.RequireClaim("http://schemas.microsoft.com/identity/claims/scope", 
                    "read"));
    options.AddPolicy("Write", policy => policy.RequireClaim("http://schemas.microsoft.com/identity/claims/scope",
                    "write"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection(); 
app.MapControllers();

app.Run();