using Microsoft.EntityFrameworkCore;
using PlantApi.Infrastructure;
using PlantApi.Repositories;
using PlantApi.Services;
using SoapCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSoapCore();
builder.Services.AddScoped<IPlantService, PlantService>();
builder.Services.AddScoped<IPlantRepository, PlantRepository>();

builder.Services.AddDbContext<RelationalDbContext>(options =>
options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

var app = builder.Build();
    app.UseSoapEndpoint<IPlantService>("/PlantService.svc", new SoapEncoderOptions());
    app.Run();