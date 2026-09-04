using System;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class Foliador
    {
        public long EmpresaId { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Consecutivo { get; set; }
    }
}
