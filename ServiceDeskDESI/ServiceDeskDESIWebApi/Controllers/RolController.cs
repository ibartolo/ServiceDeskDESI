using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    //[Authorize]
    [RoutePrefix("api/Rol")]
    public class RolController : BaseController
    {
        /// <summary>
        /// Obtiene todos los roles activos
        /// </summary>
        /// <returns>Lista de roles</returns>
        [HttpGet, Route("Lista")]
        public ModelResponse ObtenerRoles()
        {
            var result = dbWrapper.ObtenerRoles();
            return result;
        }

        /// <summary>
        /// Obtiene un rol por su ID
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <returns>Rol encontrado</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerRolPorId(long id)
        {
            var result = dbWrapper.ObtenerRolPorId(id);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un rol
        /// </summary>
        /// <param name="rol">Objeto rol con los datos</param>
        /// <returns>Rol guardado con su ID actualizado</returns>
        [HttpPost, Route("")]
        public ModelResponse GuardarOActualizarRol(Rol rol)
        {
            var result = dbWrapper.GuardarOActualizarRol(rol);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un rol
        /// </summary>
        /// <param name="rol">Rol a eliminar (debe incluir Id, ModificadoPor y FechaModificacion)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("")]
        public ModelResponse EliminarRol(Rol rol)
        {
            rol.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarRol(rol.Id, rol.ModificadoPor, rol.FechaModificacion.Value);
            return result;
        }
    }
}
