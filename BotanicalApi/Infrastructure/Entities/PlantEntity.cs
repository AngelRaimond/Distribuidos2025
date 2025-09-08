namespace PlantApi.Infrastructure.Entities;

public class PlantEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string ScientificName { get; set; }
    public string Family { get; set; }
    public int MaxHeight/*enM?*/{ get; set; } //una buena practica que me gustaria consultar es 
                                              // si considera necesario el definir la unidad de medida en el nombre
                                              //veo que a muchos programadores les gusta hacerlo pero no se si SIEMPRE sea necesario
    public int MaxAge { get; set; }
    public int ConservationLevel { get; set; }
}
