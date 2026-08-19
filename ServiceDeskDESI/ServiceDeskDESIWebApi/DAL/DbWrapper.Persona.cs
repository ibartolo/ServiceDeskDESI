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
        public ModelResponse<List<PersonaDTO>> ObtenerTodasLasPersonas(string usuario)
        {
            var modelResponse = new ModelResponse<List<PersonaDTO>>();

            try
            {
                var personas = GetObjects("ObtenerPersonas", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, PersonaDTO>((reader) =>
                    {
                        var persona = LlenarEntidad<PersonaDTO>(reader);
                        return persona;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = personas.ToList();
                modelResponse.Message = "Personas obtenidas correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener personas para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las personas";
            }

            return modelResponse;
        }

        public ModelResponse<PersonaDTO> ObtenerPersonaPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse<PersonaDTO>();

            try
            {
                var persona = GetObject("ObtenerPersonaPorId", CommandType.StoredProcedure,
                    new[] {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, PersonaDTO>((reader) =>
                    {
                        var p = LlenarEntidad<PersonaDTO>(reader);
                        return p;
                    }));

                if (persona == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la persona especificada.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = persona;
                modelResponse.Message = "Persona obtenida correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener persona {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la persona";
            }

            return modelResponse;
        }

        public ModelResponse<Persona> GuardarOActualizarPersona(Persona p, string usuario)
        {
            var modelResponse = new ModelResponse<Persona>();

            try
            {
                var parametrosObj = new
                {
                    p.Id,
                    p.Nombre,
                    p.Apellido,
                    p.Correo,
                    p.Telefono,
                    p.PuestoId,
                    p.CreadoPor,
                    p.FechaCreacion,
                    p.ModificadoPor,
                    p.FechaModificacion,
                    p.Estatus,
                    Usuario = usuario
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var personaId = ExecuteScalar("GuardarOActualizarPersona", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(personaId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                p.Id = Convert.ToInt64(personaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = p;
                modelResponse.Message = "Persona guardada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar persona para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar la persona";
            }

            return modelResponse;
        }

        public ModelResponse EliminarPersona(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = ExecuteScalar("EliminarPersona", CommandType.StoredProcedure, new SqlParameter[]
                {
                new SqlParameter("@Id", id),
                new SqlParameter("@ModificadoPor", modificadoPor),
                new SqlParameter("@FechaModificacion", fechaModificacion),
                new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta persona.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Persona eliminada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar persona {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la persona";
            }

            return modelResponse;
        }
    }
}
