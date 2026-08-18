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
    [RoutePrefix("api/Modelo")]
    public class ModeloController : BaseController
    {
        private readonly ModeloService _modeloService;
        public ModeloController()
        {
            _modeloService = new ModeloService();
        }
        /// <summary>
        /// Obtiene todos los modelos de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de modelos</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerTodosLosModelos()
        {
            var usuario = User.Identity.Name;
            var result = _modeloService.ObtenerModelos(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene un modelo por su ID
        /// </summary>
        /// <param name="id">ID del modelo</param>
        /// <returns>Modelo encontrado</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerModeloPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = _modeloService.ObtenerModeloPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Obtiene modelos por marca
        /// </summary>
        /// <param name="marcaId">ID de la marca</param>
        /// <returns>Lista de modelos de la marca</returns>
        [HttpGet, Route("PorMarca/{marcaId:long}")]
        public ModelResponse ObtenerModelosPorMarca(long marcaId)
        {
            var usuario = User.Identity.Name;
            var result = _modeloService.ObtenerModelosPorMarcaId(marcaId, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un modelo
        /// </summary>
        /// <param name="modelo">Objeto modelo con los datos</param>
        /// <returns>Modelo guardado con su ID actualizado</returns>
        [Permiso("Modelos")]
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarModelo(Modelo modelo)
        {
            var usuario = User.Identity.Name;
            var result = _modeloService.GuardarOActualizarModelo(modelo, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un modelo
        /// </summary>
        /// <param name="modelo">Modelo a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [Permiso("Modelos", "Eliminar")]
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarModelo(Modelo modelo)
        {
            var usuario = User.Identity.Name;
            modelo.FechaModificacion = DateTime.Now;
            var result = _modeloService.EliminarModelo(modelo.Id, modelo.ModificadoPor, modelo.FechaModificacion.Value, usuario);
            return result;
        }
    }
}