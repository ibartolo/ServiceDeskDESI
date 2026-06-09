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
        public ModelResponse ObtenerModelos(long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var modelos = GetObjects("ObtenerModelos", CommandType.StoredProcedure,
                     new[] { new SqlParameter("@EmpresaId", empresaId) },
                    new Func<IDataReader, Modelo>((reader) =>
                    {
                        var modelo = LlenarEntidad<Modelo>(reader);

                        modelo.Marca = new Marca()
                        {
                            Id = MapearPorpiedades<long>(reader["MarcaId"]),
                            Nombre = MapearPorpiedades<string>(reader["MarcaNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["MarcaDescripcion"])
                        };

                        return modelo;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = modelos;
                modelResponse.Message = "Modelos obtenidos correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los Modelos";
            }


            return modelResponse;
        }

        public ModelResponse GuardarOActualizarModelo(Modelo m, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(m.Nombre)) { throw new ArgumentException("El nombre del modelo es requerido."); }
                if (m.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                // if (string.IsNullOrWhiteSpace(m.Descripcion)) { throw new ArgumentException("La descripción del modelo es requerida."); }
                if (m.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                if (m.Marca == null || m.Marca.Id <= 0) { throw new ArgumentException("La marca es requerida."); }
                if (string.IsNullOrWhiteSpace(m.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var parametrosObj = new
                {
                    m.Id,
                    m.Nombre,
                    m.Descripcion,
                    m.Marca,
                    m.CreadoPor,
                    m.FechaCreacion,
                    m.ModificadoPor,
                    m.FechaModificacion,
                    m.Estatus,
                    EmpresaId = empresaId


                };
                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var modeloId = ExecuteScalar("GuardarOActualizarModelo", CommandType.StoredProcedure, parametros);
                if ( Convert.ToInt64(modeloId)==0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }
                m.Id = Convert.ToInt64(modeloId);
                modelResponse.IsSuccess = true;
                modelResponse.Response = m;
                modelResponse.Message = "Modelo guardado correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el modelo";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerModeloPorId(long id, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {

                if (id <= 0) { throw new ArgumentException("El ID del modelo es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var modelo = GetObject("ObtenerModeloPorId", CommandType.StoredProcedure,
                  new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@EmpresaId", empresaId)
                    },
                    new Func<IDataReader, Modelo>((reader) =>
                    {
                        var m = LlenarEntidad<Modelo>(reader);

                        m.Marca = new Marca()
                        {
                            Id = MapearPorpiedades<long>(reader["MarcaId"]),
                            Nombre = MapearPorpiedades<string>(reader["MarcaNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["MarcaDescripcion"])
                        };

                        return m;
                    }));

                if (modelo == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el modelo especificado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = modelo;
                modelResponse.Message = "Modelo obtenido correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el modelo";
            }

            return modelResponse;
        }
        public ModelResponse EliminarModelo(long id, string modificadoPor, DateTime fechaModificacion, long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del Modelo es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var result = ExecuteScalar("EliminarModelo", CommandType.StoredProcedure,new SqlParameter[]
                {
                   new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@FechaModificacion", fechaModificacion),
            new SqlParameter("@EmpresaId", empresaId)
                });
                if (Convert.ToInt64(result)== 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar este Modelo.";
                    return modelResponse;
                }
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Marca eliminado correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la Marca";
            }
            return modelResponse;
        }
    }
}