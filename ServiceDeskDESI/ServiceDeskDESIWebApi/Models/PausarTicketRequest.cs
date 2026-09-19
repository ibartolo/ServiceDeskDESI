using System;

namespace ServiceDeskDESIWebApi.Models
{
    public class PausarTicketRequest
    {
        public long TicketId { get; set; }
        public string TipoPausa { get; set; }
        public string Comentario { get; set; }
        public DateTime? FechaEstimada { get; set; }
    }
}
