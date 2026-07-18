using Serilog;
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
        public ModelResponse ObtenerTodasLasEmpresas()
        {
            var modelResponse = new ModelResponse();
            try
            {
                var empresa = GetObjects("ObtenerEmpresas", CommandType.StoredProcedure, null,
               new Func<IDataReader, Empresa>((reader) =>
               {
                   var empresas = LlenarEntidad<Empresa>(reader);
                   return empresas;
               }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = empresa;
                modelResponse.Message = "Empresas obtenidas correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las áreas";
            }
            return modelResponse;
        }
        public ModelResponse GuardarOActualizarEmpresas(Empresa e, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(e.NombreComercial)) { throw new ArgumentException("El Nombre es requerido."); }
                if (e.RazonSocial.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (e.RFC.Length > 250) { throw new ArgumentException("El RFC no puede exceder los 250 caracteres."); }
                if (e.Responsable != null && e.Responsable.Length > 500) { throw new ArgumentException("La descripción no puede exceder los 500 caracteres."); }
                if (e.Direccion.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (e.Ciudad.Length > 250) { throw new ArgumentException("El RFC no puede exceder los 250 caracteres."); }
                if (e.Estado.Length > 250) { throw new ArgumentException("El RFC no puede exceder los 250 caracteres."); }
                if (e.CodigoPostal.Length > 250) { throw new ArgumentException("El RFC no puede exceder los 250 caracteres."); }
                if (e.Telefono.Length > 250) { throw new ArgumentException("El RFC no puede exceder los 250 caracteres."); }
                if (e.CorreoContacto != null && e.CorreoContacto.Length > 100) { throw new ArgumentException("El correo no puede exceder los 100 caracteres."); }
                // Ing me falta esto: FechaVigenciaInicio,FechaVigenciaFin,EsPeriodoPrueba
                if (string.IsNullOrWhiteSpace(e.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
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
                    EmpresaId = empresaId
                };
                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var empressaId = ExecuteScalar("GuardarOActualizarEmpresa", CommandType.StoredProcedure, parametros);
                if (Convert.ToInt64(empressaId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }
                e.Id = Convert.ToInt64(empressaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = e;
                modelResponse.Message = "Empresas Guardados Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse GuardarNuevaEmpresa(Empresa e)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(e.NombreComercial)) { throw new ArgumentException("El nombre comercial es requerido."); }
                if (e.NombreComercial.Length > 250) { throw new ArgumentException("El nombre comercial no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(e.RazonSocial)) { throw new ArgumentException("La razón social es requerida."); }
                if (e.RazonSocial.Length > 250) { throw new ArgumentException("La razón social no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(e.RFC)) { throw new ArgumentException("El RFC es requerido."); }
                if (e.RFC.Length > 50) { throw new ArgumentException("El RFC no puede exceder los 50 caracteres."); }
                if (string.IsNullOrWhiteSpace(e.Responsable)) { throw new ArgumentException("El responsable es requerido."); }
                if (e.Responsable.Length > 250) { throw new ArgumentException("El responsable no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(e.Direccion)) { throw new ArgumentException("La dirección es requerida."); }
                if (e.Direccion.Length > 500) { throw new ArgumentException("La dirección no puede exceder los 500 caracteres."); }
                if (e.Ciudad != null && e.Ciudad.Length > 100) { throw new ArgumentException("La ciudad no puede exceder los 100 caracteres."); }
                if (e.Estado != null && e.Estado.Length > 100) { throw new ArgumentException("El estado no puede exceder los 100 caracteres."); }
                if (e.CodigoPostal != null && e.CodigoPostal.Length > 10) { throw new ArgumentException("El código postal no puede exceder los 10 caracteres."); }
                if (e.Telefono != null && e.Telefono.Length > 50) { throw new ArgumentException("El teléfono no puede exceder los 50 caracteres."); }
                if (string.IsNullOrWhiteSpace(e.CorreoContacto)) { throw new ArgumentException("El correo de contacto es requerido."); }
                if (e.CorreoContacto.Length > 250) { throw new ArgumentException("El correo de contacto no puede exceder los 250 caracteres."); }
                if (e.FechaVigenciaInicio == DateTime.MinValue) { throw new ArgumentException("La fecha de vigencia inicio es requerida."); }
                if (e.FechaVigenciaFin == DateTime.MinValue) { throw new ArgumentException("La fecha de vigencia fin es requerida."); }
                if (string.IsNullOrWhiteSpace(e.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al registrar nueva empresa");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al registrar la empresa.";
            }

            return modelResponse;
        }
        public ModelResponse ObtenerEmpresaPorRFC(string rfc)
        {
            var modelResponse = new ModelResponse();
            try
            {

                modelResponse.IsSuccess = true;
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter()
                {
                    Value = rfc,
                    IsNullable = true,
                    ParameterName = "@RFC",
                    SqlDbType = System.Data.SqlDbType.Int
                });

                var result = GetObject("ObtenerEmpresaPorId", System.Data.CommandType.StoredProcedure,
                    parameters, new Func<System.Data.IDataReader, Empresa>((reader) =>
                    {
                        var r = LlenarEntidad<Empresa>(reader);
                        return r;
                    }));
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Empresa Obtenido Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse ObtenerEmpresasPorId(long id, long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la Compania es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                var result = GetObject("ObtenerEmpresaPorId", CommandType.StoredProcedure,
                     new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@EmpresaId", empresaId)
                    },
                    new Func<IDataReader, Empresa>((reader) =>
                    {
                        var r = LlenarEntidad<Empresa>(reader);
                        return r;
                    }));
                if (result == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la Empresa especificada.";
                    return modelResponse;
                }
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Empresa Obtenido Correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la Empresa";
            }

            return modelResponse;
        }
        public ModelResponse EliminarEmpresa(long id, string modificadoPor, DateTime fechaModificacion, long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var result = ExecuteScalar("EliminarEmpresa", CommandType.StoredProcedure, new SqlParameter[]
                {
                   new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@FechaModificacion", fechaModificacion),
            new SqlParameter("@EmpresaId", empresaId)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar la Empresa.";
                    return modelResponse;
                }
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Empresa Eliminado Correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar La Empresa";
            }

            return modelResponse;
        }

        #region Nueva empresa
        public ModelResponse GuardarRolParaNuevaEmpresa(Rol rol)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(rol.Nombre)) { throw new ArgumentException("El nombre del rol es requerido."); }
                if (rol.Nombre.Length > 50) { throw new ArgumentException("El nombre no puede exceder los 50 caracteres."); }
                if (rol.Descripcion != null && rol.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(rol.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

                var parametrosObj = new
                {
                    rol.Nombre,
                    rol.Descripcion,
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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
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
                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (rolId <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
                if (string.IsNullOrWhiteSpace(creadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
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
                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (paginaId <= 0) { throw new ArgumentException("El ID de la página es requerido."); }
                if (string.IsNullOrWhiteSpace(creadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al asignar página {PaginaId} al usuario {UsuarioId} para nueva empresa", paginaId, usuarioId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al asignar la página al usuario.";
            }

            return modelResponse;
        }
        #endregion

    }
}