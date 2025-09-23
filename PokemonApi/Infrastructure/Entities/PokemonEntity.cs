using PokemonApi.Infrastructure.Entities;

namespace PokemonApi.Infrastructure.Entities
{

    public class PokemonEntity
    {
        //GUID es una buena opción para la clave primaria, ya que es única y no se repite.
        //Además, permite una mejor escalabilidad en sistemas distribuidos.
        public Guid Id
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public string Type
        {
            get; set;
        }
        public int Level
        {
            get; set;
        }
        // HP (Health Points)

        public int Attack
        {
            get; set;
        }
        public int Defense
        {
            get; set;
        }
        public int Speed
        {
            get; set;
        }
        public int Hp
        {
            get; set;
        }
    }
}