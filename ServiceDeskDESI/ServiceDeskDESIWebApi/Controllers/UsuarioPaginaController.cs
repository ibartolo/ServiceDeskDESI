using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/UsuarioPagina")]
    public class UsuarioPaginaController : BaseController
    {
        [HttpGet, Route("List")]
        public ModelResponse ObtenerUsuarioPagina()
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.ObtenerUsuarioPagina(usuario);
            return result;
        }
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerUsuarioPaginaPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.ObtenerUsuarioPaginaPorId(id, usuario);
            return result;
        }
        [Permiso("Permisos")]
        [HttpPost, Route("")]
        public ModelResponse GuardarOActualizarUsuarioPagina(UsuarioPagina r)
        {
            var result = dbWrapper.GuardarOActualizarUsuarioPagina(r);
            return result;
        }
        public ModelResponse EliminarUsuarioPagina(UsuarioPagina r)
        {
            var result = dbWrapper.EliminarUsuarioPagina(r.Id, r.ModificadoPor, r.FechaModificacion.Value);
            return result;
        }
    }
}
