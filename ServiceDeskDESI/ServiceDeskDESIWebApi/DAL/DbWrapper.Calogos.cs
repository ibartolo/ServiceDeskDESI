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
        public ModelResponse ObtenerAreas()
        {
            var modelResponse = new ModelResponse();

            try
            {
                var areas = GetObjects("ObtenerAreas", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                    new Func<IDataReader, Area>((reader) =>
                    {
                        var area = LlenarEntidad<Area>(reader);
                        return area;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = areas;
                modelResponse.Message = "Áreas obtenidas correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }

        public ModelResponse ObtenerAreaPorId(long id)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var area = GetObject("ObtenerAreaPorId", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@Id", id)
                    },
                    new Func<IDataReader, Area>((reader) =>
                    {
                        var a = LlenarEntidad<Area>(reader);
                        return a;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = area;
                modelResponse.Message = "Área obtenida correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarArea(Area a)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(a.Nombre)) { throw new ArgumentException("El nombre del área es requerido."); }
                if (a.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }

                if (string.IsNullOrWhiteSpace(a.Descripcion)) { throw new ArgumentException("La descripción del área es requerida."); }
                if (a.Descripcion.Length > 500) { throw new ArgumentException("La descripción no puede exceder los 500 caracteres."); }

                if (string.IsNullOrWhiteSpace(a.Correo)) { throw new ArgumentException("El correo del área es requerido."); }
                if (a.Correo.Length > 100) { throw new ArgumentException("El correo no puede exceder los 100 caracteres."); }

                if (!System.Text.RegularExpressions.Regex.IsMatch(a.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                { throw new ArgumentException("El formato del correo no es válido."); }

                var parametros = ObtenerParametrosSQL(a).ToArray();
                var areaId = ExecuteScalar("GuardarOActualizarArea", CommandType.StoredProcedure, parametros);
                a.Id = Convert.ToInt64(areaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = a;
                modelResponse.Message = "Área guardada correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error inesperado, por favor contacte al administrador.";
            }

            return modelResponse;
        }

        public ModelResponse EliminarArea(Area a)
        {
            var modelResponse = new ModelResponse();

            try
            {
                ExecuteNonQuery("EliminarArea", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Id", a.Id),
            new SqlParameter("@ModificadoPor", a.ModificadoPor),
            new SqlParameter("@FechaModificacion", a.FechaModificacion)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Área eliminada correctamente";
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