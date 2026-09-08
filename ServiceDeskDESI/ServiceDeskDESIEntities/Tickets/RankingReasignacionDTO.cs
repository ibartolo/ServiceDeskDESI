namespace ServiceDeskDESIEntities.Tickets
{
    /// <summary>
    /// Ranking de reasignaciones (Reciben / Quitan) por agente.
    /// </summary>
    public class RankingReasignacionDTO
    {
        public string Tipo { get; set; }
        public long UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreUsuario { get; set; }
        public int Cantidad { get; set; }
    }
}
