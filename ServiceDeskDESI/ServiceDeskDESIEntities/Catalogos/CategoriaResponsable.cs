using ServiceDeskDESIEntities.Autenticacion;
using System;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class CategoriaResponsable : BaseObject
    {
        public Categoria Categoria { get; set; }
        public Usuario Usuario { get; set; }
        public bool EsPrincipal { get; set; }
    }
}