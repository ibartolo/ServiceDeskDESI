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

        public ModelResponse<List<PermisosViewModel>> ObtenerPermisosPorUsuario(string usuario)
        {
            try
            {
                Log.Information("PermisosService.ObtenerPermisosPorUsuario para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerPermisosPorUsuario(usuario);
                Log.Information("PermisosService.ObtenerPermisosPorUsuario RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerPermisosPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse<List<PermisosViewModel>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPermisosPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse<List<PermisosViewModel>> { IsSuccess = false, Message = "Ocurrió un error al obtener los permisos." };
            }
        }

        public ModelResponse<bool> ValidarPermisoUsuario(string usuario, string nombrePagina, string accion)
        {
            try
            {
                Log.Information("PermisosService.ValidarPermisoUsuario para usuario {Usuario}, página {NombrePagina}, acción {Accion}", usuario, nombrePagina, accion);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(nombrePagina)) { throw new ArgumentException("El nombre de la página es requerido."); }
                if (string.IsNullOrWhiteSpace(accion)) { throw new ArgumentException("La acción es requerida."); }

                var usuarioResponse = _dbWrapper.ObtenerUsuarioPorNombreUsuario(usuario, usuario);
                if (!usuarioResponse.IsSuccess || usuarioResponse.Response == null)
                {
                    throw new ArgumentException("Usuario no encontrado.");
                }

                var usuarioObj = (Usuario)usuarioResponse.Response;

                var paginaResponse = _dbWrapper.ObtenerPaginaPorNombre(nombrePagina);
                if (!paginaResponse.IsSuccess || paginaResponse.Response == null)
                {
                    throw new ArgumentException("Página no encontrada.");
                }

                var pagina = (Pagina)paginaResponse.Response;

                var result = _dbWrapper.ValidarPermisoUsuario(usuarioObj.Id, pagina.Id, accion);
                Log.Information("PermisosService.ValidarPermisoUsuario RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ValidarPermisoUsuario para usuario {Usuario}, página {NombrePagina}, acción {Accion}",
                    usuario, nombrePagina, accion);
                return new ModelResponse<bool> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ValidarPermisoUsuario para usuario {Usuario}, página {NombrePagina}, acción {Accion}",
                    usuario, nombrePagina, accion);
                return new ModelResponse<bool> { IsSuccess = false, Message = "Ocurrió un error al validar el permiso." };
            }
        }

        public ModelResponse<List<Pagina>> ObtenerPaginas()
        {
            try
            {
                Log.Information("PermisosService.ObtenerPaginas");

                var result = _dbWrapper.ObtenerPaginas();
                Log.Information("PermisosService.ObtenerPaginas RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPaginas");
                return new ModelResponse<List<Pagina>> { IsSuccess = false, Message = "Ocurrió un error al obtener las páginas." };
            }
        }

        public ModelResponse<List<RolPaginaAccionDTO>> ObtenerPermisosPorRol(long rolId, string usuario)
        {
            try
            {
                Log.Information("PermisosService.ObtenerPermisosPorRol para RolId {RolId} usuario {Usuario}", rolId, usuario);

                if (rolId <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerPermisosPorRol(rolId, usuario);
                Log.Information("PermisosService.ObtenerPermisosPorRol RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerPermisosPorRol para rol {RolId} y usuario {Usuario}", rolId, usuario);
                return new ModelResponse<List<RolPaginaAccionDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPermisosPorRol para rol {RolId} y usuario {Usuario}", rolId, usuario);
                return new ModelResponse<List<RolPaginaAccionDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener los permisos del rol." };
            }
        }

        public ModelResponse GuardarPermisosRol(long rolId, long paginaId, bool puedeLeer, bool puedeCrear,
            bool puedeEditar, bool puedeEliminar, bool puedeExportar, string modificadoPor, string usuario)
        {
            try
            {
                Log.Information("PermisosService.GuardarPermisosRol para RolId {RolId}, PaginaId {PaginaId}, usuario {Usuario}", rolId, paginaId, usuario);

                if (rolId <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (paginaId <= 0) { throw new ArgumentException("El ID de la página es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                // Validar que el usuario sea administrador
                var usuarioResponse = _dbWrapper.ObtenerUsuarioPorNombreUsuario(usuario, usuario);
                if (!usuarioResponse.IsSuccess || usuarioResponse.Response == null)
                {
                    throw new ArgumentException("Usuario no encontrado.");
                }

                var usuarioObj = (Usuario)usuarioResponse.Response;

                var rolesResponse = _dbWrapper.ObtenerRolesPorUsuario(usuario);
                if (!rolesResponse.IsSuccess || rolesResponse.Response == null)
                {
                    throw new ArgumentException("No tiene permisos de administrador.");
                }

                var roles = (IEnumerable<dynamic>)rolesResponse.Response;
                var esAdmin = roles.Any(r => r.Nombre == "Administrador");

                if (!esAdmin)
                {
                    throw new ArgumentException("No tiene permisos de administrador para modificar permisos.");
                }

                var result = _dbWrapper.GuardarPermisosRol(rolId, paginaId, puedeLeer, puedeCrear, puedeEditar,
                    puedeEliminar, puedeExportar, modificadoPor, usuario);
                Log.Information("PermisosService.GuardarPermisosRol RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarPermisosRol para rol {RolId}, página {PaginaId}, usuario {Usuario}",
                    rolId, paginaId, usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarPermisosRol para rol {RolId}, página {PaginaId}, usuario {Usuario}",
                    rolId, paginaId, usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al guardar los permisos." };
            }
        }

        public ModelResponse GuardarPermisosRolMasivo(long rolId, List<PermisoRequest> permisos, string usuario)
        {
            try
            {
                Log.Information("PermisosService.GuardarPermisosRolMasivo para RolId {RolId} usuario {Usuario}", rolId, usuario);

                // Validaciones
                if (rolId <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (permisos == null) { throw new ArgumentException("La lista de permisos es requerida."); }


                // Validar cada permiso individualmente
                foreach (var permiso in permisos)
                {
                    if (permiso.PaginaId <= 0) { throw new ArgumentException("El ID de la página es requerido."); }

                    // Validar que si tiene permisos de creación/edición/eliminación/exportación también tenga lectura
                    if ((permiso.PuedeCrear || permiso.PuedeEditar || permiso.PuedeEliminar || permiso.PuedeExportar) && !permiso.PuedeLeer)
                    {
                        throw new ArgumentException($"No se puede asignar permisos de creación, edición, eliminación o exportación sin permisos de lectura para la página {permiso.PaginaId}.");
                    }
                }

                var result = _dbWrapper.GuardarPermisosRolMasivo(rolId, permisos, usuario);
                Log.Information("PermisosService.GuardarPermisosRolMasivo RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarPermisosRolMasivo para rol {RolId} y usuario {Usuario}", rolId, usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarPermisosRolMasivo para rol {RolId} y usuario {Usuario}", rolId, usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al guardar los permisos." };
            }
        }
    }
}