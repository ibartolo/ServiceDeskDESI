using System.Collections.Generic;

namespace ServiceDeskDESIEntities.Tickets
{
    public class EvidenciaConfigDTO
    {
        public int MaxArchivos { get; set; }
        public int MaxTamanoMB { get; set; }
        public List<string> ExtensionesPermitidas { get; set; }
    }
}
