using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class Persona : BaseObject
    {
        public string Nombre { get; set; }
        public string Apellido  { get; set; }

        public string Correo { get; set; }
        public string Telefono { get; set; }
        public Puesto Puesto { get; set; }

    }
}
