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
    [RoutePrefix("api/Compania")]
    public class CompaniaController : BaseController
    {
        private readonly CompaniaService _companiaService;

        public CompaniaController()
        {
            _companiaService = new CompaniaService();
        }

        /// <summary>
        /// Obtiene todas las compañías de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de compañías</returns>
        [HttpGet, Route("List")]
        public ModelResponse<List<Compania>> ObtenerCompanias()
        {
            var usuario = User.Identity.Name;
            var result = _companiaService.ObtenerCompanias(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene una compañía por su ID
        /// </summary>
        /// <param name="id">ID de la compañía</param>
        /// <returns>Compañía encontrada</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse<Compania> ObtenerCompaniaPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = _companiaService.ObtenerCompaniaPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza una compañía
        /// </summary>
        /// <param name="compania">Objeto compañía con los datos</param>
        /// <returns>Compañía guardada con su ID actualizado</returns>
        [Permiso("Compañías")]
        [HttpPost, Route("Guardar")]
        public ModelResponse<Compania> GuardarOActualizarCompania(Compania compania)
        {
            var usuario = User.Identity.Name;
            var result = _companiaService.GuardarOActualizarCompania(compania, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente una compañía
        /// </summary>
        /// <param name="compania">Compañía a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [Permiso("Compañías", "Eliminar")]
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarCompania(Compania compania)
        {
            var usuario = User.Identity.Name;
            compania.FechaModificacion = DateTime.Now;
            var result = _companiaService.EliminarCompania(compania.Id, compania.ModificadoPor, compania.FechaModificacion.Value, usuario);
            return result;
        }
    }
}