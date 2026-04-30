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
        public long TipoActivoID { get; set; }
        public string Serial { get; set; }
        public long  MarcaID { get; set; }
        public long ModeloID { get; set; }
        public string Notas { get; set; }
        public DateTime FechaCompra { get; set; }
  }
}
