using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Seguridad
{
    public class PermisosViewModel
    {
        public long PaginaId { get; set; }
        public string PaginaNombre { get; set; }
        public string Direccion { get; set; }
        public bool PuedeLeer { get; set; }
        public bool PuedeCrear { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
        public bool PuedeExportar { get; set; }
    }
}
