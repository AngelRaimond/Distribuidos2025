

using System.Runtime.Serialization;


namespace PlantApi.Dtos
{
    [DataContract(Name = "DataDto", Namespace = "http://plant-api/plant-service")]
    public class DataDto

    {
        [DataMember(Name = "MaxHeight", Order = 1)]
        public int MaxHeight { get; set; }

        [DataMember(Name = "MaxAge", Order = 2)]
        public int MaxAge { get; set; }

        [DataMember(Name = "ConservationLevel", Order = 3)]
        public int ConservationLevel { get; set; }


    }
}