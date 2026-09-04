using System;

namespace ServiceDeskDESIEntities.Seguridad
{
    public class RolPaginaAccion : BaseObject
    {
        public long RolId { get; set; }
        public long PaginaId { get; set; }
        public bool PuedeLeer { get; set; }
        public bool PuedeCrear { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
        public bool PuedeExportar { get; set; }
    }
}
