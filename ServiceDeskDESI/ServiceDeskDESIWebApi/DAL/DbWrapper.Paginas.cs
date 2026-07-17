using Serilog;
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
        public ModelResponse ObtenerPaginasPorUsuario(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener páginas para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las páginas";
            }

            return modelResponse;
        }

        public ModelResponse ValidarAccesoPagina(string usuario, string direccion)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(direccion)) { throw new ArgumentException("La dirección de la página es requerida."); }

                var resultado = ExecuteScalar("ValidarAccesoPagina", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Usuario", usuario),
            new SqlParameter("@Direccion", direccion)
                });

                var tieneAcceso = Convert.ToInt32(resultado) == 1;

                modelResponse.IsSuccess = true;
                modelResponse.Response = tieneAcceso;
                modelResponse.Message = tieneAcceso ? "Acceso permitido" : "Acceso denegado";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al validar acceso para usuario {Usuario} a {Direccion}", usuario, direccion);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al validar el acceso";
            }

            return modelResponse;
        }

        public ModelResponse InsertarUsuarioPagina(long usuarioId, long paginaId, string creadoPor)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (paginaId <= 0) { throw new ArgumentException("El ID de la página es requerido."); }
                if (string.IsNullOrWhiteSpace(creadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

                var resultado = ExecuteScalar("InsertarUsuarioPagina", CommandType.StoredProcedure, new SqlParameter[]
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
                modelResponse.Message = "Página asignada al usuario correctamente.";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al asignar página {PaginaId} al usuario {UsuarioId}", paginaId, usuarioId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al asignar la página al usuario.";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerPaginaPorNombre(string nombre)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El nombre de la página es requerido.";
                    return modelResponse;
                }

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
    }
}