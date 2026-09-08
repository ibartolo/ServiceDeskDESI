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
using ServiceDeskDESIMVC.Filters;

namespace ServiceDeskDESIMVC.Controllers
{
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
            // 1. Obtener permisos para la página "Roles"
            var permisos = await _rolService.ObtenerPermisosParaRol();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            var rol = new Rol();

            if (id > 0)
            {
                var response = await _rolService.ObtenerRolPorId(id);
                if (response != null)
                {
                    rol = response;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se encontró el rol.";
                }
            }

            // 3. Pasar permisos a la vista
            ViewBag.Permisos = permisos;

            return View(rol);
        }

        public async Task<ActionResult> Permisos()
        {
            // 1. Obtener permisos para la página "Permisos"
            var permisos = await _permisosService.ObtenerPermisosParaPermisos();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            // 3. Obtener todos los roles
            var rolesResponse = await _rolService.ObtenerTodosLosRoles();
            var roles = new List<Rol>();
            if (rolesResponse.IsSuccess && rolesResponse.Response != null)
            {
                roles = rolesResponse.Response;
            }
            ViewBag.Roles = roles;

            // 4. Obtener todas las páginas
            var paginasResponse = await _permisosService.ObtenerPaginas();
            var paginas = new List<Pagina>();
            if (paginasResponse.IsSuccess && paginasResponse.Response != null)
            {
                paginas = paginasResponse.Response;
            }
            ViewBag.Paginas = paginas;

            // 5. Pasar permisos del usuario para la vista (ya los tenemos en permisos)
            ViewBag.PuedeLeer = ((PermisosViewModel)permisos).PuedeLeer;
            ViewBag.PuedeCrear = ((PermisosViewModel)permisos).PuedeCrear;
            ViewBag.PuedeEditar = ((PermisosViewModel)permisos).PuedeEditar;
            ViewBag.PuedeEliminar = ((PermisosViewModel)permisos).PuedeEliminar;
            ViewBag.PuedeExportar = ((PermisosViewModel)permisos).PuedeExportar;

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

        [Permiso("Roles")]
        public async Task<string> GuardarOActualizarRol(Rol r)
        {
            var response = await _rolService.GuardarOActualizarRol(r);
            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Roles", "Eliminar")]
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

        [Permiso("Permisos", "Leer")]
        public async Task<string> ConsultarConteoPaginasPorRol()
        {
            var response = await _permisosService.ObtenerConteoPaginasPorRol();
            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Permisos", "Editar")]
        public async Task<string> GuardarPermisosRol(GuardarPermisosRequest request)
        {
            var response = await _permisosService.GuardarPermisosRol(request);
            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Permisos", "Editar")]
        public async Task<string> GuardarPermisosRolMasivo([FromBody] GuardarPermisosMasivoRequest request)
        {
            var response = await _permisosService.GuardarPermisosRolMasivo(request);
            return JsonConvert.SerializeObject(response);
        }

        #endregion
    }
}