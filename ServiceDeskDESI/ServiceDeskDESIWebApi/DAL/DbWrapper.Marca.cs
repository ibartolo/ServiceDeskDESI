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
        public ModelResponse ObtenerTodosLasMarcas(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var marcas = GetObjects("ObtenerMarca", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Marca>((reader) =>
                    {
                        var marca = LlenarEntidad<Marca>(reader);
                        Log.Debug("Marca encontrada: Id={Id}, Nombre={Nombre}", marca.Id, marca.Nombre);
                        return marca;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = marcas;
                modelResponse.Message = "Marcas obtenidas correctamente";
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex, "Error de validación al obtener marcas para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener marcas para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las marcas";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerMarcaPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la marca es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var marca = GetObject("ObtenerMarcaPorId", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Marca>((reader) =>
                    {
                        var m = LlenarEntidad<Marca>(reader);
                        return m;
                    }));

                if (marca == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la marca especificada.";
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
                Log.Error(ex, "Error al obtener marca {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la marca";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarMarca(Marca m, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(m.Nombre)) { throw new ArgumentException("El nombre de la marca es requerido."); }
                if (m.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (m.Descripcion != null && m.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(m.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

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
                    Usuario = usuario
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var marcaId = ExecuteScalar("GuardarOActualizarMarca", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(marcaId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                m.Id = Convert.ToInt64(marcaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = m;
                modelResponse.Message = "Marca guardada correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar marca para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar la marca";
            }

            return modelResponse;
        }

        public ModelResponse EliminarMarca(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la marca es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = ExecuteScalar("EliminarMarca", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@FechaModificacion", fechaModificacion),
            new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta marca.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Marca eliminada correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar marca {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la marca";
            }

            return modelResponse;
        }
    }
}