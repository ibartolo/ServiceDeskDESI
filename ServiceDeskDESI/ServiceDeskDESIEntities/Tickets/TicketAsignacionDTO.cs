namespace ServiceDeskDESIEntities.Tickets
{
    public class TicketAsignacionDTO : TicketAsignacion
    {
        public string AgenteNombre { get; set; }
        public string AgenteApellido { get; set; }
        public string AgenteNombreUsuario { get; set; }
        public string EstatusNombre { get; set; }
        public string EstatusColor { get; set; }
    }
}
