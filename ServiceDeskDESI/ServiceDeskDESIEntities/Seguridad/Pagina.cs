using System;

namespace ServiceDeskDESIEntities.Seguridad
{
    public class Pagina : BaseObject
    {
        public string Nombre { get; set; }
        public string NombreVisible { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; } // Menu, SubMenu
        public string Direccion { get; set; }
        public long? PermisosPadreId { get; set; } // Para jerarquía (menú padre)
        public string Logo { get; set; } // Ícono FontAwesome (ej: fa-users)
        public int OrdenB { get; set; } // Orden de visualización
    }
}