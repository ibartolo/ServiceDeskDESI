using ServiceDeskDESIEntities.Catalogos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Autenticacion
{
    public class Usuario : BaseObject
    {
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
        public string ImagenPerfil { get; set; }
        public string Correo { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Celular { get; set; }
        public long? SucursalId { get; set; }
        public string Firma { get; set; }
        public string RFC { get; set; }
        public long? AreaId { get; set; }
        public long? EmpresaId { get; set; }
        public long? PersonaId { get; set; }
    }
}
