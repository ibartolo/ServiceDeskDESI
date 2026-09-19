namespace ServiceDeskDESIEntities.Tickets
{
    /// <summary>
    /// Distribución de tickets por estatus (gráfica de pastel).
    /// </summary>
    public class DistribucionEstatusDTO
    {
        public int EstatusId { get; set; }
        public string Nombre { get; set; }
        public string Color { get; set; }
        public int Cantidad { get; set; }
    }
}
