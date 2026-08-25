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
                Log.Information("RolService.ObtenerRoles para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerRoles(usuario);
                Log.Information("RolService.ObtenerRoles RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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
                Log.Information("RolService.ObtenerRolPorId para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerRolPorId(id, usuario);
                Log.Information("RolService.ObtenerRolPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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
                Log.Information("RolService.GuardarOActualizarRol para usuario {UsuarioAdmin}", usuarioAdmin);

                if (string.IsNullOrWhiteSpace(rol.Nombre)) { throw new ArgumentException("El nombre del rol es requerido."); }
                if (rol.Nombre.Length > 50) { throw new ArgumentException("El nombre no puede exceder los 50 caracteres."); }
                if (rol.Descripcion != null && rol.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(rol.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAdmin)) { throw new ArgumentException("El usuario administrador es requerido."); }

                var result = _dbWrapper.GuardarOActualizarRol(rol, usuarioAdmin);
                Log.Information("RolService.GuardarOActualizarRol RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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
                Log.Information("RolService.EliminarRol para Id {Id} usuario {UsuarioAdmin}", id, usuarioAdmin);

                if (id <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAdmin)) { throw new ArgumentException("El usuario administrador es requerido."); }

                var result = _dbWrapper.EliminarRol(id, usuarioAdmin, fechaModificacion);
                Log.Information("RolService.EliminarRol RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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
                Log.Information("RolService.AsignarRolUsuario para UsuarioId {UsuarioId}, RolId {RolId}, usuario {AsignadoPor}, EmpresaId {EmpresaId}", usuarioId, rolId, asignadoPor, empresaId);

                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (rolId <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(asignadoPor)) { throw new ArgumentException("El usuario que asigna es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var result = _dbWrapper.AsignarRolUsuario(usuarioId, rolId, asignadoPor, empresaId);
                Log.Information("RolService.AsignarRolUsuario RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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
                Log.Information("RolService.ObtenerRolesPorUsuario para UsuarioId {UsuarioId} usuario {UsuarioAutenticado}", usuarioId, usuarioAutenticado);

                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAutenticado)) { throw new ArgumentException("El usuario autenticado es requerido."); }

                var result = _dbWrapper.ObtenerRolesPorUsuarioId(usuarioId);
                Log.Information("RolService.ObtenerRolesPorUsuario RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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

        public ModelResponse<List<UsuarioRol>> ObtenerUsuarioRolesPorUsuario(long usuarioId, string usuarioAutenticado)
        {
            try
            {
                Log.Information("RolService.ObtenerUsuarioRolesPorUsuario para UsuarioId {UsuarioId} usuario {UsuarioAutenticado}", usuarioId, usuarioAutenticado);

                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAutenticado)) { throw new ArgumentException("El usuario autenticado es requerido."); }

                var result = _dbWrapper.ObtenerUsuarioRolesPorUsuario(usuarioId);
                Log.Information("RolService.ObtenerUsuarioRolesPorUsuario RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerUsuarioRolesPorUsuario para usuario {UsuarioId}", usuarioId);
                return new ModelResponse<List<UsuarioRol>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.ObtenerUsuarioRolesPorUsuario para usuario {UsuarioId}", usuarioId);
                return new ModelResponse<List<UsuarioRol>> { IsSuccess = false, Message = "Ocurrió un error al obtener las asignaciones usuario-rol." };
            }
        }

        public ModelResponse EliminarRolUsuario(long usuarioRolId, string modificadoPor, long empresaId)
        {
            try
            {
                Log.Information("RolService.EliminarRolUsuario para UsuarioRolId {UsuarioRolId} usuario {ModificadoPor}, EmpresaId {EmpresaId}", usuarioRolId, modificadoPor, empresaId);

                if (usuarioRolId <= 0) { throw new ArgumentException("El ID de la relación usuario-rol es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var result = _dbWrapper.EliminarRolUsuario(usuarioRolId, modificadoPor, empresaId);
                Log.Information("RolService.EliminarRolUsuario RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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
