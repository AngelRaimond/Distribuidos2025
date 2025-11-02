using MongoDB.Driver;
using BiologistApi.Services;
using BiologistApi.Infrastructure;
using BiologistApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDBSettings>(
builder.Configuration.GetSection("MongoDB"));
builder.Services.AddScoped<IBiologistRepository, BiologistRepository>();

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDB").Get<MongoDBSettings>()!;
    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
});

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<BiologistService>();
app.MapGet("/", () => "C");

app.Run();
