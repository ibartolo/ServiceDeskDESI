using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Catalogos
{
  public class Activo:BaseObject
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public TipoActivo TipoActivo{ get; set; }
        public string Serial { get; set; }
        public Marca Marca { get; set; }
        public Modelo Modelo{ get; set; }
        public string Notas { get; set; }
        public DateTime FechaCompra { get; set; }
  }
}
