using System;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class BitacoraCorreo
    {
        public long Id { get; set; }
        public string TipoCorreo { get; set; }
        public string Destinatario { get; set; }
        public string Asunto { get; set; }
        public string Estado { get; set; }
        public string Error { get; set; }
        public DateTime FechaEnvio { get; set; }
        public long? ReferenciaId { get; set; }
    }
}
