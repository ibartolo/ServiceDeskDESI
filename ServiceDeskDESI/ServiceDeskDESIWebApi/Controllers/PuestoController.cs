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
    [RoutePrefix("api/Puesto")]
    public class PuestoController : BaseController
    {
        private readonly PuestoService _puestoService;

        public PuestoController()
        {
            _puestoService = new PuestoService ();
        }

        /// <summary>
        /// Obtiene todos los puestos de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de puestos</returns>
        [HttpGet, Route("List")]
        public ModelResponse<List<Puesto>> ObtenerTodosLosPuestos()
        {
            var usuario = User.Identity.Name;
            var result = _puestoService.ObtenerTodosLosPuestos(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene un puesto por su ID
        /// </summary>
        /// <param name="id">ID del puesto</param>
        /// <returns>Puesto encontrado</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse<Puesto> ObtenerPuestoPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = _puestoService.ObtenerPuestoPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un puesto
        /// </summary>
        /// <param name="puesto">Objeto puesto con los datos</param>
        /// <returns>Puesto guardado con su ID actualizado</returns>
        [Permiso("Tipped")]
        [HttpPost, Route("Guardar")]
        public ModelResponse<Puesto> GuardarOActualizarPuesto(Puesto puesto)
        {
            var usuario = User.Identity.Name;
            var result = _puestoService.GuardarOActualizarPuesto(puesto, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un puesto
        /// </summary>
        /// <param name="puesto">Puesto a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [Permiso("Tipped", "Eliminar")]
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarPuesto(Puesto puesto)
        {
            var usuario = User.Identity.Name;
            puesto.FechaModificacion = DateTime.Now;
            var result = _puestoService.EliminarPuesto(puesto.Id, puesto.ModificadoPor, puesto.FechaModificacion.Value, usuario);
            return result;
        }
    }
}
    