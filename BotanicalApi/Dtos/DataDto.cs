

using System.Runtime.Serialization;


namespace PlantApi.Dtos
{
    [DataContract(Name = "DataDto", Namespace = "http://plant-api/plant-service")]
    public class DataDto

    // DataDto como nombre es una mala practica, pues Data es a menudo usado en otros contextos,
    // mas en este archivo que existen los DataMember
    //  esto puede causar confusion, aun asi, 
    // es el unico nombre que se le podria poner a esto
    {
        [DataMember(Name = "MaxHeight", Order = 1)]
        public int MaxHeight { get; set; }

        [DataMember(Name = "MaxAge", Order = 2)]
        public int MaxAge { get; set; }

        [DataMember(Name = "ConservationLevel", Order = 3)]
        public int ConservationLevel { get; set; }


    }
}