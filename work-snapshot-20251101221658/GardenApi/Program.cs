using GardenApi.Services;
using GardenApi.Gateways;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar servicios
builder.Services.AddScoped<IPlantService, PlantService>();
builder.Services.AddScoped<IPlantGateway, PlantGateway>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();