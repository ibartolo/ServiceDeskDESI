using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class TicketEstatus : BaseObject
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Color { get; set; }
        public int Orden { get; set; }
    }
}
