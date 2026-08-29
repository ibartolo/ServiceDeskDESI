using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Serilog;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper
    {
        public ModelResponse<List<UsuarioDTO>> ObtenerUsuarios(string usuario)
        {
            var modelResponse = new ModelResponse<List<UsuarioDTO>>();

            try
            {
                var usuarios = GetObjects("ObtenerUsuarios", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, UsuarioDTO>((reader) =>
                    {
                        var u = LlenarEntidad<UsuarioDTO>(reader);

                        u.Contrasena = null;

                        return u;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuarios.ToList();
                modelResponse.Message = "Usuarios obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener usuarios para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los usuarios";
            }

            return modelResponse;
        }

        public ModelResponse<UsuarioDTO> ObtenerUsuarioPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse<UsuarioDTO>();

            try
            {
                var u = GetObject("ObtenerUsuarioPorId", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, UsuarioDTO>((reader) =>
                    {
                        var user = LlenarEntidad<UsuarioDTO>(reader);

                        user.Contrasena = null;

                        return user;
                    }));

                if (u == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el usuario especificado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = u;
                modelResponse.Message = "Usuario obtenido correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener usuario {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el usuario";
            }

            return modelResponse;
        }

        public ModelResponse<UsuarioDTO> ObtenerUsuarioPorNombreUsuario(string nombreUsuario, string usuario)
        {
            var modelResponse = new ModelResponse<UsuarioDTO>();

            try
            {
                var u = GetObject("ObtenerUsuarioPorNombreUsuario", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@NombreUsuario", nombreUsuario),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, UsuarioDTO>((reader) =>
                    {
                        var user = LlenarEntidad<UsuarioDTO>(reader);

                        user.Contrasena = null;

                        return user;
                    }));

                if (u == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el usuario especificado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = u;
                modelResponse.Message = "Usuario obtenido correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener usuario por nombre {NombreUsuario} para usuario {Usuario}", nombreUsuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el usuario";
            }

            return modelResponse;
        }

        public ModelResponse<UsuarioDTO> ObtenerUsuarioPorCorreo(string correo)
        {
            var modelResponse = new ModelResponse<UsuarioDTO>();

            try
            {
                var usuario = GetObject("ObtenerUsuarioPorCorreo", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Correo", correo) },
                    new Func<IDataReader, UsuarioDTO>((reader) =>
                    {
                        var u = LlenarEntidad<UsuarioDTO>(reader);

                        u.Contrasena = null;

                        return u;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuario;
                modelResponse.Message = "Usuario obtenido correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener usuario por correo {Correo}", correo);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el usuario";
            }

            return modelResponse;
        }

        public ModelResponse<Usuario> GuardarOActualizarUsuario(Usuario u)
        {
            var modelResponse = new ModelResponse<Usuario>();

            try
            {
                var parametrosObj = new
                {
                    u.Id,
                    u.NombreUsuario,
                    u.Contrasena,
                    u.ImagenPerfil,
                    u.Correo,
                    u.Nombre,
                    u.Apellido,
                    u.Celular,
                    u.CreadoPor,
                    u.FechaCreacion,
                    u.ModificadoPor,
                    u.FechaModificacion,
                    u.Estatus,
                    u.SucursalId,
                    u.Firma,
                    u.RFC,
                    u.AreaId,
                    u.EmpresaId
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var usuarioId = ExecuteScalar("GuardarOActualizarUsuario", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(usuarioId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para modificar este usuario.";
                    return modelResponse;
                }

                u.Id = Convert.ToInt64(usuarioId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = u;
                modelResponse.Message = "Usuario guardado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar usuario");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el usuario";
            }

            return modelResponse;
        }

        public ModelResponse<Usuario> GuardarOActualizarUsuarioAdmin(Usuario usuario, string usuarioAdmin)
        {
            var modelResponse = new ModelResponse<Usuario>();

            try
            {
                var parametrosObj = new
                {
                    usuario.Id,
                    usuario.NombreUsuario,
                    usuario.Contrasena,
                    usuario.ImagenPerfil,
                    usuario.Correo,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Celular,
                    usuario.CreadoPor,
                    usuario.FechaCreacion,
                    usuario.ModificadoPor,
                    usuario.FechaModificacion,
                    usuario.Estatus,
                    usuario.SucursalId,
                    usuario.Firma,
                    usuario.RFC,
                    usuario.AreaId,
                    usuario.EmpresaId,
                    UsuarioAdmin = usuarioAdmin
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var resultado = ExecuteScalar("GuardarOActualizarUsuarioAdmin", CommandType.StoredProcedure, parametros);
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
                    modelResponse.Message = "El nombre de usuario ya existe en esta empresa.";
                    return modelResponse;
                }

                if (resultadoLong == -2)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El correo electrónico ya está registrado en esta empresa.";
                    return modelResponse;
                }

                usuario.Id = resultadoLong;

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuario;
                modelResponse.Message = "Usuario guardado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar usuario por administrador {UsuarioAdmin}", usuarioAdmin);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el usuario.";
            }

            return modelResponse;
        }

        public ModelResponse<Usuario> ActualizarPerfilUsuario(Usuario usuario, string usuarioAutenticado)
        {
            var modelResponse = new ModelResponse<Usuario>();

            try
            {
                var parametrosObj = new
                {
                    usuario.Id,
                    usuario.NombreUsuario,
                    usuario.ImagenPerfil,
                    usuario.Correo,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Celular,
                    usuario.ModificadoPor,
                    usuario.FechaModificacion,
                    usuario.Estatus,
                    usuario.SucursalId,
                    usuario.Firma,
                    usuario.RFC,
                    usuario.AreaId,
                    usuario.EmpresaId,
                    Usuario = usuarioAutenticado
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var resultado = ExecuteScalar("ActualizarPerfilUsuario", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(resultado) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para actualizar este perfil o el usuario no existe.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuario;
                modelResponse.Message = "Perfil actualizado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al actualizar perfil del usuario {UsuarioId} por {UsuarioAutenticado}", usuario.Id, usuarioAutenticado);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al actualizar el perfil.";
            }

            return modelResponse;
        }

        public ModelResponse EliminarUsuario(long id, string modificadoPor, DateTime fechaModificacion)
        {
            var modelResponse = new ModelResponse();

            try
            {
                ExecuteNonQuery("EliminarUsuario", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Usuario eliminado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar usuario {Id}", id);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el usuario.";
            }

            return modelResponse;
        }

        public ModelResponse<UsuarioDTO> AutenticarUsuario(string nombreUsuario, string contrasena)
        {
            var modelResponse = new ModelResponse<UsuarioDTO>();

            try
            {
                var usuario = GetObject("AutenticarUsuario", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@NombreUsuario", nombreUsuario)
                    },
                    new Func<IDataReader, UsuarioDTO>((reader) =>
                    {
                        var u = LlenarEntidad<UsuarioDTO>(reader);
                        return u;
                    }));

                if (usuario != null && Cryptography.VerifyPassword(contrasena, usuario.Contrasena))
                {
                    // Enforce trial: si la empresa está en periodo de prueba y ya venció, se bloquea el acceso.
                    if (usuario.EsPeriodoPrueba == true && usuario.FechaVigenciaFin.HasValue && usuario.FechaVigenciaFin.Value < DateTime.Now)
                    {
                        modelResponse.IsSuccess = false;
                        modelResponse.Message = "El periodo de prueba de su empresa ha expirado. Contacte al administrador.";
                        return modelResponse;
                    }

                    usuario.Contrasena = null; // no exponer el hash/ciphertext en la respuesta
                    modelResponse.IsSuccess = true;
                    modelResponse.Response = usuario;
                }
                else
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Usuario o contraseña incorrectos.";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al autenticar usuario");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al autenticar el usuario.";
            }

            return modelResponse;
        }

        public ModelResponse InsertarTokenRecuperacion(long usuarioId, string token, DateTime fechaExpiracion, string creadoPor)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var tokenId = ExecuteScalar("InsertarTokenRecuperacion", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@UsuarioId", usuarioId),
                    new SqlParameter("@Token", token),
                    new SqlParameter("@FechaExpiracion", fechaExpiracion),
                    new SqlParameter("@CreadoPor", creadoPor),
                    new SqlParameter("@FechaCreacion", DateTime.Now)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Response = Convert.ToInt64(tokenId);
                modelResponse.Message = "Token guardado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al insertar token de recuperación para usuario {UsuarioId}", usuarioId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el token.";
            }

            return modelResponse;
        }

        public ModelResponse<TokenRecuperacionDTO> ObtenerTokenRecuperacion(string token)
        {
            var modelResponse = new ModelResponse<TokenRecuperacionDTO>();

            try
            {
                var result = GetObject("ObtenerTokenRecuperacion", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Token", token) },
                    new Func<IDataReader, TokenRecuperacionDTO>(r => LlenarEntidad<TokenRecuperacionDTO>(r)));

                if (result == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El token no es válido o ha expirado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener token de recuperación");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el token.";
            }

            return modelResponse;
        }

        public ModelResponse ActualizarTokenUsado(long id, string modificadoPor)
        {
            var modelResponse = new ModelResponse();

            try
            {
                ExecuteNonQuery("ActualizarTokenUsado", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", DateTime.Now)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Token actualizado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al actualizar token usado {Id}", id);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al actualizar el token.";
            }

            return modelResponse;
        }

        public ModelResponse ActualizarContrasena(Usuario usuario, string usuarioAutenticado)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = ExecuteScalar("ActualizarContrasena", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", usuario.Id),
                    new SqlParameter("@Contrasena", usuario.Contrasena),
                    new SqlParameter("@ModificadoPor", usuario.ModificadoPor),
                    new SqlParameter("@FechaModificacion", usuario.FechaModificacion ?? DateTime.Now),
                    new SqlParameter("@Usuario", usuarioAutenticado)
                });

                long idActualizado = Convert.ToInt64(result);

                if (idActualizado > 0)
                {
                    modelResponse.IsSuccess = true;
                    modelResponse.Message = "Contraseña actualizada correctamente.";
                    modelResponse.Response = idActualizado;
                }
                else
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se pudo actualizar la contraseña. El usuario no existe o está inactivo.";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al actualizar contraseña para usuario {Id}", usuario.Id);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al actualizar la contraseña.";
            }

            return modelResponse;
        }
    }
}
