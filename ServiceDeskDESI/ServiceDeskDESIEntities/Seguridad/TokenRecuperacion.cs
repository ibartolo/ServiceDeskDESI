using System;

namespace ServiceDeskDESIEntities.Seguridad
{
    public class TokenRecuperacion : BaseObject
    {
        public long UsuarioId { get; set; }
        public string Token { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Usado { get; set; }
    }
}
