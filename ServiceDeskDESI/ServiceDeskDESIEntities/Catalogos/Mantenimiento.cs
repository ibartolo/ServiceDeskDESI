using System;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class Mantenimiento : BaseObject
    {
        public long ActivoId { get; set; }
        public string Comentario { get; set; }
        public DateTime Fecha { get; set; }
        public long EmpresaId { get; set; }
    }
}
