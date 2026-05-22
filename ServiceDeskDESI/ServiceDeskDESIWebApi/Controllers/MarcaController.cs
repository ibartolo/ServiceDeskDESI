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
        [HttpGet, Route("List")]
        public ModelResponse ObtenerMarcas()
        {
            var result = dbWrapper.ObtenerTodasLasMarcas();
            return result;
        }
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerMarcaPorId(long id)
        {
            var result = dbWrapper.ObtenerMarcasPorId(id);
            return result;
        }
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarMarca(Marca m)
        {
            var result = dbWrapper.GuardarOActualizarMarca(m);
            return result;
        }
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarMarcas(Marca m)
        {
            m.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarMarca(m);
            return result;
        }

    }
}
