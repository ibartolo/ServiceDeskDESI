using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Seguridad
{
    public class RestablecerContraseniaRequest
    {
        public string Token { get; set; }
        public string NuevaContrasena { get; set; }
    }
}
