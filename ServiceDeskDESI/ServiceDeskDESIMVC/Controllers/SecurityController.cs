using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using static ServiceDeskDESIMVC.Helpers.FiltersHelper;

namespace ServiceDeskDESIMVC.Controllers
{
    [Autenticated]
    public class SecurityController : BaseController
    {
        private readonly PermisosService _permisosService;
        private readonly RolService _rolService;

        public SecurityController()
        {
            _permisosService = new PermisosService(httpClientConnection);
            _rolService = new RolService(httpClientConnection);
        }

        #region Views

        public async Task<ActionResult> Role(long id = 0)
        {
            var rol = new Rol();
            if (id > 0)
            {
                var response = await _rolService.ObtenerRolPorId(id);
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

        public async Task<ActionResult> Permisos()
        {
            // Obtener todos los roles
            var rolesResponse = await _rolService.ObtenerTodosLosRoles();
            var roles = new List<Rol>();
            if (rolesResponse.IsSuccess && rolesResponse.Response != null)
            {
                roles = JsonConvert.DeserializeObject<List<Rol>>(rolesResponse.Response.ToString());
            }
            ViewBag.Roles = roles;

            // Obtener todas las páginas
            var paginasResponse = await _permisosService.ObtenerPaginas();
            var paginas = new List<Pagina>();
            if (paginasResponse.IsSuccess && paginasResponse.Response != null)
            {
                paginas = JsonConvert.DeserializeObject<List<Pagina>>(paginasResponse.Response.ToString());
            }
            ViewBag.Paginas = paginas;

            // Obtener permisos del usuario para esta página
            var permisosUser = await _permisosService.ObtenerPermisosParaPagina("Permisos");
            var permiso = permisosUser.FirstOrDefault();

            ViewBag.PuedeLeer = permiso?.PuedeLeer ?? false;
            ViewBag.PuedeCrear = permiso?.PuedeCrear ?? false;
            ViewBag.PuedeEditar = permiso?.PuedeEditar ?? false;
            ViewBag.PuedeEliminar = permiso?.PuedeEliminar ?? false;
            ViewBag.PuedeExportar = permiso?.PuedeExportar ?? false;

            return View();
        }

        #endregion

        #region Data Access

        public async Task<string> ConsultarTodosLosRoles()
        {
            var response = await _rolService.ObtenerTodosLosRoles();
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTodosLosRolesPorId(long id)
        {
            var response = await _rolService.ObtenerRolPorId(id);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> GuardarOActualizarRol(Rol r)
        {
            var response = await _rolService.GuardarOActualizarRol(r);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> EliminarRol(Rol r)
        {
            var response = await _rolService.EliminarRol(r);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ObtenerPermisosPorRol(long rolId)
        {
            var response = await _permisosService.ObtenerPermisosPorRol(rolId);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> GuardarPermisosRol(GuardarPermisosRequest request)
        {
            var response = await _permisosService.GuardarPermisosRol(request);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> GuardarPermisosRolMasivo([FromBody] GuardarPermisosMasivoRequest request)
        {
            var response = await _permisosService.GuardarPermisosRolMasivo(request);
            return JsonConvert.SerializeObject(response);
        }

        #endregion
    }
}