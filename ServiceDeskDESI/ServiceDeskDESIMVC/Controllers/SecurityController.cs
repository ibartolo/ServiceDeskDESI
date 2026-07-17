using Newtonsoft.Json;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
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
    public class SecurityController : BaseController
    {
        #region Views
        public async Task<ActionResult> Role(long id = 0)
        {
            var rol = new Rol();
            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerRolPorId(id);
                if (response.IsSuccess && response.Response != null)
                {
                    rol = JsonConvert.DeserializeObject<Rol>(response.Response.ToString());
                }
                else
                {
                    ViewBag.ErrorMessage = response.Message;
                }
            }
            return View(rol);
        }
        #endregion

        #region Data Access
        public async Task<string> ConsultarTodosLosRoles()
        {
            var response = await httpClientConnection.ObtenerTodosLosRoles();
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodosLosRolesPorId(long id)
        {
            var response = await httpClientConnection.ObtenerRolPorId(id);
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> GuardarOActualizarRol(Rol r)
        {
            var response = await httpClientConnection.GuardarOActualizarRol(r);
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> EliminarRol(Rol r)
        {
            var response = await httpClientConnection.EliminarRol(r);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}