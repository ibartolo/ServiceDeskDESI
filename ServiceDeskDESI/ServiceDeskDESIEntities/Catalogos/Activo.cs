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
        public long? TipoActivoId { get; set; }
        public string Serial { get; set; }
        public long? MarcaId { get; set; }
        public long? ModeloId { get; set; }
        public string Notas { get; set; }
        public DateTime? FechaCompra { get; set; }
  }
}
