using System;

namespace ServiceDeskDESIEntities.Catalogos
{
    /// <summary>
    /// DTO para renderizar la página anónima de aceptación/desvinculación de una
    /// asignación a partir de su TokenConfirmacion (SP ObtenerAsignacionPorToken).
    /// </summary>
    public class AsignacionActivoDetalleDTO
    {
        public long Id { get; set; }
        public long PersonaId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public DateTime? FechaFin { get; set; }
        public string ActivoNombre { get; set; }
        public string ActivoSerial { get; set; }
        public string TipoActivoNombre { get; set; }
        public string MarcaNombre { get; set; }
        public string ModeloNombre { get; set; }
        public string PersonaNombre { get; set; }
        public string PersonaApellido { get; set; }
        public string AsignadorNombre { get; set; }
    }
}
