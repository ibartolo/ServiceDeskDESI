using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [RoutePrefix("api/modelo")]
    public class ModeloController : BaseController
    {
        [HttpGet, Route("List")]
        public ModelResponse ObtenerModelos()
        {
            var result = dbWrapper.ObtenerTodosLosModelos();
            return result;
        }
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerModelosPorId(long id)
        {
            var result = dbWrapper.ObtnerModeloPorId(id);
            return result;
        }
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarModelos(Modelo m)
        {
            var result = dbWrapper.GuardarOActualizarModelo(m);
            return result;
        }
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarModelos(Modelo m)
        {
            m.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarModelo(m.Id,m.ModificadoPor,m.FechaModificacion.Value);
            return result;
        }
    }
}
