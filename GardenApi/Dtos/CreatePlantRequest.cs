using System.ComponentModel.DataAnnotations;

namespace GardenApi.Dtos;

public class CreatePlantRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string ScientificName { get; set; } = string.Empty;
    [Required] public string Family { get; set; } = string.Empty;
    [Required] public PlantDataRequest Data { get; set; } = new();
}

public class UpdatePlantRequest : CreatePlantRequest
{
    [Required] public Guid Id { get; set; }
}

public class PlantDataRequest
{
    [Range(1,int.MaxValue)] public int MaxHeight { get; set; }
    [Range(1,int.MaxValue)] public int MaxAge { get; set; }
    [Range(0,10)] public int ConservationLevel { get; set; }
}

public class PaginatedResponse<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public IList<T> Items { get; set; } = new List<T>();
}
