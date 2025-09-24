namespace GardenApi.Dtos;

public class PlantResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string ScientificName { get; set; }
    public required string Family { get; set; }
    public required PlantDataResponse Data { get; set; }
}

public class PlantDataResponse
{
    public int MaxHeight { get; set; }
    public int MaxAge { get; set; }
    public int ConservationLevel { get; set; }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}
