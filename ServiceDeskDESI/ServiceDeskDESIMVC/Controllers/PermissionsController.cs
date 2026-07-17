using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static ServiceDeskDESIMVC.Helpers.FiltersHelper;

namespace ServiceDeskDESIMVC.Controllers
{
    [Autenticated]
    public class PermissionsController : BaseController
    {
        public async Task<string> ConsultarPermisosUsuario()
        {
            var response = await httpClientConnection.ObtenerPermisosPorUsuario();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        public async Task<string> ValidarPermisoUsuario(string nombrePagina, string accion)
        {
            var response = await httpClientConnection.ValidarPermisoUsuario(nombrePagina, accion);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
    }
}