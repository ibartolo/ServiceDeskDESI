namespace ServiceDeskDESIEntities.Catalogos
{
    public class PersonaActivoDTO : PersonaActivo
    {
        public string ActivoNombre { get; set; }
        public string ActivoSerial { get; set; }
        public string TipoActivoNombre { get; set; }
        public string MarcaNombre { get; set; }
        public string ModeloNombre { get; set; }
        public string PersonaNombre { get; set; }
        public string PersonaApellido { get; set; }
        public string AsignadoPor { get; set; }
    }
}
