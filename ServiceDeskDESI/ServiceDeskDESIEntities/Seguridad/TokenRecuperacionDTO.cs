using System;

namespace ServiceDeskDESIEntities.Seguridad
{
    public class TokenRecuperacionDTO : TokenRecuperacion
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string NombreUsuario { get; set; }
    }
}
