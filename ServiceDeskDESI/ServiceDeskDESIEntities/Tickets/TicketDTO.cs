namespace ServiceDeskDESIEntities.Tickets
{
    public class TicketDTO : Ticket
    {
        public string AreaNombre { get; set; }
        public string CategoriaNombre { get; set; }
        public string SubcategoriaNombre { get; set; }
        public string EstatusNombre { get; set; }
        public string EstatusColor { get; set; }
    }
}
