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
    [RoutePrefix("api/Activo")]
    public class ActivoController : BaseController
    {
        private readonly ActivoService _activoService;

        public ActivoController()
        {
            _activoService = new ActivoService();
        }

        /// <summary>
        /// Obtiene todos los activos de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de activos</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerTodosLosActivos()
        {
            var usuario = User.Identity.Name;
            var result = _activoService.ObtenerTodosLosActivos(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene un activo por su ID
        /// </summary>
        /// <param name="id">ID del activo</param>
        /// <returns>Activo encontrado</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerActivoPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = _activoService.ObtenerActivoPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un activo
        /// </summary>
        /// <param name="activo">Objeto activo con los datos</param>
        /// <returns>Activo guardado con su ID actualizado</returns>
        [Permiso("Activos")]
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarActivo(Activo activo)
        {
            var usuario = User.Identity.Name;
            var result = _activoService.GuardarOActualizarActivo(activo, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un activo
        /// </summary>
        /// <param name="activo">Activo a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [Permiso("Activos", "Eliminar")]
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarActivo(Activo activo)
        {
            var usuario = User.Identity.Name;
            activo.FechaModificacion = DateTime.Now;
            var result = _activoService.EliminarActivo(activo.Id, activo.ModificadoPor, activo.FechaModificacion.Value, usuario);
            return result;
        }
    }
}