using System;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class HorarioLaboral : BaseObject
    {
        public long EmpresaId { get; set; }
        public int DiaSemana { get; set; }        // 1=Lun..7=Dom
        public DateTime? HoraInicio { get; set; } // datetime NULL (ancla 1900-01-01)
        public DateTime? HoraFin { get; set; }
    }
}
