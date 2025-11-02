namespace BiologistApi.Models;

public class Biologist
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AreaModel> Areas { get; set; } = new();
}

public class AreaModel
{
    public string Area { get; set; } = null!;
    public SubArea SubArea { get; set; }
}

// Compatibility wrapper: keep `Area` type name for existing code that expects it
public class Area : AreaModel {}

public enum SubArea
{
    Unknown = 0,
    Gold = 1,
    Silver = 2,
    Bronze = 3
}