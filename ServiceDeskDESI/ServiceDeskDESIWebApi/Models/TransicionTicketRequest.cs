namespace ServiceDeskDESIWebApi.Models
{
    public class TransicionTicketRequest
    {
        public long TicketId { get; set; }
        public string Comentario { get; set; }
        public long? NuevoUsuarioId { get; set; }
    }
}
