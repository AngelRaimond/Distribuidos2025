using System;

namespace GardenApi.Models;

public class Plant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string Family { get; set; } = string.Empty;
    public PlantData Data { get; set; } = new();
}

public class PlantData
{
    public int MaxHeight { get; set; }
    public int MaxAge { get; set; }
    public int ConservationLevel { get; set; }
}
