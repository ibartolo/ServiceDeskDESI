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
        public ModelResponse ObtenerTodasLasMarcas(long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                var marcas = GetObjects("ObtenerMarca", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@EmpresaId", empresaId) },
                    new Func<IDataReader, Marca>((reader) =>
                    {
                        var marca = LlenarEntidad<Marca>(reader);
                        return marca;
                    }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = marcas;
                modelResponse.Message = "Marcas Obtenidos Correctamente";

            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las Marcas";
            }
            return modelResponse;
        }
        public ModelResponse GuardarOActualizarMarca (Marca m, long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(m.Nombre)) { throw new ArgumentException("El nombre de la Marca es requerido."); }
                if (m.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (m.Descripcion != null && m.Descripcion.Length > 500) { throw new ArgumentException("La descripción no puede exceder los 500 caracteres."); }
                if (string.IsNullOrWhiteSpace(m.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                var parametrosObj = new
                {
                    m.Id,
                    m.Nombre,
                    m.Descripcion,
                    m.CreadoPor,
                    m.FechaCreacion,
                    m.ModificadoPor,
                    m.FechaModificacion,
                    m.Estatus,
                    EmpresaId = empresaId
                };
                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var marcaId = ExecuteScalar("GuardarOActualizarMarca", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(marcaId)==0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }
                m.Id = Convert.ToInt64(marcaId);
                modelResponse.IsSuccess = true;
                modelResponse.Response = m;
                modelResponse.Message = "Marcas Guardados Correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar la Marca";
            }

            return modelResponse;
        }
        public ModelResponse ObtenerMarcasPorId(long id, long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la Marca es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                var marca = GetObject("ObtenerMarcaPorId", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@EmpresaId", empresaId)
                    },
                    new Func<IDataReader, Marca>((reader) =>
                    {
                        var a = LlenarEntidad<Marca>(reader);
                        return a;
                    }));
                if (marca == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la Marca especificada.";
                    return modelResponse;
                }
                modelResponse.IsSuccess = true;
                modelResponse.Response = marca;
                modelResponse.Message = "Marca obtenida correctamente";
            }

            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener Marca";
            }


            return modelResponse;
        }
        public ModelResponse EliminarMarca (long id, string modificadoPor, DateTime fechaModificacion, long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var result = ExecuteScalar("EliminarMarca", CommandType.StoredProcedure,new SqlParameter[]
                {
                      new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@FechaModificacion", fechaModificacion),
            new SqlParameter("@EmpresaId", empresaId)
                });
                if (Convert.ToInt64(result)==0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta Marca";
                    return modelResponse;
                }
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Marca Eliminado Correctamente";
                
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