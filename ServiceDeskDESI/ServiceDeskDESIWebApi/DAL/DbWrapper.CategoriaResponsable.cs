using Serilog;
using ServiceDeskDESIEntities.Autenticacion;
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
        public ModelResponse<List<CategoriaResponsableDTO>> ObtenerResponsablesPorCategoria(long categoriaId, string usuario)
        {
            var modelResponse = new ModelResponse<List<CategoriaResponsableDTO>>();

            try
            {
                var responsables = GetObjects("ObtenerResponsablesPorCategoria", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@CategoriaId", categoriaId),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, CategoriaResponsableDTO>((reader) =>
                    {
                        var cr = LlenarEntidad<CategoriaResponsableDTO>(reader);
                        return cr;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = responsables.ToList();
                modelResponse.Message = "Responsables obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener responsables para categoría {CategoriaId} y usuario {Usuario}", categoriaId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los responsables";
            }

            return modelResponse;
        }

        public ModelResponse<List<CategoriaResponsableDTO>> ObtenerCategoriasPorResponsable(long usuarioId, string usuario)
        {
            var modelResponse = new ModelResponse<List<CategoriaResponsableDTO>>();

            try
            {
                var categorias = GetObjects("ObtenerCategoriasPorResponsable", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@UsuarioId", usuarioId),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, CategoriaResponsableDTO>((reader) =>
                    {
                        var cr = LlenarEntidad<CategoriaResponsableDTO>(reader);
                        return cr;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = categorias.ToList();
                modelResponse.Message = "Categorías por responsable obtenidas correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener categorías para responsable {UsuarioId} y usuario {Usuario}", usuarioId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las categorías";
            }

            return modelResponse;
        }

        public ModelResponse<CategoriaResponsable> GuardarOActualizarCategoriaResponsable(CategoriaResponsable cr, string usuario)
        {
            var modelResponse = new ModelResponse<CategoriaResponsable>();

            try
            {
                var parametrosObj = new
                {
                    cr.Id,
                    cr.CategoriaId,
                    cr.UsuarioId,
                    cr.EsPrincipal,
                    cr.CreadoPor,
                    cr.FechaCreacion,
                    cr.ModificadoPor,
                    cr.FechaModificacion,
                    cr.Estatus,
                    Usuario = usuario
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var resultado = ExecuteScalar("GuardarOActualizarCategoriaResponsable", CommandType.StoredProcedure, parametros);
                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                if (resultadoLong == -1)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El usuario seleccionado no puede atender tickets.";
                    return modelResponse;
                }

                cr.Id = resultadoLong;

                modelResponse.IsSuccess = true;
                modelResponse.Response = cr;
                modelResponse.Message = "Responsable guardado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar responsable de categoría para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el responsable";
            }

            return modelResponse;
        }

        public ModelResponse EliminarCategoriaResponsable(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("EliminarCategoriaResponsable", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion),
                    new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(resultado) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar este responsable.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Responsable eliminado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar responsable de categoría {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el responsable";
            }

            return modelResponse;
        }

        public ModelResponse<List<CategoriaResponsableDTO>> ObtenerTodosLosResponsables(string usuario)
        {
            var modelResponse = new ModelResponse<List<CategoriaResponsableDTO>>();

            try
            {
                var responsables = GetObjects("ObtenerTodosLosResponsables", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, CategoriaResponsableDTO>((reader) =>
                    {
                        var cr = LlenarEntidad<CategoriaResponsableDTO>(reader);
                        return cr;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = responsables.ToList();
                modelResponse.Message = "Responsables obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener todos los responsables para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los responsables";
            }

            return modelResponse;
        }
    }
}
