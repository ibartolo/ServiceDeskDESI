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
        public ModelResponse ObtenerCategoriasPorArea(long areaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (areaId <= 0) { throw new ArgumentException("El ID del área es requerido."); }

                var categorias = GetObjects("ObtenerCategoriasPorArea", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@AreaId", areaId) },
                    new Func<IDataReader, Categoria>((reader) =>
                    {
                        var categoria = LlenarEntidad<Categoria>(reader);

                        categoria.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        if (reader["CategoriaPadreId"] != DBNull.Value)
                        {
                            categoria.CategoriaPadre = new Categoria()
                            {
                                Id = MapearPorpiedades<long>(reader["CategoriaPadreId"]),
                                Nombre = MapearPorpiedades<string>(reader["CategoriaPadreNombre"])
                            };
                        }

                        return categoria;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = categorias;
                modelResponse.Message = "Categorías obtenidas correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las categorías";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerCategoriaPorId(long id)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la categoría es requerido."); }

                var categoria = GetObject("ObtenerCategoriaPorId", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) },
                    new Func<IDataReader, Categoria>((reader) =>
                    {
                        var c = LlenarEntidad<Categoria>(reader);

                        c.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"])
                        };

                        if (reader["CategoriaPadreId"] != DBNull.Value)
                        {
                            c.CategoriaPadre = new Categoria()
                            {
                                Id = MapearPorpiedades<long>(reader["CategoriaPadreId"])
                            };
                        }

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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la categoría";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerCategoriasPorPadre(long categoriaPadreId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (categoriaPadreId <= 0) { throw new ArgumentException("El ID de la categoría padre es requerido."); }

                var categorias = GetObjects("ObtenerCategoriasPorPadre", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@CategoriaPadreId", categoriaPadreId) },
                    new Func<IDataReader, Categoria>((reader) =>
                    {
                        var c = LlenarEntidad<Categoria>(reader);

                        c.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"])
                        };

                        return c;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = categorias;
                modelResponse.Message = "Subcategorías obtenidas correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las subcategorías";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarCategoria(Categoria c)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(c.Nombre)) { throw new ArgumentException("El nombre de la categoría es requerido."); }
                if (c.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (c.Area == null || c.Area.Id <= 0) { throw new ArgumentException("El área es requerida."); }
                if (c.CategoriaPadre != null && c.CategoriaPadre.Id == c.Id) { throw new ArgumentException("La categoría no puede ser padre de sí misma."); }
                if (string.IsNullOrWhiteSpace(c.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

                // Construir objeto con IDs
                var categoriaParaSP = new
                {
                    c.Id,
                    c.Nombre,
                    c.Descripcion,
                    CategoriaPadreId = c.CategoriaPadre?.Id,
                    AreaId = c.Area.Id,
                    c.Orden,
                    c.CreadoPor,
                    c.FechaCreacion,
                    c.ModificadoPor,
                    c.FechaModificacion,
                    c.Estatus
                };

                var parametros = ObtenerParametrosSQL(categoriaParaSP).ToArray();
                var categoriaId = ExecuteScalar("GuardarOActualizarCategoria", CommandType.StoredProcedure, parametros);
                c.Id = Convert.ToInt64(categoriaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = c;
                modelResponse.Message = "Categoría guardada correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar la categoría";
            }

            return modelResponse;
        }

        public ModelResponse EliminarCategoria(long id, string modificadoPor, DateTime fechaModificacion)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la categoría es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }

                // Verificar si tiene subcategorías
                var subcategorias = ObtenerCategoriasPorPadre(id);
                if (subcategorias.IsSuccess && subcategorias.Response != null)
                {
                    var lista = subcategorias.Response as IEnumerable<Categoria>;
                    if (lista != null && lista.Any())
                    {
                        modelResponse.IsSuccess = false;
                        modelResponse.Message = "No se puede eliminar la categoría porque tiene subcategorías asociadas.";
                        return modelResponse;
                    }
                }

                ExecuteNonQuery("EliminarCategoria", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@FechaModificacion", fechaModificacion)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Categoría eliminada correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la categoría";
            }

            return modelResponse;
        }
    }
}