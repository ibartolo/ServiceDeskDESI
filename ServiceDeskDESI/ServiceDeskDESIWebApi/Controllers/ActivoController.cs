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
    [RoutePrefix("api/Activo")]
    public class ActivoController : BaseController
    {
        /// <summary>
        /// Obtiene todos los activos de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de activos</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerTodosLosActivos()
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.ObtenerTodosLosActivos(usuario);
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
            var result = dbWrapper.ObtenerActivoPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un activo
        /// </summary>
        /// <param name="activo">Objeto activo con los datos</param>
        /// <returns>Activo guardado con su ID actualizado</returns>
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarActivo(Activo activo)
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.GuardarOActualizarActivo(activo, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un activo
        /// </summary>
        /// <param name="activo">Activo a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarActivo(Activo activo)
        {
            var usuario = User.Identity.Name;
            activo.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarActivo(activo.Id, activo.ModificadoPor, activo.FechaModificacion.Value, usuario);
            return result;
        }
    }
}