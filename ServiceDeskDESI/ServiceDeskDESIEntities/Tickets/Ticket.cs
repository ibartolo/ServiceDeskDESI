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
        public long AreaId { get; set; }
        public long CategoriaId { get; set; }
        public long? SubcategoriaId { get; set; }
        public int Urgencia { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int TicketEstatusId { get; set; }
    }
}
