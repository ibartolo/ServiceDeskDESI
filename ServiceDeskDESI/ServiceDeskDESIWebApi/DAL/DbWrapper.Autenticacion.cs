using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Helpers;
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
        public ModelResponse ObtenerUsuarios(long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var usuarios = GetObjects("ObtenerUsuarios", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@EmpresaId", empresaId) },
                    new Func<IDataReader, Usuario>((reader) =>
                    {
                        var usuario = LlenarEntidad<Usuario>(reader);

                        usuario.Sucursal = new Sucursal()
                        {
                            Id = MapearPorpiedades<long>(reader["SucursalId"]),
                            Nombre = MapearPorpiedades<string>(reader["SucursalNombre"])
                        };

                        usuario.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        usuario.Empresa = new Empresa()
                        {
                            Id = MapearPorpiedades<long>(reader["EmpresaId"]),
                            NombreComercial = MapearPorpiedades<string>(reader["EmpresaNombre"])
                        };

                        return usuario;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuarios;
                modelResponse.Message = "Usuarios obtenidos correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los usuarios";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerUsuarioPorId(long id, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var usuario = GetObject("ObtenerUsuarioPorId", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@EmpresaId", empresaId)
                    },
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

                if (usuario == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el usuario especificado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuario;
                modelResponse.Message = "Usuario obtenido correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
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
                // Validaciones
                if (string.IsNullOrWhiteSpace(u.NombreUsuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (u.NombreUsuario.Length > 25) { throw new ArgumentException("El nombre de usuario no puede exceder los 25 caracteres."); }
                if (string.IsNullOrWhiteSpace(u.Contrasena)) { throw new ArgumentException("La contraseña es requerida."); }
                if (u.Contrasena.Length > 250) { throw new ArgumentException("La contraseña no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(u.Correo)) { throw new ArgumentException("El correo es requerido."); }
                if (u.Correo.Length > 250) { throw new ArgumentException("El correo no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(u.Nombre)) { throw new ArgumentException("El nombre es requerido."); }
                if (u.Nombre.Length > 150) { throw new ArgumentException("El nombre no puede exceder los 150 caracteres."); }
                if (string.IsNullOrWhiteSpace(u.Apellido)) { throw new ArgumentException("El apellido es requerido."); }
                if (u.Apellido.Length > 250) { throw new ArgumentException("El apellido no puede exceder los 250 caracteres."); }
                if (u.Sucursal == null || u.Sucursal.Id <= 0) { throw new ArgumentException("La sucursal es requerida."); }
                if (u.Area == null || u.Area.Id <= 0) { throw new ArgumentException("El área es requerida."); }
                if (u.Empresa == null || u.Empresa.Id <= 0) { throw new ArgumentException("La empresa es requerida."); }
                if (string.IsNullOrWhiteSpace(u.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

                // Crear objeto anónimo con los nombres de parámetros correctos
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
                    EmpresaId = u.Empresa.Id,
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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el usuario";
            }

            return modelResponse;
        }

        public ModelResponse GuardarNuevaEmpresaConDatosIniciales(Empresa empresa)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // =========================================
                // VALIDACIONES DE EMPRESA
                // =========================================
                if (string.IsNullOrWhiteSpace(empresa.NombreComercial)) { throw new ArgumentException("El nombre comercial es requerido."); }
                if (empresa.NombreComercial.Length > 250) { throw new ArgumentException("El nombre comercial no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.RazonSocial)) { throw new ArgumentException("La razón social es requerida."); }
                if (empresa.RazonSocial.Length > 250) { throw new ArgumentException("La razón social no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.RFC)) { throw new ArgumentException("El RFC es requerido."); }
                if (empresa.RFC.Length > 50) { throw new ArgumentException("El RFC no puede exceder los 50 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.Responsable)) { throw new ArgumentException("El responsable es requerido."); }
                if (empresa.Responsable.Length > 250) { throw new ArgumentException("El responsable no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.Direccion)) { throw new ArgumentException("La dirección es requerida."); }
                if (empresa.Direccion.Length > 500) { throw new ArgumentException("La dirección no puede exceder los 500 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.CorreoContacto)) { throw new ArgumentException("El correo de contacto es requerido."); }
                if (empresa.CorreoContacto.Length > 250) { throw new ArgumentException("El correo de contacto no puede exceder los 250 caracteres."); }

                // =========================================
                // PASO 1: GUARDAR EMPRESA
                // =========================================
                empresa.FechaVigenciaInicio = DateTime.Now;
                empresa.FechaVigenciaFin = DateTime.Now.AddDays(30);
                empresa.EsPeriodoPrueba = true;
                empresa.CreadoPor = "system.register";
                empresa.FechaCreacion = DateTime.Now;
                empresa.Estatus = true;

                var empresaResponse = GuardarOActualizarEmpresas(empresa);

                if (!empresaResponse.IsSuccess || empresaResponse.Response == null)
                {
                    throw new Exception(empresaResponse.Message ?? "Error al guardar la empresa");
                }

                var empresaGuardada = (Empresa)empresaResponse.Response;
                var usernameAdmin = $"admin_{empresaGuardada.Id}";

                // =========================================
                // PASO 2: GUARDAR SUCURSAL
                // =========================================
                var sucursal = new Sucursal()
                {
                    Nombre = empresaGuardada.NombreComercial,
                    Descripcion = $"Sucursal principal de {empresaGuardada.NombreComercial}",
                    Calle = empresaGuardada.Direccion,
                    Ciudad = empresaGuardada.Ciudad,
                    Colonia = null,
                    CodigoPostal = empresaGuardada.CodigoPostal,
                    CreadoPor = usernameAdmin,
                    FechaCreacion = DateTime.Now,
                    Estatus = true
                };

                var sucursalResponse = GuardarOActualizarSucursales(sucursal);

                if (!sucursalResponse.IsSuccess || sucursalResponse.Response == null)
                {
                    throw new Exception(sucursalResponse.Message ?? "Error al guardar la sucursal");
                }

                var sucursalGuardada = (Sucursal)sucursalResponse.Response;

                // =========================================
                // PASO 3: GUARDAR ÁREA (TI)
                // =========================================
                var area = new Area()
                {
                    Nombre = "TI",
                    Descripcion = "Área de Tecnologías de la Información",
                    Correo = empresaGuardada.CorreoContacto,
                    CreadoPor = usernameAdmin,
                    FechaCreacion = DateTime.Now,
                    Estatus = true
                };

                var areaResponse = GuardarOActualizarArea(area);

                if (!areaResponse.IsSuccess || areaResponse.Response == null)
                {
                    throw new Exception(areaResponse.Message ?? "Error al guardar el área");
                }

                var areaGuardada = (Area)areaResponse.Response;

                // =========================================
                // PASO 4: GUARDAR USUARIO ADMINISTRADOR
                // =========================================
                var usuarioAdmin = new Usuario()
                {
                    NombreUsuario = usernameAdmin,
                    Contrasena = Cryptography.Encrypt("Admin123!"),
                    ImagenPerfil = null,
                    Correo = empresaGuardada.CorreoContacto,
                    Nombre = "Administrador",
                    Apellido = "Sistema",
                    Celular = empresaGuardada.Telefono,
                    Sucursal = sucursalGuardada,
                    Firma = null,
                    RFC = empresaGuardada.RFC,
                    Area = areaGuardada,
                    Empresa = empresaGuardada,
                    CreadoPor = usernameAdmin,
                    FechaCreacion = DateTime.Now,
                    Estatus = true
                };

                var usuarioResponse = GuardarOActualizarUsuario(usuarioAdmin);

                if (!usuarioResponse.IsSuccess)
                {
                    throw new Exception(usuarioResponse.Message ?? "Error al guardar el usuario administrador");
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = empresaGuardada;
                modelResponse.Message = "Empresa registrada correctamente con sucursal, área y usuario administrador te llegara un correo con tus datos para poder autenticarte.";

                // Enviar correo de bienvenida
                EnviarCorreoBienvenida(empresaGuardada, usernameAdmin, "Admin123!");
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }

            return modelResponse;
        }

        public ModelResponse EliminarUsuario(long id, string modificadoPor, DateTime fechaModificacion)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }

                ExecuteNonQuery("EliminarUsuario", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Usuario eliminado correctamente.";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
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
                if (string.IsNullOrWhiteSpace(nombreUsuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(contrasena)) { throw new ArgumentException("La contraseña es requerida."); }

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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al autenticar el usuario.";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerUsuarioPorNombreUsuario(string nombreUsuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(nombreUsuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var usuario = GetObject("ObtenerUsuarioPorNombreUsuario", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@NombreUsuario", nombreUsuario),
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
                            NombreComercial = MapearPorpiedades<string>(reader["EmpresaNombre"])
                        };

                        return u;
                    }));

                if (usuario == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el usuario especificado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = usuario;
                modelResponse.Message = "Usuario obtenido correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el usuario";
            }

            return modelResponse;
        }

        public ModelResponse InsertarTokenRecuperacion(long usuarioId, string token, DateTime fechaExpiracion, string creadoPor)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(token)) { throw new ArgumentException("El token es requerido."); }
                if (fechaExpiracion <= DateTime.Now) { throw new ArgumentException("La fecha de expiración debe ser mayor a la fecha actual."); }
                if (string.IsNullOrWhiteSpace(creadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
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
                if (string.IsNullOrWhiteSpace(token)) { throw new ArgumentException("El token es requerido."); }

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

                if (result == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El token no es válido o ha expirado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = result;
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
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
                if (id <= 0) { throw new ArgumentException("El ID del token es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }

                ExecuteNonQuery("ActualizarTokenUsado", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", DateTime.Now)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Token actualizado correctamente.";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al actualizar el token.";
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
                if (usuario.Contrasena.Length > 250) { throw new ArgumentException("La contraseña no puede exceder los 250 caracteres."); }
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
                    modelResponse.Message = "Contraseña actualizada correctamente.";
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
                modelResponse.Message = "Ocurrió un error al actualizar la contraseña.";
            }

            return modelResponse;
        }
        public ModelResponse ObtenerUsuarioPorCorreo(string correo)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(correo)) { throw new ArgumentException("El correo es requerido."); }

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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el usuario";
            }

            return modelResponse;
        }

        private void EnviarCorreoBienvenida(Empresa empresa, string usuario, string contrasenaTemporal)
        {
            try
            {
                // Obtener URL base del Web.config
                string baseUri = System.Configuration.ConfigurationManager.AppSettings["BaseUri"];
                string urlLogin = $"{baseUri}Home/Autentication";

                // Leer template
                string templatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Template/Template_AltaEmpresa.html");
                string templateHtml = System.IO.File.ReadAllText(templatePath);

                // Reemplazar variables en el template
                templateHtml = templateHtml.Replace("{{NombreCompleto}}", empresa.Responsable);
                templateHtml = templateHtml.Replace("{{NombreEmpresa}}", empresa.NombreComercial);
                templateHtml = templateHtml.Replace("{{RFC}}", empresa.RFC);
                templateHtml = templateHtml.Replace("{{CorreoContacto}}", empresa.CorreoContacto);
                templateHtml = templateHtml.Replace("{{Usuario}}", usuario);
                templateHtml = templateHtml.Replace("{{ContrasenaTemporal}}", contrasenaTemporal);
                templateHtml = templateHtml.Replace("{{UrlLogin}}", urlLogin);

                // Enviar correo
                var para = new List<string> { empresa.CorreoContacto };
                EmailHelper.EnvioEmaiil(para, "Bienvenido a Service Desk DESI - Tus credenciales de acceso", templateHtml, false);
            }
            catch (Exception ex)
            {
                // Solo registrar el error, no afectar el flujo principal
                // Logger.Error("Error al enviar correo de bienvenida", ex);
            }
        }
    }
}