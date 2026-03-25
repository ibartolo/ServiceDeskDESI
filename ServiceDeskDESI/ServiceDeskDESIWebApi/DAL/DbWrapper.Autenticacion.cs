using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
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
        public ModelResponse ObtenerUsuarios()
        {
            var modelResponse = new ModelResponse();

            try
            {
                var usuarios = GetObjects("ObtenerUsuarios", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                    new Func<IDataReader, Usuario>((reader) =>
                    {
                        var usuario = LlenarEntidad<Usuario>(reader);

                        usuario.Sucursal = new Sucursal()
                        {
                            Id = MapearPorpiedades<long>(reader["SucursalId"])
                        };

                        usuario.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"])
                        };

                        return usuario;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuarios;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse ObtenerUsuarioPorId(long id)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var usuario = GetObject("ObtenerUsuarioPorId", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@Id", id)
                    },
                    new Func<IDataReader, Usuario>((reader) =>
                    {
                        var u = LlenarEntidad<Usuario>(reader);

                        u.Sucursal = new Sucursal()
                        {
                            Id = MapearPorpiedades<long>(reader["SucursalId"])
                        };

                        u.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"])
                        };

                        return u;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuario;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse GuardarOActualizarUsuario(Usuario u)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var parametros = ObtenerParametrosSQL(u).ToArray();
                var usuarioId = ExecuteScalar("GuardarOActualizarUsuario", CommandType.StoredProcedure, parametros);
                u.Id = Convert.ToInt64(usuarioId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = u;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
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
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse AutenticarUsuario(string nombreUsuario, string contrasena)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var usuario = GetObject("AutenticarUsuario", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@NombreUsuario", nombreUsuario),
                new SqlParameter("@Contrasena", contrasena)
                    },
                    new Func<IDataReader, Usuario>((reader) =>
                    {
                        var u = LlenarEntidad<Usuario>(reader);

                        u.Sucursal = new Sucursal()
                        {
                            Id = MapearPorpiedades<long>(reader["SucursalId"]),
                            Nombre = MapearPorpiedades<string>(reader["SucursalNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["SucursalDescripcion"]),
                            Calle = MapearPorpiedades<string>(reader["Calle"]),
                            Ciudad = MapearPorpiedades<string>(reader["Ciudad"]),
                            Colonia = MapearPorpiedades<string>(reader["Colonia"]),
                            CodigoPostal = MapearPorpiedades<string>(reader["CodigoPostal"])
                        };

                        u.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["AreaDescripcion"]),
                            Correo = MapearPorpiedades<string>(reader["AreaCorreo"])
                        };

                        return u;
                    }));

                if (usuario != null)
                {
                    modelResponse.IsSuccess = true;
                    modelResponse.Response = usuario;
                }
                else
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Response = null;
                    modelResponse.Message = "Usuario o contraseña incorrectos";
                }
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse ObtenerUsuarioPorNombreUsuario(string nombreUsuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var usuario = GetObject("ObtenerUsuarioPorNombreUsuario", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@NombreUsuario", nombreUsuario)
                    },
                    new Func<IDataReader, Usuario>((reader) =>
                    {
                        var u = LlenarEntidad<Usuario>(reader);

                        u.Sucursal = new Sucursal()
                        {
                            Id = MapearPorpiedades<long>(reader["SucursalId"]),
                            Nombre = MapearPorpiedades<string>(reader["SucursalNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["SucursalDescripcion"]),
                            Calle = MapearPorpiedades<string>(reader["Calle"]),
                            Ciudad = MapearPorpiedades<string>(reader["Ciudad"]),
                            Colonia = MapearPorpiedades<string>(reader["Colonia"]),
                            CodigoPostal = MapearPorpiedades<string>(reader["CodigoPostal"])
                        };

                        u.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["AreaDescripcion"]),
                            Correo = MapearPorpiedades<string>(reader["AreaCorreo"])
                        };

                        return u;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuario;
                modelResponse.Message = "Usuario obtenido correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        // Insertar token de recuperación
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
                modelResponse.Message = "Token guardado correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }

        // Obtener token de recuperación para validación
        public ModelResponse ObtenerTokenRecuperacion(string token)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = GetObject("ObtenerTokenRecuperacion", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@Token", token)
                    },
                    new Func<IDataReader, dynamic>((reader) =>
                    {
                        return new
                        {
                            Id = MapearPorpiedades<long>(reader["Id"]),
                            UsuarioId = MapearPorpiedades<long>(reader["UsuarioId"]),
                            Token = MapearPorpiedades<string>(reader["Token"]),
                            FechaExpiracion = MapearPorpiedades<DateTime>(reader["FechaExpiracion"]),
                            Usado = MapearPorpiedades<bool>(reader["Usado"]),
                            Nombre = MapearPorpiedades<string>(reader["Nombre"]),
                            Apellido = MapearPorpiedades<string>(reader["Apellido"]),
                            Correo = MapearPorpiedades<string>(reader["Correo"]),
                            NombreUsuario = MapearPorpiedades<string>(reader["NombreUsuario"])
                        };
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = result;
                modelResponse.Message = "Token obtenido correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }

        // Actualizar token como usado
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
                modelResponse.Message = "Token actualizado correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }

            return modelResponse;
        }
        public ModelResponse ActualizarContrasena(Usuario usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                if (usuario.Id <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario.Contrasena)) { throw new ArgumentException("La contraseña es requerida."); }
                if (usuario.Contrasena.Length < 6) { throw new ArgumentException("La contraseña debe tener al menos 6 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.ModificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }

                var result = ExecuteScalar("ActualizarContrasena", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Id", usuario.Id),
            new SqlParameter("@Contrasena", usuario.Contrasena),
            new SqlParameter("@ModificadoPor", usuario.ModificadoPor),
            new SqlParameter("@FechaModificacion", usuario.FechaModificacion ?? DateTime.Now)
                });

                long idActualizado = Convert.ToInt64(result);

                if (idActualizado > 0)
                {
                    modelResponse.IsSuccess = true;
                    modelResponse.Message = "Contraseña actualizada correctamente";
                    modelResponse.Response = idActualizado;
                }
                else
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se pudo actualizar la contraseña. El usuario no existe o está inactivo.";
                }
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al actualizar la contraseña";
            }

            return modelResponse;
        }
    }
}