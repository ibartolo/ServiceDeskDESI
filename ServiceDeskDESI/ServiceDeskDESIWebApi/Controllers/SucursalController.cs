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
    [RoutePrefix("api/Sucursales")]
    public class SucursalController : BaseController
    {
        /// <summary>
        /// Obtiene todas las sucursales de la empresa
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Lista de sucursales</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerSucursales(long empresaId)
        {
            var result = dbWrapper.ObtenerSucursales(empresaId);
            return result;
        }

        /// <summary>
        /// Obtiene una sucursal por su ID
        /// </summary>
        /// <param name="id">ID de la sucursal</param>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Sucursal encontrada</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerSucursalPorId(long id, long empresaId)
        {
            var result = dbWrapper.ObtenerSucursalPorId(id, empresaId);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza una sucursal
        /// </summary>
        /// <param name="sucursal">Objeto sucursal con los datos</param>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Sucursal guardada con su ID actualizado</returns>
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarActualizarSucursal(Sucursal sucursal, long empresaId)
        {
            var result = dbWrapper.GuardarOActualizarSucursal(sucursal, empresaId);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente una sucursal
        /// </summary>
        /// <param name="sucursal">Sucursal a eliminar (debe incluir Id, ModificadoPor)</param>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarSucursal(Sucursal sucursal, long empresaId)
        {
            sucursal.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarSucursal(sucursal.Id, sucursal.ModificadoPor, sucursal.FechaModificacion.Value, empresaId);
            return result;
        }
    }
}