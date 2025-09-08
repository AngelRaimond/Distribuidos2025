namespace PlantApi.Models;

public class Plant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string ScientificName { get; set; } = null!;
    public string Family { get; set; }

    public Data Data { get; set; }
}
