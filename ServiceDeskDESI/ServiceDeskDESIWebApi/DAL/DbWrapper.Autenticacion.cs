using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
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
        public ModelResponse ObtenerUsuarios(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var usuarios = GetObjects("ObtenerUsuarios", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Usuario>((reader) =>
                    {
                        var u = LlenarEntidad<Usuario>(reader);

                        u.Sucursal = new Sucursal()
                        {
                            Id = MapearPorpiedades<long>(reader["SucursalId"]),
                            Nombre = MapearPorpiedades<string>(reader["SucursalNombre"])
                        };

                        u.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        u.Empresa = new Empresa()
                        {
                            Id = MapearPorpiedades<long>(reader["EmpresaId"]),
                            NombreComercial = MapearPorpiedades<string>(reader["EmpresaNombre"])
                        };

                        return u;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuarios;
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

        public ModelResponse ObtenerUsuarioPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var u = GetObject("ObtenerUsuarioPorId", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Usuario>((reader) =>
                    {
                        var user = LlenarEntidad<Usuario>(reader);

                        user.Sucursal = new Sucursal()
                        {
                            Id = MapearPorpiedades<long>(reader["SucursalId"]),
                            Nombre = MapearPorpiedades<string>(reader["SucursalNombre"])
                        };

                        user.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        user.Empresa = new Empresa()
                        {
                            Id = MapearPorpiedades<long>(reader["EmpresaId"]),
                            NombreComercial = MapearPorpiedades<string>(reader["EmpresaNombre"])
                        };

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

        public ModelResponse ObtenerUsuarioPorNombreUsuario(string nombreUsuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var u = GetObject("ObtenerUsuarioPorNombreUsuario", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@NombreUsuario", nombreUsuario)
                    },
                    new Func<IDataReader, Usuario>((reader) =>
                    {
                        var user = LlenarEntidad<Usuario>(reader);

                        user.Sucursal = new Sucursal()
                        {
                            Id = MapearPorpiedades<long>(reader["SucursalId"]),
                            Nombre = MapearPorpiedades<string>(reader["SucursalNombre"])
                        };

                        user.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        user.Empresa = new Empresa()
                        {
                            Id = MapearPorpiedades<long>(reader["EmpresaId"]),
                            NombreComercial = MapearPorpiedades<string>(reader["EmpresaNombre"])
                        };

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

        public ModelResponse ObtenerUsuarioPorCorreo(string correo)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var usuario = GetObject("ObtenerUsuarioPorCorreo", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Correo", correo) },
                    new Func<IDataReader, Usuario>((reader) =>
                    {
                        var u = LlenarEntidad<Usuario>(reader);

                        u.Sucursal = new Sucursal()
                        {
                            Id = MapearPorpiedades<long>(reader["SucursalId"]),
                            Nombre = MapearPorpiedades<string>(reader["SucursalNombre"])
                        };

                        u.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        u.Empresa = new Empresa()
                        {
                            Id = MapearPorpiedades<long>(reader["EmpresaId"]),
                            NombreComercial = MapearPorpiedades<string>(reader["EmpresaNombre"])
                        };

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

        public ModelResponse GuardarOActualizarUsuario(Usuario u)
        {
            var modelResponse = new ModelResponse();

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
                    SucursalId = u.Sucursal.Id,
                    u.Firma,
                    u.RFC,
                    AreaId = u.Area.Id,
                    EmpresaId = u.Empresa.Id
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

        public ModelResponse GuardarOActualizarUsuarioAdmin(Usuario usuario, string usuarioAdmin)
        {
            var modelResponse = new ModelResponse();

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
                    SucursalId = usuario.Sucursal.Id,
                    usuario.Firma,
                    usuario.RFC,
                    AreaId = usuario.Area.Id,
                    EmpresaId = usuario.Empresa.Id,
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

        public ModelResponse ActualizarPerfilUsuario(Usuario usuario, string usuarioAutenticado)
        {
            var modelResponse = new ModelResponse();

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
                    SucursalId = usuario.Sucursal.Id,
                    usuario.Firma,
                    usuario.RFC,
                    AreaId = usuario.Area.Id,
                    EmpresaId = usuario.Empresa.Id,
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

                        u.Empresa = new Empresa()
                        {
                            Id = MapearPorpiedades<long>(reader["EmpresaId"]),
                            NombreComercial = MapearPorpiedades<string>(reader["EmpresaNombreComercial"]),
                            RazonSocial = MapearPorpiedades<string>(reader["EmpresaRazonSocial"]),
                            RFC = MapearPorpiedades<string>(reader["EmpresaRFC"]),
                            Responsable = MapearPorpiedades<string>(reader["EmpresaResponsable"]),
                            Direccion = MapearPorpiedades<string>(reader["EmpresaDireccion"]),
                            Ciudad = MapearPorpiedades<string>(reader["EmpresaCiudad"]),
                            Estado = MapearPorpiedades<string>(reader["EmpresaEstado"]),
                            CodigoPostal = MapearPorpiedades<string>(reader["EmpresaCodigoPostal"]),
                            Telefono = MapearPorpiedades<string>(reader["EmpresaTelefono"]),
                            CorreoContacto = MapearPorpiedades<string>(reader["EmpresaCorreoContacto"]),
                            FechaVigenciaInicio = MapearPorpiedades<DateTime>(reader["FechaVigenciaInicio"]),
                            FechaVigenciaFin = MapearPorpiedades<DateTime>(reader["FechaVigenciaFin"]),
                            EsPeriodoPrueba = MapearPorpiedades<bool>(reader["EsPeriodoPrueba"])
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

        public ModelResponse ObtenerTokenRecuperacion(string token)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = GetObject("ObtenerTokenRecuperacion", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Token", token) },
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