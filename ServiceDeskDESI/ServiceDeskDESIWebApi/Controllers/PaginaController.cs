using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Pagina")]
    public class PaginaController : BaseController
    {
        /// <summary>
        /// Obtiene todas las páginas a las que tiene acceso el usuario autenticado
        /// </summary>
        /// <returns>Lista de páginas</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerPaginasPorUsuario()
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.ObtenerPaginasPorUsuario(usuario);
            return result;
        }

        /// <summary>
        /// Valida si el usuario autenticado tiene acceso a una página específica
        /// </summary>
        /// <param name="request">Objeto con la dirección de la página</param>
        /// <returns>True si tiene acceso, False en caso contrario</returns>
        [HttpPost, Route("ValidarAcceso")]
        public ModelResponse ValidarAccesoPagina([FromBody] ValidarAccesoRequest request)
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.ValidarAccesoPagina(usuario, request.Direccion);
            return result;
        }
    }

    public class ValidarAccesoRequest
    {
        public string Direccion { get; set; }
    }
}