using Serilog;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceDeskDESIWebApi.Services
{
    public class PermisosService
    {
        private readonly DbWrapper _dbWrapper;

        public PermisosService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse ObtenerPermisosPorUsuario(string usuario)
        {
            var response = new ModelResponse();

            try
            {
                // Validar que el usuario no esté vacío
                if (string.IsNullOrWhiteSpace(usuario))
                {
                    response.IsSuccess = false;
                    response.Message = "El nombre de usuario es requerido.";
                    return response;
                }

                // Obtener permisos directamente por nombre de usuario
                var permisosResponse = _dbWrapper.ObtenerPermisosPorUsuario(usuario);
                return permisosResponse;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PermisosService.ObtenerPermisosPorUsuario para usuario {Usuario}", usuario);
                response.IsSuccess = false;
                response.Message = "Ocurrió un error al obtener los permisos.";
                return response;
            }
        }

        public ModelResponse ValidarPermisoUsuario(string usuario, string nombrePagina, string accion)
        {
            var response = new ModelResponse();

            try
            {
                // Validar que el usuario exista
                var usuarioResponse = _dbWrapper.ObtenerUsuarioPorNombreUsuario(usuario);
                if (!usuarioResponse.IsSuccess || usuarioResponse.Response == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Usuario no encontrado.";
                    return response;
                }

                var usuarioObj = (Usuario)usuarioResponse.Response;

                // Obtener página por nombre
                var paginaResponse = _dbWrapper.ObtenerPaginaPorNombre(nombrePagina);
                if (!paginaResponse.IsSuccess || paginaResponse.Response == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Página no encontrada.";
                    return response;
                }

                var pagina = (Pagina)paginaResponse.Response;

                // Validar permiso
                var permisoResult = _dbWrapper.ValidarPermisoUsuario(usuarioObj.Id, pagina.Id, accion);
                return permisoResult;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PermisosService.ValidarPermisoUsuario para usuario {Usuario}, página {NombrePagina}, acción {Accion}",
                    usuario, nombrePagina, accion);
                response.IsSuccess = false;
                response.Message = "Error al validar permiso.";
                return response;
            }
        }
    }
}