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

        public ModelResponse ObtenerPermisosPorUsuario(string nombreUsuario)
        {
            var modelResponse = new ModelResponse();

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
                    new Func<IDataReader, dynamic>((reader) =>
                    {
                        return new
                        {
                            PaginaId = MapearPorpiedades<long>(reader["PaginaId"]),
                            PaginaNombre = MapearPorpiedades<string>(reader["PaginaNombre"]),
                            Direccion = MapearPorpiedades<string>(reader["Direccion"]),
                            PuedeLeer = Convert.ToInt32(reader["PuedeLeer"]) == 1,
                            PuedeCrear = Convert.ToInt32(reader["PuedeCrear"]) == 1,
                            PuedeEditar = Convert.ToInt32(reader["PuedeEditar"]) == 1,
                            PuedeEliminar = Convert.ToInt32(reader["PuedeEliminar"]) == 1,
                            PuedeExportar = Convert.ToInt32(reader["PuedeExportar"]) == 1
                        };
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = permisos;
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

        public ModelResponse ValidarPermisoUsuario(long usuarioId, long paginaId, string accion)
        {
            var modelResponse = new ModelResponse();

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
    }
}