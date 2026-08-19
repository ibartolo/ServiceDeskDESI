using Serilog;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper
    {
        public ModelResponse<List<Rol>> ObtenerRoles(string usuario)
        {
            var modelResponse = new ModelResponse<List<Rol>>();

            try
            {
                var roles = GetObjects("ObtenerRoles", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Rol>((reader) =>
                    {
                        var rol = LlenarEntidad<Rol>(reader);
                        return rol;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = roles.ToList();
                modelResponse.Message = "Roles obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener roles para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los roles";
            }

            return modelResponse;
        }

        public ModelResponse<Rol> ObtenerRolPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse<Rol>();

            try
            {
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
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener rol {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el rol";
            }

            return modelResponse;
        }

        public ModelResponse<Rol> GuardarOActualizarRol(Rol rol, string usuarioAdmin)
        {
            var modelResponse = new ModelResponse<Rol>();

            try
            {
                var parametrosObj = new
                {
                    rol.Id,
                    rol.Nombre,
                    rol.Descripcion,
                    PuedeAtenderTickets = rol.PuedeAtenderTickets,
                    rol.CreadoPor,
                    rol.FechaCreacion,
                    rol.ModificadoPor,
                    rol.FechaModificacion,
                    rol.Estatus,
                    Usuario = usuarioAdmin
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var resultado = ExecuteScalar("GuardarOActualizarRol", CommandType.StoredProcedure, parametros);
                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                if (resultadoLong == -1)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Ya existe un rol con ese nombre en la empresa.";
                    return modelResponse;
                }

                rol.Id = resultadoLong;

                modelResponse.IsSuccess = true;
                modelResponse.Response = rol;
                modelResponse.Message = "Rol guardado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar rol para usuario {UsuarioAdmin}", usuarioAdmin);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el rol";
            }

            return modelResponse;
        }

        public ModelResponse EliminarRol(long id, string usuarioAdmin, DateTime fechaModificacion)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("EliminarRol", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", usuarioAdmin),
                    new SqlParameter("@FechaModificacion", fechaModificacion),
                    new SqlParameter("@Usuario", usuarioAdmin)
                });

                if (Convert.ToInt64(resultado) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar este rol.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Rol eliminado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar rol {Id} para usuario {UsuarioAdmin}", id, usuarioAdmin);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el rol";
            }

            return modelResponse;
        }

        public ModelResponse AsignarRolUsuario(long usuarioId, long rolId, string asignadoPor, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
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
            catch (Exception ex)
            {
                Log.Error(ex, "Error al asignar rol al usuario {UsuarioId}", usuarioId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al asignar el rol";
            }

            return modelResponse;
        }

        public ModelResponse EliminarRolUsuario(long usuarioRolId, string modificadoPor, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
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
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar rol del usuario {UsuarioRolId}", usuarioRolId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el rol del usuario";
            }

            return modelResponse;
        }

        public ModelResponse<List<Rol>> ObtenerRolesPorUsuario(string usuario)
        {
            var modelResponse = new ModelResponse<List<Rol>>();

            try
            {
                var roles = GetObjects("ObtenerRolesPorUsuario", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Rol>((reader) =>
                    {
                        var rol = LlenarEntidad<Rol>(reader);
                        return rol;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = roles.ToList();
                modelResponse.Message = "Roles obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener roles del usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los roles del usuario";
            }

            return modelResponse;
        }

        /// <summary>
        /// Obtiene los roles asignados a un usuario por su ID (no por username).
        /// Usa el SP ObtenerRolesPorUsuarioId para respetar el usuario objetivo.
        /// </summary>
        public ModelResponse<List<Rol>> ObtenerRolesPorUsuarioId(long usuarioId)
        {
            var modelResponse = new ModelResponse<List<Rol>>();

            try
            {
                var roles = GetObjects("ObtenerRolesPorUsuarioId", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@UsuarioId", usuarioId) },
                    new Func<IDataReader, Rol>((reader) =>
                    {
                        var rol = LlenarEntidad<Rol>(reader);
                        return rol;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = roles.ToList();
                modelResponse.Message = "Roles obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener roles del usuario {UsuarioId}", usuarioId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los roles del usuario";
            }

            return modelResponse;
        }

        /// <summary>
        /// Obtiene las filas UsuarioRol (junction) de un usuario, incluyendo el Id de cada fila.
        /// Necesario para eliminar correctamente la asignación usuario-rol (EliminarRolUsuario espera UsuarioRol.Id).
        /// </summary>
        public ModelResponse<List<UsuarioRol>> ObtenerUsuarioRolesPorUsuario(long usuarioId)
        {
            var modelResponse = new ModelResponse<List<UsuarioRol>>();

            try
            {
                var usuarioRoles = GetObjects("ObtenerUsuarioRolesPorUsuario", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@UsuarioId", usuarioId) },
                    new Func<IDataReader, UsuarioRol>((reader) =>
                    {
                        var ur = LlenarEntidad<UsuarioRol>(reader);
                        return ur;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuarioRoles.ToList();
                modelResponse.Message = "Asignaciones usuario-rol obtenidas correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener las asignaciones usuario-rol del usuario {UsuarioId}", usuarioId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las asignaciones usuario-rol del usuario";
            }

            return modelResponse;
        }
    }
}