using Serilog;
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
        public ModelResponse ObtenerPaginasPorUsuario(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var paginas = GetObjects("ObtenerPaginasPorUsuario", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Pagina>((reader) =>
                    {
                        var pagina = LlenarEntidad<Pagina>(reader);
                        return pagina;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = paginas;
                modelResponse.Message = "Páginas obtenidas correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener páginas para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las páginas";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerPaginaPorNombre(string nombre)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var pagina = GetObject("ObtenerPaginaPorNombre", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Nombre", nombre) },
                    new Func<IDataReader, Pagina>((reader) =>
                    {
                        var p = LlenarEntidad<Pagina>(reader);
                        return p;
                    }));

                if (pagina == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la página especificada.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = pagina;
                modelResponse.Message = "Página obtenida correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener página por nombre {Nombre}", nombre);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la página";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerPaginas()
        {
            var modelResponse = new ModelResponse();

            try
            {
                var paginas = GetObjects("ObtenerPaginas", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                    new Func<IDataReader, Pagina>((reader) =>
                    {
                        var pagina = LlenarEntidad<Pagina>(reader);
                        return pagina;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = paginas;
                modelResponse.Message = "Páginas obtenidas correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener páginas");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las páginas.";
            }

            return modelResponse;
        }
    }
}