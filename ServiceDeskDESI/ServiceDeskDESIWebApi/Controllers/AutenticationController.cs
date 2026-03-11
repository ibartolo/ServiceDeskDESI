using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [RoutePrefix("api/Autentication")]
    public class AutenticationController : BaseController
    {
        /// <summary>
        /// Controlador para gestión de usuarios
        /// </summary>
        [RoutePrefix("api/usuario")]
        public class UsuarioController : ApiController
        {
            private readonly DbWrapper dbWrapper;

            public UsuarioController()
            {
                dbWrapper = new DbWrapper();
            }

            /// <summary>
            /// Obtiene todos los usuarios activos
            /// </summary>
            /// <returns>Lista de usuarios con sus sucursales y áreas</returns>
            [HttpGet, Route("Lista")]
            public ModelResponse ObtenerUsuarios()
            {
                var result = dbWrapper.ObtenerUsuarios();
                return result;
            }

            /// <summary>
            /// Obtiene un usuario por su ID
            /// </summary>
            /// <param name="id">ID del usuario</param>
            /// <returns>Usuario encontrado</returns>
            [HttpGet, Route("{id:long}")]
            public ModelResponse ObtenerUsuarioPorId(long id)
            {
                var result = dbWrapper.ObtenerUsuarioPorId(id);
                return result;
            }

            /// <summary>
            /// Guarda o actualiza un usuario
            /// </summary>
            /// <param name="u">Objeto usuario con los datos</param>
            /// <returns>Usuario guardado con su ID actualizado</returns>
            [HttpPost, Route("")]
            public ModelResponse GuardarOActualizarUsuario(Usuario u)
            {
                var result = dbWrapper.GuardarOActualizarUsuario(u);
                return result;
            }

            /// <summary>
            /// Elimina lógicamente un usuario
            /// </summary>
            /// <param name="u">Usuario a eliminar (debe incluir Id y ModificadoPor)</param>
            /// <returns>Resultado de la operación</returns>
            [HttpDelete, Route("")]
            public ModelResponse EliminarUsuario(Usuario u)
            {
                u.FechaModificacion = DateTime.Now;
                var result = dbWrapper.EliminarUsuario(u.Id, u.ModificadoPor, u.FechaModificacion.Value);
                return result;
            }

            /// <summary>
            /// Autentica un usuario en el sistema
            /// </summary>
            /// <param name="u">Objeto usuario con NombreUsuario y Contrasena</param>
            /// <returns>Usuario autenticado con sus datos completos</returns>
            [HttpPost, Route("autenticar")]
            public ModelResponse AutenticarUsuario(Usuario u)
            {
                var result = dbWrapper.AutenticarUsuario(u.NombreUsuario, u.Contrasena);
                return result;
            }
        }
    }
}
