using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class Relacion : BaseObject
    {
        public Usuarios Usuarios { get; set; }
        public Paginas  Pagina { get; set; }
        public Rol Rol { get; set; }        
    }
}
