namespace ServiceDeskDESIWebApi.Models
{
    public class ReasignarTicketRequest
    {
        public long TicketId { get; set; }
        public long NuevoUsuarioId { get; set; }
        public string Comentario { get; set; }
    }
}
