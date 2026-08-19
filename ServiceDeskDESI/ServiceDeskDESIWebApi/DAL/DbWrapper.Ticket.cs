using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Serilog;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper
    {
        public ModelResponse<List<TicketDTO>> ObtenerTickets(string usuario)
        {
            var modelResponse = new ModelResponse<List<TicketDTO>>();

            try
            {
                var tickets = GetObjects("ObtenerTickets", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, TicketDTO>((reader) =>
                    {
                        var ticket = LlenarEntidad<TicketDTO>(reader);
                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets.ToList();
                modelResponse.Message = "Tickets obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tickets para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los tickets";
            }

            return modelResponse;
        }

        public ModelResponse<TicketDTO> ObtenerTicketPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse<TicketDTO>();

            try
            {
                var ticket = GetObject("ObtenerTicketPorId", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, TicketDTO>((reader) =>
                    {
                        var t = LlenarEntidad<TicketDTO>(reader);
                        return t;
                    }));

                if (ticket == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el ticket especificado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = ticket;
                modelResponse.Message = "Ticket obtenido correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener ticket {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el ticket";
            }

            return modelResponse;
        }

        public ModelResponse<Ticket> GuardarOActualizarTicket(Ticket t, string usuario)
        {
            var modelResponse = new ModelResponse<Ticket>();

            try
            {
                var parametrosObj = new
                {
                    t.Id,
                    t.AreaId,
                    t.CategoriaId,
                    t.SubcategoriaId,
                    t.Urgencia,
                    t.Titulo,
                    t.Descripcion,
                    t.TicketEstatusId,
                    t.CreadoPor,
                    t.FechaCreacion,
                    t.ModificadoPor,
                    t.FechaModificacion,
                    t.Estatus,
                    Usuario = usuario
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var ticketId = ExecuteScalar("GuardarOActualizarTicket", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(ticketId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                t.Id = Convert.ToInt64(ticketId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = t;
                modelResponse.Message = "Ticket guardado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar ticket para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el ticket";
            }

            return modelResponse;
        }

        public ModelResponse EliminarTicket(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = ExecuteScalar("EliminarTicket", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion),
                    new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar este ticket.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Ticket eliminado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar ticket {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el ticket";
            }

            return modelResponse;
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorArea(long areaId, string usuario)
        {
            var modelResponse = new ModelResponse<List<TicketDTO>>();

            try
            {
                var tickets = GetObjects("ObtenerTicketsPorArea", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@AreaId", areaId),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, TicketDTO>((reader) =>
                    {
                        var ticket = LlenarEntidad<TicketDTO>(reader);
                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets.ToList();
                modelResponse.Message = "Tickets por área obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tickets por área {AreaId} para usuario {Usuario}", areaId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los tickets por área";
            }

            return modelResponse;
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorUsuario(string creadoPor, string usuario)
        {
            var modelResponse = new ModelResponse<List<TicketDTO>>();

            try
            {
                var tickets = GetObjects("ObtenerTicketsPorUsuario", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@CreadoPor", creadoPor),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, TicketDTO>((reader) =>
                    {
                        var ticket = LlenarEntidad<TicketDTO>(reader);
                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets.ToList();
                modelResponse.Message = "Tickets por usuario obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tickets por usuario {CreadoPor} para usuario {Usuario}", creadoPor, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los tickets por usuario";
            }

            return modelResponse;
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorUrgencia(int urgencia, string usuario)
        {
            var modelResponse = new ModelResponse<List<TicketDTO>>();

            try
            {
                var tickets = GetObjects("ObtenerTicketsPorUrgencia", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Urgencia", urgencia),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, TicketDTO>((reader) =>
                    {
                        var ticket = LlenarEntidad<TicketDTO>(reader);
                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets.ToList();
                modelResponse.Message = "Tickets por urgencia obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tickets por urgencia {Urgencia} para usuario {Usuario}", urgencia, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los tickets por urgencia";
            }

            return modelResponse;
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorEstatus(int ticketEstatusId, string usuario)
        {
            var modelResponse = new ModelResponse<List<TicketDTO>>();

            try
            {
                var tickets = GetObjects("ObtenerTicketsPorEstatus", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@TicketEstatusId", ticketEstatusId),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, TicketDTO>((reader) =>
                    {
                        var ticket = LlenarEntidad<TicketDTO>(reader);
                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets.ToList();
                modelResponse.Message = "Tickets por estatus obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tickets por estatus {TicketEstatusId} para usuario {Usuario}", ticketEstatusId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los tickets por estatus";
            }

            return modelResponse;
        }
        public ModelResponse<List<TicketEstatus>> ObtenerTicketEstatus()
        {
            var modelResponse = new ModelResponse<List<TicketEstatus>>();

            try
            {
                var estatus = GetObjects("ObtenerTicketEstatus", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                    new Func<IDataReader, TicketEstatus>((reader) =>
                    {
                        var e = LlenarEntidad<TicketEstatus>(reader);
                        return e;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = estatus.ToList();
                modelResponse.Message = "Estatus obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener estatus de tickets");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los estatus de tickets";
            }

            return modelResponse;
        }

        public ModelResponse TomarTicket(long ticketId, string usuario, string comentario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("TomarTicket", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@TicketId", ticketId),
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Comentario", (object)comentario ?? DBNull.Value)
                });

                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong <= 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se pudo tomar el ticket. Verifique que sea un agente, que el ticket sea de su área y que esté disponible.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = resultadoLong;
                modelResponse.Message = "Ticket tomado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al tomar ticket {TicketId} para usuario {Usuario}", ticketId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al tomar el ticket.";
            }

            return modelResponse;
        }

        public ModelResponse ReasignarTicket(long ticketId, long nuevoUsuarioId, string usuario, string comentario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("ReasignarTicket", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@TicketId", ticketId),
                    new SqlParameter("@NuevoUsuarioId", nuevoUsuarioId),
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Comentario", (object)comentario ?? DBNull.Value)
                });

                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong <= 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se pudo reasignar el ticket. Solo el responsable del área puede reasignar.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = resultadoLong;
                modelResponse.Message = "Ticket reasignado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al reasignar ticket {TicketId} para usuario {Usuario}", ticketId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al reasignar el ticket.";
            }

            return modelResponse;
        }

        public ModelResponse<List<TicketAsignacionDTO>> ObtenerTicketAsignaciones(long ticketId)
        {
            var modelResponse = new ModelResponse<List<TicketAsignacionDTO>>();

            try
            {
                var asignaciones = GetObjects("ObtenerTicketAsignaciones", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@TicketId", ticketId) },
                    new Func<IDataReader, TicketAsignacionDTO>((reader) =>
                    {
                        var ta = LlenarEntidad<TicketAsignacionDTO>(reader);
                        return ta;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = asignaciones.ToList();
                modelResponse.Message = "Asignaciones obtenidas correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener asignaciones del ticket {TicketId}", ticketId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las asignaciones del ticket.";
            }

            return modelResponse;
        }
    }
}
