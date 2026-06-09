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
    [RoutePrefix("api/Marca")]
    public class MarcaController : BaseController
    {
        [HttpGet, Route("List/{empresaId:long}")]
        public ModelResponse ObtenerMarcas(long empresaId)
        {
            var result = dbWrapper.ObtenerTodasLasMarcas(empresaId);
            return result;
        }
        [HttpGet, Route("{id:long}/{empresaId:long}")]
        public ModelResponse ObtenerMarcaPorId(long id, long empresaId)
        {
            var result = dbWrapper.ObtenerMarcasPorId(id, empresaId);
            return result;
        }
        [HttpPost, Route("Guardar/{empresaId:long")]
        public ModelResponse GuardarOActualizarMarca(Marca m,long empresaId)
        {
            var result = dbWrapper.GuardarOActualizarMarca(m, empresaId);
            return result;
        }
        [HttpDelete, Route("Eliminar/{empresaId:long}")]
        public ModelResponse EliminarMarcas(Marca m, long empresaId)
        {
            m.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarMarca(m.Id,m.ModificadoPor,m.FechaModificacion.Value,empresaId);
            return result;
        }

    }
}
