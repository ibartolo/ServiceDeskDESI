using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Permisos")]
    public class PermisosController : BaseController
    {
        private readonly PermisosService _permisosService;

        public PermisosController()
        {
            _permisosService = new PermisosService();
        }

        /// <summary>
        /// Obtiene todos los permisos del usuario autenticado
        /// </summary>
        /// <returns>Lista de permisos por página</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerPermisosPorUsuario()
        {
            var usuario = User.Identity.Name;
            var result = _permisosService.ObtenerPermisosPorUsuario(usuario);
            return result;
        }

        /// <summary>
        /// Valida si el usuario tiene un permiso específico sobre una página
        /// </summary>
        /// <param name="request">Objeto con nombre de página y acción</param>
        /// <returns>True si tiene permiso, False en caso contrario</returns>
        [HttpPost, Route("Validar")]
        public ModelResponse ValidarPermisoUsuario([FromBody] ValidarPermisoRequest request)
        {
            var usuario = User.Identity.Name;
            var result = _permisosService.ValidarPermisoUsuario(usuario, request.NombrePagina, request.Accion);
            return result;
        }

        /// <summary>
        /// Obtiene todas las páginas del sistema
        /// </summary>
        /// <returns>Lista de páginas</returns>
        [HttpGet, Route("Paginas")]
        public ModelResponse ObtenerPaginas()
        {
            var result = _permisosService.ObtenerPaginas();
            return result;
        }

        /// <summary>
        /// Obtiene los permisos de un rol específico
        /// </summary>
        /// <param name="rolId">ID del rol</param>
        /// <returns>Permisos del rol</returns>
        [HttpGet, Route("Rol/{rolId:long}")]
        public ModelResponse ObtenerPermisosPorRol(long rolId)
        {
            var usuario = User.Identity.Name;
            var result = _permisosService.ObtenerPermisosPorRol(rolId, usuario);
            return result;
        }

        /// <summary>
        /// Guarda los permisos de un rol sobre una página
        /// </summary>
        /// <param name="request">Objeto con los datos del permiso</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarPermisosRol([FromBody] GuardarPermisosRequest request)
        {
            var usuario = User.Identity.Name;
            var result = _permisosService.GuardarPermisosRol(
                request.RolId,
                request.PaginaId,
                request.PuedeLeer,
                request.PuedeCrear,
                request.PuedeEditar,
                request.PuedeEliminar,
                request.PuedeExportar,
                usuario,
                usuario
            );
            return result;
        }

        /// <summary>
        /// Guarda todos los permisos de un rol de forma masiva
        /// </summary>
        /// <param name="request">Objeto con RolId y lista de permisos</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost, Route("GuardarMasivo")]
        public ModelResponse GuardarPermisosRolMasivo([FromBody] GuardarPermisosMasivoRequest request)
        {
            var usuario = User.Identity.Name;
            var result = _permisosService.GuardarPermisosRolMasivo(request.RolId, request.Permisos, usuario);
            return result;
        }
    }
}