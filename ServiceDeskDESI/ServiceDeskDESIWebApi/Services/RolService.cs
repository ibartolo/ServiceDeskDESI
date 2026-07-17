using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;

namespace ServiceDeskDESIWebApi.Services
{
    public class RolService
    {
        private readonly DbWrapper _dbWrapper;

        public RolService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse ObtenerRoles(string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerRoles(usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.ObtenerRoles para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los roles."
                };
            }
        }

        public ModelResponse ObtenerRolPorId(long id, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerRolPorId(id, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.ObtenerRolPorId para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener el rol."
                };
            }
        }

        public ModelResponse GuardarOActualizarRol(Rol rol, string usuarioAdmin)
        {
            try
            {
                return _dbWrapper.GuardarOActualizarRol(rol, usuarioAdmin);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.GuardarOActualizarRol para usuario {UsuarioAdmin}", usuarioAdmin);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar el rol."
                };
            }
        }

        public ModelResponse EliminarRol(long id, string usuarioAdmin, DateTime fechaModificacion)
        {
            try
            {
                return _dbWrapper.EliminarRol(id, usuarioAdmin, fechaModificacion);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RolService.EliminarRol para usuario {UsuarioAdmin}", usuarioAdmin);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al eliminar el rol."
                };
            }
        }
    }
}