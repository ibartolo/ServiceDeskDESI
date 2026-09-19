using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Seguridad
{
    public class PermisoRequest
    {
        public long PaginaId { get; set; }
        public bool PuedeLeer { get; set; }
        public bool PuedeCrear { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
        public bool PuedeExportar { get; set; }
    }

    public class ValidarPermisoRequest
    {
        public string NombrePagina { get; set; }
        public string Accion { get; set; }
    }

    public class GuardarPermisosRequest
    {
        public long RolId { get; set; }
        public long PaginaId { get; set; }
        public bool PuedeLeer { get; set; }
        public bool PuedeCrear { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
        public bool PuedeExportar { get; set; }
    }

    public class GuardarPermisosMasivoRequest
    {
        public long RolId { get; set; }
        public List<PermisoRequest> Permisos { get; set; }
    }
}
