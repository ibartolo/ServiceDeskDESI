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
    [Authorize]
    [RoutePrefix("api/Sucursales")]
    public class SucursalController : BaseController
    {
        /// <summary>
        /// Obtiene todas las sucursales de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de sucursales</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerSucursales()
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.ObtenerSucursales(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene una sucursal por su ID
        /// </summary>
        /// <param name="id">ID de la sucursal</param>
        /// <returns>Sucursal encontrada</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerSucursalPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.ObtenerSucursalPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza una sucursal
        /// </summary>
        /// <param name="sucursal">Objeto sucursal con los datos</param>
        /// <returns>Sucursal guardada con su ID actualizado</returns>
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarActualizarSucursal(Sucursal sucursal)
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.GuardarOActualizarSucursal(sucursal, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente una sucursal
        /// </summary>
        /// <param name="sucursal">Sucursal a eliminar (debe incluir Id, ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarSucursal(Sucursal sucursal)
        {
            var usuario = User.Identity.Name;
            sucursal.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarSucursal(sucursal.Id, sucursal.ModificadoPor, sucursal.FechaModificacion.Value, usuario);
            return result;
        }
    }
}