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
    [RoutePrefix("api/TipoActivo")]
    public class TipoActivoController : BaseController
    {
        /// <summary>
        /// Obtiene todos los tipos de activo de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de tipos de activo</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerTodosLosTipoActivos()
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.ObtenerTodosLosTipoActivos(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene un tipo de activo por su ID
        /// </summary>
        /// <param name="id">ID del tipo de activo</param>
        /// <returns>Tipo de activo encontrado</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerTipoActivoPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.ObtenerTipoActivoPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un tipo de activo
        /// </summary>
        /// <param name="tipoActivo">Objeto tipo de activo con los datos</param>
        /// <returns>Tipo de activo guardado con su ID actualizado</returns>
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarTipoActivo(TipoActivo tipoActivo)
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.GuardarOActualizarTipoActivo(tipoActivo, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un tipo de activo
        /// </summary>
        /// <param name="tipoActivo">Tipo de activo a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarTipoActivo(TipoActivo tipoActivo)
        {
            var usuario = User.Identity.Name;
            tipoActivo.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarTipoActivo(tipoActivo.Id, tipoActivo.ModificadoPor, tipoActivo.FechaModificacion.Value, usuario);
            return result;
        }
    }
}