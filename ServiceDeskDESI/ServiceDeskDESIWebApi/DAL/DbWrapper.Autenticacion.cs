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
                modelResponse.Message = "Usuarios obtenidos correctamente";
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
                modelResponse.Message = "Usuario guardado correctamente";
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
                modelResponse.Message = "Usuario eliminado correctamente";
                modelResponse.Response = null;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
    }
}