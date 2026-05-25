using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class UsuarioPagina : BaseObject
    {
        public Usuario Usuarios { get; set; }
        public Pagina  Pagina { get; set; } 
    }
}
