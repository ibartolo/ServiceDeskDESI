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
            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerPermisosPorUsuario(usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerPermisosPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPermisosPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener los permisos." };
            }
        }

        public ModelResponse ValidarPermisoUsuario(string usuario, string nombrePagina, string accion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(nombrePagina)) { throw new ArgumentException("El nombre de la página es requerido."); }
                if (string.IsNullOrWhiteSpace(accion)) { throw new ArgumentException("La acción es requerida."); }

                var usuarioResponse = _dbWrapper.ObtenerUsuarioPorNombreUsuario(usuario);
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

                return _dbWrapper.ValidarPermisoUsuario(usuarioObj.Id, pagina.Id, accion);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ValidarPermisoUsuario para usuario {Usuario}, página {NombrePagina}, acción {Accion}",
                    usuario, nombrePagina, accion);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ValidarPermisoUsuario para usuario {Usuario}, página {NombrePagina}, acción {Accion}",
                    usuario, nombrePagina, accion);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al validar el permiso." };
            }
        }

        public ModelResponse ObtenerPaginas()
        {
            try
            {
                return _dbWrapper.ObtenerPaginas();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPaginas");
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener las páginas." };
            }
        }

        public ModelResponse ObtenerPermisosPorRol(long rolId, string usuario)
        {
            try
            {
                if (rolId <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerPermisosPorRol(rolId, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerPermisosPorRol para rol {RolId} y usuario {Usuario}", rolId, usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPermisosPorRol para rol {RolId} y usuario {Usuario}", rolId, usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener los permisos del rol." };
            }
        }

        public ModelResponse GuardarPermisosRol(long rolId, long paginaId, bool puedeLeer, bool puedeCrear,
            bool puedeEditar, bool puedeEliminar, bool puedeExportar, string modificadoPor, string usuario)
        {
            try
            {
                if (rolId <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (paginaId <= 0) { throw new ArgumentException("El ID de la página es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                // Validar que el usuario sea administrador
                var usuarioResponse = _dbWrapper.ObtenerUsuarioPorNombreUsuario(usuario);
                if (!usuarioResponse.IsSuccess || usuarioResponse.Response == null)
                {
                    throw new ArgumentException("Usuario no encontrado.");
                }

                var usuarioObj = (Usuario)usuarioResponse.Response;

                var rolesResponse = _dbWrapper.ObtenerRolesPorUsuario(usuarioObj.Id, usuario);
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

                return _dbWrapper.GuardarPermisosRol(rolId, paginaId, puedeLeer, puedeCrear, puedeEditar,
                    puedeEliminar, puedeExportar, modificadoPor, usuario);
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

                return _dbWrapper.GuardarPermisosRolMasivo(rolId, permisos, usuario);
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