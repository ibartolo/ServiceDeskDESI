namespace ServiceDeskDESIEntities.Tickets
{
    /// <summary>
    /// Resumen de KPIs del módulo de Estadísticas (una sola fila).
    /// </summary>
    public class MetricasResumenDTO
    {
        public int Total { get; set; }
        public int Nuevos { get; set; }
        public int EnProgreso { get; set; }
        public int Resueltos { get; set; }
        public int Cerrados { get; set; }
        public int Rechazados { get; set; }
        public decimal Eficiencia { get; set; }
        public decimal HorasPromedioResolucion { get; set; }
    }
}
