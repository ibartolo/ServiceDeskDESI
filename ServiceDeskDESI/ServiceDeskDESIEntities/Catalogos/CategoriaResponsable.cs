using ServiceDeskDESIEntities.Autenticacion;
using System;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class CategoriaResponsable : BaseObject
    {
        public long CategoriaId { get; set; }
        public long UsuarioId { get; set; }
        public bool EsPrincipal { get; set; }
    }
}