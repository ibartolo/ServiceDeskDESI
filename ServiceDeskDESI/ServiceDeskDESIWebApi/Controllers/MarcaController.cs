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
    [RoutePrefix("api/Marca")]
    public class MarcaController : BaseController
    {
        /// <summary>
        /// Obtiene todas las marcas de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de marcas</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerTodosLasMarcas()
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.ObtenerTodosLasMarcas(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene una marca por su ID
        /// </summary>
        /// <param name="id">ID de la marca</param>
        /// <returns>Marca encontrada</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerMarcaPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.ObtenerMarcaPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza una marca
        /// </summary>
        /// <param name="marca">Objeto marca con los datos</param>
        /// <returns>Marca guardada con su ID actualizado</returns>
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarMarca(Marca marca)
        {
            var usuario = User.Identity.Name;
            var result = dbWrapper.GuardarOActualizarMarca(marca, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente una marca
        /// </summary>
        /// <param name="marca">Marca a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarMarca(Marca marca)
        {
            var usuario = User.Identity.Name;
            marca.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarMarca(marca.Id, marca.ModificadoPor, marca.FechaModificacion.Value, usuario);
            return result;
        }
    }
}