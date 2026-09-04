namespace ServiceDeskDESIEntities.Tickets
{
    public class EvidenciaDescargaDTO
    {
        public string NombreArchivo { get; set; }
        public string ContentType { get; set; }
        public byte[] Contenido { get; set; }
    }
}
