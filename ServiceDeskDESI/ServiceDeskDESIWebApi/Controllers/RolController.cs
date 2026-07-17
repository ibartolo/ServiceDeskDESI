using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
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
    [RoutePrefix("api/Rol")]
    public class RolController : BaseController
    {
        private readonly RolService _rolService;

        public RolController()
        {
            _rolService = new RolService();
        }

        /// <summary>
        /// Obtiene todos los roles de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de roles</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerRoles()
        {
            var usuario = User.Identity.Name;
            var result = _rolService.ObtenerRoles(usuario);
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
            var usuario = User.Identity.Name;
            var result = _rolService.ObtenerRolPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un rol (solo administradores)
        /// </summary>
        /// <param name="rol">Objeto rol con los datos</param>
        /// <returns>Rol guardado con su ID actualizado</returns>
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarRol(Rol rol)
        {
            var usuarioAdmin = User.Identity.Name;
            var result = _rolService.GuardarOActualizarRol(rol, usuarioAdmin);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un rol (solo administradores)
        /// </summary>
        /// <param name="rol">Rol a eliminar (debe incluir Id)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarRol(Rol rol)
        {
            var usuarioAdmin = User.Identity.Name;
            rol.FechaModificacion = DateTime.Now;
            var result = _rolService.EliminarRol(rol.Id, usuarioAdmin, rol.FechaModificacion.Value);
            return result;
        }

        /// <summary>
        /// Asigna un rol a un usuario (solo administradores)
        /// </summary>
        /// <param name="request">Objeto con UsuarioId y RolId</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost, Route("Asignar")]
        public ModelResponse AsignarRolUsuario([FromBody] AsignarRolRequest request)
        {
            var usuarioAdmin = User.Identity.Name;
            var empresaId = ObtenerEmpresaId();
            var result = dbWrapper.AsignarRolUsuario(request.UsuarioId, request.RolId, usuarioAdmin, empresaId);
            return result;
        }

        /// <summary>
        /// Obtiene los roles de un usuario específico
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Lista de roles del usuario</returns>
        [HttpGet, Route("Usuario/{usuarioId:long}")]
        public ModelResponse ObtenerRolesPorUsuario(long usuarioId)
        {
            var usuarioAutenticado = User.Identity.Name;
            var result = dbWrapper.ObtenerRolesPorUsuario(usuarioId, usuarioAutenticado);
            return result;
        }

        /// <summary>
        /// Elimina un rol de un usuario (solo administradores)
        /// </summary>
        /// <param name="request">Objeto con UsuarioRolId</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("EliminarUsuarioRol")]
        public ModelResponse EliminarRolUsuario([FromBody] EliminarRolUsuarioRequest request)
        {
            var usuarioAdmin = User.Identity.Name;
            var empresaId = ObtenerEmpresaId();
            var result = dbWrapper.EliminarRolUsuario(request.UsuarioRolId, usuarioAdmin, empresaId);
            return result;
        }

        #region Métodos auxiliares
        private long ObtenerEmpresaId()
        {
            // Obtener el EmpresaId del usuario autenticado desde el token o base de datos
            var usuario = User.Identity.Name;
            var userResponse = dbWrapper.ObtenerUsuarioPorNombreUsuario(usuario);
            if (userResponse.IsSuccess && userResponse.Response != null)
            {
                var usuarioObj = (Usuario)userResponse.Response;
                return usuarioObj.Empresa.Id;
            }
            return 0;
        }
        #endregion
    }

    #region Request classes
    public class AsignarRolRequest
    {
        public long UsuarioId { get; set; }
        public long RolId { get; set; }
    }

    public class EliminarRolUsuarioRequest
    {
        public long UsuarioRolId { get; set; }
    }
    #endregion
}