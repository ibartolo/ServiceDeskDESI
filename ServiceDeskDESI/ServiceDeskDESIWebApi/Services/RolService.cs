using Serilog;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class RolService
    {
        private readonly DbWrapper _dbWrapper;

        public RolService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Rol>> ObtenerRoles(string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerRoles(usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerRoles para usuario {Usuario}", usuario);
                return new ModelResponse<List<Rol>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.ObtenerRoles para usuario {Usuario}", usuario);
                return new ModelResponse<List<Rol>> { IsSuccess = false, Message = "Ocurrió un error al obtener los roles." };
            }
        }

        public ModelResponse<Rol> ObtenerRolPorId(long id, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerRolPorId(id, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerRolPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Rol> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.ObtenerRolPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Rol> { IsSuccess = false, Message = "Ocurrió un error al obtener el rol." };
            }
        }

        public ModelResponse<Rol> GuardarOActualizarRol(Rol rol, string usuarioAdmin)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rol.Nombre)) { throw new ArgumentException("El nombre del rol es requerido."); }
                if (rol.Nombre.Length > 50) { throw new ArgumentException("El nombre no puede exceder los 50 caracteres."); }
                if (rol.Descripcion != null && rol.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(rol.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAdmin)) { throw new ArgumentException("El usuario administrador es requerido."); }

                return _dbWrapper.GuardarOActualizarRol(rol, usuarioAdmin);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarRol para usuario {UsuarioAdmin}", usuarioAdmin);
                return new ModelResponse<Rol> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.GuardarOActualizarRol para usuario {UsuarioAdmin}", usuarioAdmin);
                return new ModelResponse<Rol> { IsSuccess = false, Message = "Ocurrió un error al guardar el rol." };
            }
        }

        public ModelResponse EliminarRol(long id, string usuarioAdmin, DateTime fechaModificacion)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAdmin)) { throw new ArgumentException("El usuario administrador es requerido."); }

                return _dbWrapper.EliminarRol(id, usuarioAdmin, fechaModificacion);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarRol para usuario {UsuarioAdmin}", usuarioAdmin);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.EliminarRol para usuario {UsuarioAdmin}", usuarioAdmin);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al eliminar el rol." };
            }
        }

        public ModelResponse AsignarRolUsuario(long usuarioId, long rolId, string asignadoPor, long empresaId)
        {
            try
            {
                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (rolId <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(asignadoPor)) { throw new ArgumentException("El usuario que asigna es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                return _dbWrapper.AsignarRolUsuario(usuarioId, rolId, asignadoPor, empresaId);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en AsignarRolUsuario para usuario {UsuarioId}", usuarioId);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.AsignarRolUsuario para usuario {UsuarioId}", usuarioId);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al asignar el rol." };
            }
        }

        public ModelResponse<List<Rol>> ObtenerRolesPorUsuario(long usuarioId, string usuarioAutenticado)
        {
            try
            {
                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAutenticado)) { throw new ArgumentException("El usuario autenticado es requerido."); }

                return _dbWrapper.ObtenerRolesPorUsuario(usuarioAutenticado);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerRolesPorUsuario para usuario {UsuarioId}", usuarioId);
                return new ModelResponse<List<Rol>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.ObtenerRolesPorUsuario para usuario {UsuarioId}", usuarioId);
                return new ModelResponse<List<Rol>> { IsSuccess = false, Message = "Ocurrió un error al obtener los roles del usuario." };
            }
        }

        public ModelResponse EliminarRolUsuario(long usuarioRolId, string modificadoPor, long empresaId)
        {
            try
            {
                if (usuarioRolId <= 0) { throw new ArgumentException("El ID de la relación usuario-rol es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                return _dbWrapper.EliminarRolUsuario(usuarioRolId, modificadoPor, empresaId);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarRolUsuario para usuario {UsuarioRolId}", usuarioRolId);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.EliminarRolUsuario para usuario {UsuarioRolId}", usuarioRolId);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al eliminar el rol del usuario." };
            }
        }
    }
}
