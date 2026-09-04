using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ServiceDeskDESIMVC.Helpers
{
    // Los atributos AutenticatedAttribute y NoAutenticatedAttribute fueron retirados.
    // La autenticación la impone ahora el filtro global (App_Start/FilterConfig.cs) y
    // los permisos de escritura el atributo [Permiso] (Filters/PermisoAttribute.cs).
    public class FiltersHelper
    {
    }
}