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
    }

    public class ValidarPermisoRequest
    {
        public string NombrePagina { get; set; }
        public string Accion { get; set; } // Leer, Crear, Editar, Eliminar, Exportar
    }
}