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
    [RoutePrefix("api/sucursales")]
    public class SucursalController : BaseController
    {
        [HttpGet, Route("List")]
        public ModelResponse ObtenerSucursales()
        {
            var result = dbWrapper.ObtenerTodosLasSucursales();
            return result;
        }
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerSucursalesPorId(long id)
        {
            var result = dbWrapper.ObtenerSucursalesPorId(id);
            return result;
        }
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarActualizarSucursal(Sucursal s)
        {
            var result = dbWrapper.GuardarOActualizarSucursales(s);
            return result;
        }
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarSucursal( Sucursal s)
        {
            s.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarSucursales(s);
            return result;
        }
    }
}
