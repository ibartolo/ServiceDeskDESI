using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Filters;
using ServiceDeskDESIWebApi.Services;
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
        private readonly SucursalService _sucursalService;

        public SucursalController()
        {
            _sucursalService = new SucursalService();
        }

        /// <summary>
        /// Obtiene todas las sucursales de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de sucursales</returns>
        [HttpGet, Route("List")]
        public ModelResponse<List<Sucursal>> ObtenerSucursales()
        {
            var usuario = User.Identity.Name;
            var result = _sucursalService.ObtenerSucursales(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene una sucursal por su ID
        /// </summary>
        /// <param name="id">ID de la sucursal</param>
        /// <returns>Sucursal encontrada</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse<Sucursal> ObtenerSucursalPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = _sucursalService.ObtenerSucursalPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza una sucursal
        /// </summary>
        /// <param name="sucursal">Objeto sucursal con los datos</param>
        /// <returns>Sucursal guardada con su ID actualizado</returns>
        [Permiso("Sucursales")]
        [HttpPost, Route("Guardar")]
        public ModelResponse<Sucursal> GuardarActualizarSucursal(Sucursal sucursal)
        {
            var usuario = User.Identity.Name;
            var result = _sucursalService.GuardarOActualizarSucursal(sucursal, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente una sucursal
        /// </summary>
        /// <param name="sucursal">Sucursal a eliminar (debe incluir Id, ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [Permiso("Sucursales", "Eliminar")]
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarSucursal(Sucursal sucursal)
        {
            var usuario = User.Identity.Name;
            sucursal.FechaModificacion = DateTime.Now;
            var result = _sucursalService.EliminarSucursal(sucursal.Id, sucursal.ModificadoPor, sucursal.FechaModificacion.Value, usuario);
            return result;
        }
    }
}