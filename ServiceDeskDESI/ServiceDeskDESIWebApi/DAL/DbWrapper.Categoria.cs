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
        public ModelResponse<List<CategoriaDTO>> ObtenerCategorias(string usuario)
        {
            var modelResponse = new ModelResponse<List<CategoriaDTO>>();

            try
            {
                var categorias = GetObjects("ObtenerCategorias", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, CategoriaDTO>((reader) =>
                    {
                        var categoria = LlenarEntidad<CategoriaDTO>(reader);
                        return categoria;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = categorias.ToList();
                modelResponse.Message = "Categorías obtenidas correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener categorías para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las categorías";
            }

            return modelResponse;
        }

        public ModelResponse<List<CategoriaDTO>> ObtenerCategoriasPorArea(long areaId, string usuario)
        {
            var modelResponse = new ModelResponse<List<CategoriaDTO>>();

            try
            {
                var categorias = GetObjects("ObtenerCategoriasPorArea", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@AreaId", areaId),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, CategoriaDTO>((reader) =>
                    {
                        var categoria = LlenarEntidad<CategoriaDTO>(reader);
                        return categoria;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = categorias.ToList();
                modelResponse.Message = "Categorías por área obtenidas correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener categorías por área {AreaId} para usuario {Usuario}", areaId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las categorías por área";
            }

            return modelResponse;
        }

        public ModelResponse<CategoriaDTO> ObtenerCategoriaPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse<CategoriaDTO>();

            try
            {
                var categoria = GetObject("ObtenerCategoriaPorId", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, CategoriaDTO>((reader) =>
                    {
                        var c = LlenarEntidad<CategoriaDTO>(reader);
                        return c;
                    }));

                if (categoria == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la categoría especificada.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = categoria;
                modelResponse.Message = "Categoría obtenida correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener categoría {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la categoría";
            }

            return modelResponse;
        }

        public ModelResponse<List<CategoriaDTO>> ObtenerCategoriasPorPadre(long categoriaPadreId, string usuario)
        {
            var modelResponse = new ModelResponse<List<CategoriaDTO>>();

            try
            {
                var categorias = GetObjects("ObtenerCategoriasPorPadre", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@CategoriaPadreId", categoriaPadreId),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, CategoriaDTO>((reader) =>
                    {
                        var c = LlenarEntidad<CategoriaDTO>(reader);
                        return c;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = categorias.ToList();
                modelResponse.Message = "Subcategorías obtenidas correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener subcategorías para categoría padre {CategoriaPadreId} para usuario {Usuario}", categoriaPadreId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las subcategorías";
            }

            return modelResponse;
        }

        public ModelResponse<Categoria> GuardarOActualizarCategoria(Categoria c, string usuario)
        {
            var modelResponse = new ModelResponse<Categoria>();

            try
            {
                var parametrosObj = new
                {
                    c.Id,
                    c.Nombre,
                    c.Descripcion,
                    c.CategoriaPadreId,
                    c.AreaId,
                    c.Orden,
                    c.CreadoPor,
                    c.FechaCreacion,
                    c.ModificadoPor,
                    c.FechaModificacion,
                    c.Estatus,
                    Usuario = usuario
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var categoriaId = ExecuteScalar("GuardarOActualizarCategoria", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(categoriaId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                c.Id = Convert.ToInt64(categoriaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = c;
                modelResponse.Message = "Categoría guardada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar categoría para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar la categoría";
            }

            return modelResponse;
        }

        public ModelResponse EliminarCategoria(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = ExecuteScalar("EliminarCategoria", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion),
                    new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta categoría.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Categoría eliminada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar categoría {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la categoría";
            }

            return modelResponse;
        }
    }
}
