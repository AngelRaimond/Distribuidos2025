using System;
using System.ServiceModel;
using PlantApi.Models;
     


namespace PlantApi.Services     // <-- IMPORTANTE: mismo namespace que usa PlantService y sus DTOs
{
    // Se asume que CreatePlantDto y UpdatePlantDto están en PlantApi.Services
    // con estas formas mínimas (propiedades que usa el mapper).
    // Si ya existen en tu código, NO necesitas declarar nada extra aquí.

    public static class PlantMapper
    {
        /// <summary>
        /// Mapea CreatePlantDto -> Plant tolerando dto.Data == null.
        /// </summary>
        public static Plant ToModel(CreatePlantDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var name = (dto.Name ?? string.Empty).Trim();
            var sci  = (dto.ScientificName ?? string.Empty).Trim();
            var fam  = (dto.Family ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
                throw new FaultException("Name is required");
            if (string.IsNullOrWhiteSpace(sci))
                throw new FaultException("ScientificName is required");
            if (string.IsNullOrWhiteSpace(fam))
                throw new FaultException("Family is required");

            // Tolerar Data == null y/o campos planos en el dto
            var maxHeight         = dto.Data?.MaxHeight         ?? dto.MaxHeight         ?? 0;
            var maxAge            = dto.Data?.MaxAge            ?? dto.MaxAge            ?? 0;
            var conservationLevel = dto.Data?.ConservationLevel ?? dto.ConservationLevel ?? 0;

            return new Plant
            {
                Id                = Guid.NewGuid().ToString(), // varchar(36)
                Name              = name,
                ScientificName    = sci,
                Family            = fam,
                MaxHeight         = maxHeight,
                MaxAge            = maxAge,
                ConservationLevel = conservationLevel
            };
        }

        /// <summary>
        /// Aplica UpdatePlantDto a una entidad existente, tolerando dto.Data == null.
        /// </summary>
        public static void ApplyUpdate(UpdatePlantDto dto, Plant entity)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (!string.IsNullOrWhiteSpace(dto.Name))
                entity.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.ScientificName))
                entity.ScientificName = dto.ScientificName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Family))
                entity.Family = dto.Family.Trim();

            var hasData = dto.Data != null;

            if (hasData && dto.Data.MaxHeight.HasValue)
                entity.MaxHeight = dto.Data.MaxHeight.Value;
            else if (dto.MaxHeight.HasValue)
                entity.MaxHeight = dto.MaxHeight.Value;

            if (hasData && dto.Data.MaxAge.HasValue)
                entity.MaxAge = dto.Data.MaxAge.Value;
            else if (dto.MaxAge.HasValue)
                entity.MaxAge = dto.MaxAge.Value;

            if (hasData && dto.Data.ConservationLevel.HasValue)
                entity.ConservationLevel = dto.Data.ConservationLevel.Value;
            else if (dto.ConservationLevel.HasValue)
                entity.ConservationLevel = dto.ConservationLevel.Value;
        }
    }
}
