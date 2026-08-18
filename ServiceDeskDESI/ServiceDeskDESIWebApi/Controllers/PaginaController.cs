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
    [RoutePrefix("api/Pagina")]
    public class PaginaController : BaseController
    {
        private readonly PaginaService _paginaService;

        public PaginaController()
        {
            _paginaService = new PaginaService();
        }

        /// <summary>
        /// Obtiene todas las páginas a las que tiene acceso el usuario autenticado
        /// </summary>
        /// <returns>Lista de páginas</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerPaginasPorUsuario()
        {
            var usuario = User.Identity.Name;
            var result = _paginaService.ObtenerPaginasPorUsuario(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene todas las páginas del sistema (activas)
        /// </summary>
        /// <returns>Lista de páginas</returns>
        [HttpGet, Route("Todas")]
        public ModelResponse ObtenerPaginas()
        {
            var result = _paginaService.ObtenerPaginas();
            return result;
        }

        /// <summary>
        /// Obtiene una página por su nombre
        /// </summary>
        /// <param name="nombre">Nombre de la página</param>
        /// <returns>Página encontrada</returns>
        [HttpGet, Route("PorNombre/{nombre}")]
        public ModelResponse ObtenerPaginaPorNombre(string nombre)
        {
            var result = _paginaService.ObtenerPaginaPorNombre(nombre);
            return result;
        }
    }
}