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
    [RoutePrefix("api/Modelo")]
    public class ModeloController : BaseController
    {
        [HttpGet, Route("List/{empresaId:long}")]
        public ModelResponse ObtenerModelos(long empresaId)
        {
            var result = dbWrapper.ObtenerModelos(empresaId);
            return result;
        }
        [HttpGet, Route("{id:long/{empresaId:long}}")]
        public ModelResponse ObtenerModelosPorId(long id, long empresaId)
        {
            var result = dbWrapper.ObtenerModeloPorId(id,empresaId);
            return result;
        }
        [HttpPost, Route("Guardar/{empresaId:long}")]
        public ModelResponse GuardarOActualizarModelos(Modelo m,long empresaId)
        {
            var result = dbWrapper.GuardarOActualizarModelo(m,empresaId);
            return result;
        }
        [HttpDelete, Route("Eliminar/{empresaId:long}")]
        public ModelResponse EliminarModelos(Modelo m, long empresaId)
        {
            m.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarModelo(m.Id,m.ModificadoPor,m.FechaModificacion.Value,empresaId);
            return result;
        }
    }
}
