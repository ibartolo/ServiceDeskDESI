using System;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class PersonaActivo : BaseObject
    {
        public long PersonaId { get; set; }
        public long ActivoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
