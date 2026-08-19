using Serilog;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper
    {
        public ModelResponse InsertarRolPaginaAccion(long rolId, long paginaId, bool puedeLeer, bool puedeCrear,
    bool puedeEditar, bool puedeEliminar, bool puedeExportar, string creadoPor, string usuarioAutenticado)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (rolId <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (paginaId <= 0) { throw new ArgumentException("El ID de la página es requerido."); }
                if (string.IsNullOrWhiteSpace(creadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAutenticado)) { throw new ArgumentException("El usuario autenticado es requerido."); }

                var resultado = ExecuteScalar("InsertarRolPaginaAccion", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@RolId", rolId),
            new SqlParameter("@PaginaId", paginaId),
            new SqlParameter("@PuedeLeer", puedeLeer),
            new SqlParameter("@PuedeCrear", puedeCrear),
            new SqlParameter("@PuedeEditar", puedeEditar),
            new SqlParameter("@PuedeEliminar", puedeEliminar),
            new SqlParameter("@PuedeExportar", puedeExportar),
            new SqlParameter("@CreadoPor", creadoPor),
            new SqlParameter("@UsuarioAutenticado", usuarioAutenticado)
                });

                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para asignar permisos a este rol o los datos no son válidos.";
                    return modelResponse;
                }

                if (resultadoLong == -1)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Ya existe una configuración de permisos para este rol y página.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = resultadoLong;
                modelResponse.Message = "Permisos asignados correctamente.";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al insertar permisos para rol {RolId} y página {PaginaId}", rolId, paginaId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al asignar los permisos.";
            }

            return modelResponse;
        }

        public ModelResponse EliminarRolPaginaAccion(long id, string modificadoPor, string usuarioAutenticado)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la configuración es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAutenticado)) { throw new ArgumentException("El usuario autenticado es requerido."); }

                var resultado = ExecuteScalar("EliminarRolPaginaAccion", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@UsuarioAutenticado", usuarioAutenticado)
                });

                if (Convert.ToInt64(resultado) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta configuración.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Configuración de permisos eliminada correctamente.";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar configuración de permisos {Id}", id);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la configuración.";
            }

            return modelResponse;
        }

        public ModelResponse ActualizarRolPaginaAccion(long id, bool puedeLeer, bool puedeCrear, bool puedeEditar,
            bool puedeEliminar, bool puedeExportar, string modificadoPor, string usuarioAutenticado)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la configuración es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAutenticado)) { throw new ArgumentException("El usuario autenticado es requerido."); }

                var resultado = ExecuteScalar("ActualizarRolPaginaAccion", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Id", id),
            new SqlParameter("@PuedeLeer", puedeLeer),
            new SqlParameter("@PuedeCrear", puedeCrear),
            new SqlParameter("@PuedeEditar", puedeEditar),
            new SqlParameter("@PuedeEliminar", puedeEliminar),
            new SqlParameter("@PuedeExportar", puedeExportar),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@UsuarioAutenticado", usuarioAutenticado)
                });

                if (Convert.ToInt64(resultado) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para actualizar esta configuración.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Permisos actualizados correctamente.";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al actualizar permisos {Id}", id);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al actualizar los permisos.";
            }

            return modelResponse;
        }

        public ModelResponse<List<PermisosViewModel>> ObtenerPermisosPorUsuario(string nombreUsuario)
        {
            var modelResponse = new ModelResponse<List<PermisosViewModel>>();

            try
            {
                if (string.IsNullOrWhiteSpace(nombreUsuario))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El nombre de usuario es requerido.";
                    return modelResponse;
                }

                var permisos = GetObjects("ObtenerPermisosPorUsuario", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@NombreUsuario", nombreUsuario) },
                    new Func<IDataReader, PermisosViewModel>(r => LlenarEntidad<PermisosViewModel>(r)));

                modelResponse.IsSuccess = true;
                modelResponse.Response = permisos.ToList();
                modelResponse.Message = "Permisos obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener permisos para usuario {NombreUsuario}", nombreUsuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los permisos.";
            }

            return modelResponse;
        }

        public ModelResponse<bool> ValidarPermisoUsuario(long usuarioId, long paginaId, string accion)
        {
            var modelResponse = new ModelResponse<bool>();

            try
            {
                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (paginaId <= 0) { throw new ArgumentException("El ID de la página es requerido."); }
                if (string.IsNullOrWhiteSpace(accion)) { throw new ArgumentException("La acción es requerida."); }

                var resultado = ExecuteScalar("ValidarPermisoUsuario", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@UsuarioId", usuarioId),
            new SqlParameter("@PaginaId", paginaId),
            new SqlParameter("@Accion", accion)
                });

                var tienePermiso = Convert.ToInt32(resultado) == 1;

                modelResponse.IsSuccess = true;
                modelResponse.Response = tienePermiso;
                modelResponse.Message = tienePermiso ? "Permiso concedido." : "Permiso denegado.";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al validar permiso para usuario {UsuarioId}, página {PaginaId}, acción {Accion}",
                    usuarioId, paginaId, accion);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al validar el permiso.";
            }

            return modelResponse;
        }



        // =========================================
        // DbWrapper.Permisos.cs - ObtenerPermisosPorRol y GuardarPermisosRol
        // =========================================

        public ModelResponse<List<RolPaginaAccionDTO>> ObtenerPermisosPorRol(long rolId, string usuario)
        {
            var modelResponse = new ModelResponse<List<RolPaginaAccionDTO>>();

            try
            {
                var permisos = GetObjects("ObtenerPermisosPorRol", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@RolId", rolId),
                new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, RolPaginaAccionDTO>(r => LlenarEntidad<RolPaginaAccionDTO>(r)));

                modelResponse.IsSuccess = true;
                modelResponse.Response = permisos.ToList();
                modelResponse.Message = "Permisos obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener permisos para rol {RolId} y usuario {Usuario}", rolId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los permisos.";
            }

            return modelResponse;
        }

        public ModelResponse GuardarPermisosRol(long rolId, long paginaId, bool puedeLeer, bool puedeCrear,
            bool puedeEditar, bool puedeEliminar, bool puedeExportar, string modificadoPor, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("GuardarPermisosRol", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@RolId", rolId),
            new SqlParameter("@PaginaId", paginaId),
            new SqlParameter("@PuedeLeer", puedeLeer),
            new SqlParameter("@PuedeCrear", puedeCrear),
            new SqlParameter("@PuedeEditar", puedeEditar),
            new SqlParameter("@PuedeEliminar", puedeEliminar),
            new SqlParameter("@PuedeExportar", puedeExportar),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@Usuario", usuario)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Response = Convert.ToInt64(resultado);
                modelResponse.Message = "Permisos guardados correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar permisos para rol {RolId}, página {PaginaId}, usuario {Usuario}",
                    rolId, paginaId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar los permisos.";
            }

            return modelResponse;
        }


        public ModelResponse GuardarPermisosRolMasivo(long rolId, List<PermisoRequest> permisos, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Transacción sobre una única conexión (evita escalación a MSDTC).
                BeginTransaction();

                // 1. Eliminar permisos existentes
                var resultadoEliminar = ExecuteScalar("EliminarPermisosRol", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@RolId", rolId),
            new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt32(resultadoEliminar) < 0)
                {
                    RollbackTransaction();
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para modificar este rol.";
                    return modelResponse;
                }

                // 2. Insertar nuevos permisos
                if (permisos != null && permisos.Any())
                {
                    foreach (var permiso in permisos)
                    {
                        var resultado = ExecuteScalar("GuardarPermisosRol", CommandType.StoredProcedure, new SqlParameter[]
                        {
                    new SqlParameter("@RolId", rolId),
                    new SqlParameter("@PaginaId", permiso.PaginaId),
                    new SqlParameter("@PuedeLeer", permiso.PuedeLeer),
                    new SqlParameter("@PuedeCrear", permiso.PuedeCrear),
                    new SqlParameter("@PuedeEditar", permiso.PuedeEditar),
                    new SqlParameter("@PuedeEliminar", permiso.PuedeEliminar),
                    new SqlParameter("@PuedeExportar", permiso.PuedeExportar),
                    new SqlParameter("@ModificadoPor", usuario),
                    new SqlParameter("@Usuario", usuario)
                        });

                        if (Convert.ToInt64(resultado) == 0)
                        {
                            throw new Exception($"Error al guardar permiso para página {permiso.PaginaId}");
                        }
                    }
                }

                // 3. Confirmar transacción
                CommitTransaction();

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Permisos guardados correctamente.";
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Log.Error(ex, "Error al guardar permisos masivos para rol {RolId} y usuario {Usuario}", rolId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar los permisos.";
            }

            return modelResponse;
        }
    }
}