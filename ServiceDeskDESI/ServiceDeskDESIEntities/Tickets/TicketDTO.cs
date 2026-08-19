namespace ServiceDeskDESIEntities.Tickets
{
    public class TicketDTO : Ticket
    {
        public string AreaNombre { get; set; }
        public string CategoriaNombre { get; set; }
        public string SubcategoriaNombre { get; set; }
        public string EstatusNombre { get; set; }
        public string EstatusColor { get; set; }
        public long? AgenteId { get; set; }
        public string AgenteNombre { get; set; }
        public string AgenteApellido { get; set; }
        public string AgenteNombreUsuario { get; set; }
    }
}
