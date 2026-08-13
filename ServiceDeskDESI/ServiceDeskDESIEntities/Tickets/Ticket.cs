using ServiceDeskDESIEntities.Catalogos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Tickets
{
    public class Ticket : BaseObject
    {
        public Area Area { get; set; }
        public Categoria Categoria { get; set; }
        public Categoria Subcategoria { get; set; }
        public int Urgencia { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public TicketEstatus TicketEstatus { get; set; }
    }
}
