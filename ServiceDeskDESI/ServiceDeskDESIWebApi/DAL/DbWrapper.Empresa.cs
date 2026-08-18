using Serilog;
using ServiceDeskDESIEntities.Catalogos;
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
        public ModelResponse ObtenerEmpresaPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = GetObject("ObtenerEmpresaPorId", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Empresa>((reader) =>
                    {
                        var r = LlenarEntidad<Empresa>(reader);
                        return r;
                    }));

                if (result == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la empresa especificada.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = result;
                modelResponse.Message = "Empresa obtenida correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener empresa {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la empresa";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerEmpresaPorRFC(string rfc)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = GetObject("ObtenerEmpresaPorRFC", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@RFC", rfc) },
                    new Func<IDataReader, Empresa>((reader) =>
                    {
                        var r = LlenarEntidad<Empresa>(reader);
                        return r;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = result;
                modelResponse.Message = "Empresa obtenida correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener empresa por RFC {RFC}", rfc);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la empresa";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerEmpresaPorCorreoContacto(string correoContacto)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = GetObject("ObtenerEmpresaPorCorreoContacto", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@CorreoContacto", correoContacto) },
                    new Func<IDataReader, Empresa>((reader) =>
                    {
                        var r = LlenarEntidad<Empresa>(reader);
                        return r;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener empresa por correo de contacto {Correo}", correoContacto);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la empresa";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerEmpresaPorNombreComercial(string nombreComercial)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = GetObject("ObtenerEmpresaPorNombreComercial", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@NombreComercial", nombreComercial) },
                    new Func<IDataReader, Empresa>((reader) =>
                    {
                        var r = LlenarEntidad<Empresa>(reader);
                        return r;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener empresa por nombre comercial {NombreComercial}", nombreComercial);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la empresa";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerEmpresaPorRazonSocial(string razonSocial)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = GetObject("ObtenerEmpresaPorRazonSocial", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@RazonSocial", razonSocial) },
                    new Func<IDataReader, Empresa>((reader) =>
                    {
                        var r = LlenarEntidad<Empresa>(reader);
                        return r;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener empresa por razón social {RazonSocial}", razonSocial);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la empresa";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarEmpresa(Empresa e, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var parametrosObj = new
                {
                    e.Id,
                    e.NombreComercial,
                    e.RazonSocial,
                    e.RFC,
                    e.Responsable,
                    e.Direccion,
                    e.Ciudad,
                    e.Estado,
                    e.CodigoPostal,
                    e.Telefono,
                    e.CorreoContacto,
                    e.FechaVigenciaInicio,
                    e.FechaVigenciaFin,
                    e.EsPeriodoPrueba,
                    e.CreadoPor,
                    e.FechaCreacion,
                    e.ModificadoPor,
                    e.FechaModificacion,
                    e.Estatus,
                    Usuario = usuario
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var empresaId = ExecuteScalar("GuardarOActualizarEmpresa", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(empresaId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                e.Id = Convert.ToInt64(empresaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = e;
                modelResponse.Message = "Empresa guardada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar empresa");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar la empresa";
            }

            return modelResponse;
        }

        public ModelResponse GuardarNuevaEmpresa(Empresa e)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var parametrosObj = new
                {
                    e.NombreComercial,
                    e.RazonSocial,
                    e.RFC,
                    e.Responsable,
                    e.Direccion,
                    e.Ciudad,
                    e.Estado,
                    e.CodigoPostal,
                    e.Telefono,
                    e.CorreoContacto,
                    e.FechaVigenciaInicio,
                    e.FechaVigenciaFin,
                    e.EsPeriodoPrueba,
                    e.CreadoPor,
                    e.FechaCreacion
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var empresaId = ExecuteScalar("GuardarNuevaEmpresa", CommandType.StoredProcedure, parametros);
                e.Id = Convert.ToInt64(empresaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = e;
                modelResponse.Message = "Empresa registrada correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al registrar nueva empresa");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al registrar la empresa.";
            }

            return modelResponse;
        }

        public ModelResponse EliminarEmpresa(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = ExecuteScalar("EliminarEmpresa", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion),
                    new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta empresa.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Empresa eliminada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar empresa {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la empresa";
            }

            return modelResponse;
        }

        public ModelResponse GuardarRolParaNuevaEmpresa(Rol rol)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var parametrosObj = new
                {
                    rol.Nombre,
                    rol.Descripcion,
                    rol.PuedeAtenderTickets,
                    rol.CreadoPor,
                    rol.FechaCreacion
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var rolId = ExecuteScalar("GuardarRolParaNuevaEmpresa", CommandType.StoredProcedure, parametros);
                rol.Id = Convert.ToInt64(rolId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = rol;
                modelResponse.Message = "Rol creado exitosamente para la nueva empresa.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al crear rol para nueva empresa");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al crear el rol para la empresa.";
            }

            return modelResponse;
        }

        public ModelResponse AsignarRolUsuarioParaNuevaEmpresa(long usuarioId, long rolId, string creadoPor)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("AsignarRolUsuarioParaNuevaEmpresa", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@UsuarioId", usuarioId),
                    new SqlParameter("@RolId", rolId),
                    new SqlParameter("@CreadoPor", creadoPor),
                    new SqlParameter("@FechaCreacion", DateTime.Now)
                });

                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El usuario o el rol no existen.";
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
                modelResponse.Message = "Rol asignado al usuario exitosamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al asignar rol {RolId} al usuario {UsuarioId} para nueva empresa", rolId, usuarioId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al asignar el rol al usuario.";
            }

            return modelResponse;
        }

        public ModelResponse InsertarUsuarioPaginaParaNuevaEmpresa(long usuarioId, long paginaId, string creadoPor)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("InsertarUsuarioPaginaParaNuevaEmpresa", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@UsuarioId", usuarioId),
                    new SqlParameter("@PaginaId", paginaId),
                    new SqlParameter("@CreadoPor", creadoPor),
                    new SqlParameter("@FechaCreacion", DateTime.Now)
                });

                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El usuario o la página no existen.";
                    return modelResponse;
                }

                if (resultadoLong == -1)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El usuario ya tiene asignada esta página.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = resultadoLong;
                modelResponse.Message = "Página asignada al usuario exitosamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al asignar página {PaginaId} al usuario {UsuarioId} para nueva empresa", paginaId, usuarioId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al asignar la página al usuario.";
            }

            return modelResponse;
        }
    }
}