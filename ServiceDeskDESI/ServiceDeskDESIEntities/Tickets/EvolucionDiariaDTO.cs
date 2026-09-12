using System;

namespace ServiceDeskDESIEntities.Tickets
{
    /// <summary>
    /// Evolución diaria de tickets creados y resueltos (gráfica de línea).
    /// </summary>
    public class EvolucionDiariaDTO
    {
        public DateTime Fecha { get; set; }
        public int Creados { get; set; }
        public int Resueltos { get; set; }
    }
}
