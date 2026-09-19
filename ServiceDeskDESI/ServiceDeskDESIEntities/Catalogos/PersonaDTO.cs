namespace ServiceDeskDESIEntities.Catalogos
{
    public class PersonaDTO : Persona
    {
        public string PuestoNombre { get; set; }
        public string PuestoDescripcion { get; set; }
        public long? UsuarioId { get; set; }
        public string NombreUsuarioVinculado { get; set; }
    }
}
