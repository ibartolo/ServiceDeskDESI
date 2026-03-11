using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [RoutePrefix("api/Autentication")]
    public class AutenticationController : BaseController
    {
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerUsuarioPorId(long id)
        {
            var result = dbWrapper.ObtenerUsuarioPorId(id);
            return result;
        }
        [HttpGet, Route("Lista")]
        public ModelResponse ObtenerUsuarios()
        {
            var result = dbWrapper.ObtenerUsuarios();
            return result;
        }
        [HttpPost, Route("")]
        public ModelResponse GuardarOActualizarUsuario(Usuario u)
        {
            var result = dbWrapper.GuardarOActualizarUsuario(u);
            return result;
        }
        [HttpDelete, Route("")]
        public ModelResponse EliminarUsuario(Usuario u)
        {
            u.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarUsuario(u.Id, u.ModificadoPor, u.FechaModificacion.Value);
            return result;
        }
    }
}
