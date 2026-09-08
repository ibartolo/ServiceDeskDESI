namespace ServiceDeskDESIEntities.Tickets
{
    /// <summary>
    /// Ranking de áreas con tickets creados en el rango.
    /// </summary>
    public class RankingAreaDTO
    {
        public long AreaId { get; set; }
        public string AreaNombre { get; set; }
        public int Total { get; set; }
        public decimal PromUrgencia { get; set; }
        public int Cerrados { get; set; }
        public int Rechazados { get; set; }
    }
}
