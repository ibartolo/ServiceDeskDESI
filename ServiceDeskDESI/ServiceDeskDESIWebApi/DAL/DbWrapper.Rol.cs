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
        // =========================================
        // D B W R A P P E R   R O L
        // =========================================

        public ModelResponse ObtenerRoles(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var roles = GetObjects("ObtenerRoles", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Rol>((reader) =>
                    {
                        var rol = LlenarEntidad<Rol>(reader);
                        return rol;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = roles;
                modelResponse.Message = "Roles obtenidos correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener roles para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los roles";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerRolPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var rol = GetObject("ObtenerRolPorId", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Rol>((reader) =>
                    {
                        var r = LlenarEntidad<Rol>(reader);
                        return r;
                    }));

                if (rol == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el rol especificado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = rol;
                modelResponse.Message = "Rol obtenido correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener rol {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el rol";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarRol(Rol r, string usuarioAdmin, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(r.Nombre)) { throw new ArgumentException("El nombre del rol es requerido."); }
                if (r.Nombre.Length > 50) { throw new ArgumentException("El nombre no puede exceder los 50 caracteres."); }
                if (r.Descripcion != null && r.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(r.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAdmin)) { throw new ArgumentException("El usuario administrador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var parametrosObj = new
                {
                    r.Id,
                    r.Nombre,
                    r.Descripcion,
                    r.CreadoPor,
                    r.FechaCreacion,
                    r.ModificadoPor,
                    r.FechaModificacion,
                    r.Estatus,
                    UsuarioAdmin = usuarioAdmin,
                    EmpresaId = empresaId
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var resultado = ExecuteScalar("GuardarOActualizarRol", CommandType.StoredProcedure, parametros);
                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos de administrador para realizar esta operación.";
                    return modelResponse;
                }

                if (resultadoLong == -1)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Ya existe un rol con ese nombre en la empresa.";
                    return modelResponse;
                }

                r.Id = resultadoLong;

                modelResponse.IsSuccess = true;
                modelResponse.Response = r;
                modelResponse.Message = "Rol guardado correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar rol para usuario {UsuarioAdmin}", usuarioAdmin);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el rol";
            }

            return modelResponse;
        }

        public ModelResponse EliminarRol(long id, string usuarioAdmin, DateTime fechaModificacion, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAdmin)) { throw new ArgumentException("El usuario administrador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var resultado = ExecuteScalar("EliminarRol", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", usuarioAdmin),
            new SqlParameter("@FechaModificacion", fechaModificacion),
            new SqlParameter("@UsuarioAdmin", usuarioAdmin),
            new SqlParameter("@EmpresaId", empresaId)
                });

                if (Convert.ToInt64(resultado) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar este rol o el rol es 'Administrador'.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Rol eliminado correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar rol {Id} para usuario {UsuarioAdmin}", id, usuarioAdmin);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el rol";
            }

            return modelResponse;
        }

        // =========================================
        // D B W R A P P E R   U S U A R I O R O L
        // =========================================

        public ModelResponse AsignarRolUsuario(long usuarioId, long rolId, string asignadoPor, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (rolId <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(asignadoPor)) { throw new ArgumentException("El usuario que asigna es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var resultado = ExecuteScalar("AsignarRolUsuario", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@UsuarioId", usuarioId),
            new SqlParameter("@RolId", rolId),
            new SqlParameter("@AsignadoPor", asignadoPor),
            new SqlParameter("@EmpresaId", empresaId)
                });

                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para asignar roles o los datos no son válidos.";
                    return modelResponse;
                }

                if (resultadoLong == -1)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El usuario ya tiene asignado este rol.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = resultadoLong;
                modelResponse.Message = "Rol asignado correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al asignar rol al usuario {UsuarioId}", usuarioId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al asignar el rol";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerRolesPorUsuario(long usuarioId, string usuarioAutenticado)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAutenticado)) { throw new ArgumentException("El usuario autenticado es requerido."); }

                var resultado = GetObject("ObtenerRolesPorUsuario", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@UsuarioId", usuarioId),
                new SqlParameter("@UsuarioAutenticado", usuarioAutenticado)
                    },
                    new Func<IDataReader, dynamic>((reader) =>
                    {
                        return new
                        {
                            TieneAcceso = MapearPorpiedades<int>(reader["TieneAcceso"])
                        };
                    }));

                if (resultado != null && resultado.TieneAcceso == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene acceso a los roles de este usuario.";
                    return modelResponse;
                }

                var roles = GetObjects("ObtenerRolesPorUsuario", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@UsuarioId", usuarioId),
                new SqlParameter("@UsuarioAutenticado", usuarioAutenticado)
                    },
                    new Func<IDataReader, Rol>((reader) =>
                    {
                        var rol = LlenarEntidad<Rol>(reader);
                        return rol;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = roles;
                modelResponse.Message = "Roles obtenidos correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener roles del usuario {UsuarioId}", usuarioId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los roles del usuario";
            }

            return modelResponse;
        }

        public ModelResponse EliminarRolUsuario(long usuarioRolId, string modificadoPor, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (usuarioRolId <= 0) { throw new ArgumentException("El ID de la relación usuario-rol es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var resultado = ExecuteScalar("EliminarRolUsuario", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@UsuarioRolId", usuarioRolId),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@EmpresaId", empresaId)
                });

                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta asignación de rol.";
                    return modelResponse;
                }

                if (resultadoLong == -1)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se puede eliminar el rol 'Administrador' del usuario administrador.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Rol eliminado del usuario correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar rol del usuario {UsuarioRolId}", usuarioRolId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el rol del usuario";
            }

            return modelResponse;
        }
    }
}