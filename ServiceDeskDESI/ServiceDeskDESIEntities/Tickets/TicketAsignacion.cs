using System;

namespace ServiceDeskDESIEntities.Tickets
{
    /// <summary>
    /// Asignación de un agente a un ticket. Cada fila es una asignación; la vigente tiene EsActiva = true.
    /// Las filas anteriores (EsActiva = false) constituyen el histórico de reasignaciones.
    /// </summary>
    public class TicketAsignacion : BaseObject
    {
        public long TicketId { get; set; }
        public long UsuarioId { get; set; }
        public string Comentario { get; set; }
        public bool EsActiva { get; set; }
    }
}
