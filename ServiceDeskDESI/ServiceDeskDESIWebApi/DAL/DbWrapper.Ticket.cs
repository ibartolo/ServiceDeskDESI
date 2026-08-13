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
        public ModelResponse ObtenerTickets(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var tickets = GetObjects("ObtenerTickets", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Ticket>((reader) =>
                    {
                        var ticket = LlenarEntidad<Ticket>(reader);

                        ticket.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        ticket.Categoria = new Categoria()
                        {
                            Id = MapearPorpiedades<long>(reader["CategoriaId"]),
                            Nombre = MapearPorpiedades<string>(reader["CategoriaNombre"])
                        };

                        if (reader["SubcategoriaId"] != DBNull.Value)
                        {
                            ticket.Subcategoria = new Categoria()
                            {
                                Id = MapearPorpiedades<long>(reader["SubcategoriaId"]),
                                Nombre = MapearPorpiedades<string>(reader["SubcategoriaNombre"])
                            };
                        }

                        ticket.TicketEstatus = new TicketEstatus()
                        {
                            Id = MapearPorpiedades<int>(reader["TicketEstatusId"]),
                            Nombre = MapearPorpiedades<string>(reader["EstatusNombre"]),
                            Color = MapearPorpiedades<string>(reader["EstatusColor"])
                        };

                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets;
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

        public ModelResponse ObtenerTicketPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var ticket = GetObject("ObtenerTicketPorId", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Ticket>((reader) =>
                    {
                        var t = LlenarEntidad<Ticket>(reader);

                        t.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        t.Categoria = new Categoria()
                        {
                            Id = MapearPorpiedades<long>(reader["CategoriaId"]),
                            Nombre = MapearPorpiedades<string>(reader["CategoriaNombre"])
                        };

                        if (reader["SubcategoriaId"] != DBNull.Value)
                        {
                            t.Subcategoria = new Categoria()
                            {
                                Id = MapearPorpiedades<long>(reader["SubcategoriaId"]),
                                Nombre = MapearPorpiedades<string>(reader["SubcategoriaNombre"])
                            };
                        }

                        t.TicketEstatus = new TicketEstatus()
                        {
                            Id = MapearPorpiedades<int>(reader["TicketEstatusId"]),
                            Nombre = MapearPorpiedades<string>(reader["EstatusNombre"]),
                            Color = MapearPorpiedades<string>(reader["EstatusColor"])
                        };

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

        public ModelResponse GuardarOActualizarTicket(Ticket t, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var parametrosObj = new
                {
                    t.Id,
                    Area = t.Area.Id,
                    Categoria = t.Categoria.Id,
                    Subcategoria = t.Subcategoria?.Id,
                    t.Urgencia,
                    t.Titulo,
                    t.Descripcion,
                    TicketEstatusId = t.TicketEstatus.Id,
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
                ExecuteNonQuery("EliminarTicket", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion),
                    new SqlParameter("@Usuario", usuario)
                });

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

        public ModelResponse ObtenerTicketsPorArea(long areaId, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var tickets = GetObjects("ObtenerTicketsPorArea", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@AreaId", areaId),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Ticket>((reader) =>
                    {
                        var ticket = LlenarEntidad<Ticket>(reader);

                        ticket.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        ticket.Categoria = new Categoria()
                        {
                            Id = MapearPorpiedades<long>(reader["CategoriaId"]),
                            Nombre = MapearPorpiedades<string>(reader["CategoriaNombre"])
                        };

                        if (reader["SubcategoriaId"] != DBNull.Value)
                        {
                            ticket.Subcategoria = new Categoria()
                            {
                                Id = MapearPorpiedades<long>(reader["SubcategoriaId"]),
                                Nombre = MapearPorpiedades<string>(reader["SubcategoriaNombre"])
                            };
                        }

                        ticket.TicketEstatus = new TicketEstatus()
                        {
                            Id = MapearPorpiedades<int>(reader["TicketEstatusId"]),
                            Nombre = MapearPorpiedades<string>(reader["EstatusNombre"]),
                            Color = MapearPorpiedades<string>(reader["EstatusColor"])
                        };

                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets;
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

        public ModelResponse ObtenerTicketsPorUsuario(string creadoPor, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var tickets = GetObjects("ObtenerTicketsPorUsuario", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@CreadoPor", creadoPor),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Ticket>((reader) =>
                    {
                        var ticket = LlenarEntidad<Ticket>(reader);

                        ticket.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        ticket.Categoria = new Categoria()
                        {
                            Id = MapearPorpiedades<long>(reader["CategoriaId"]),
                            Nombre = MapearPorpiedades<string>(reader["CategoriaNombre"])
                        };

                        if (reader["SubcategoriaId"] != DBNull.Value)
                        {
                            ticket.Subcategoria = new Categoria()
                            {
                                Id = MapearPorpiedades<long>(reader["SubcategoriaId"]),
                                Nombre = MapearPorpiedades<string>(reader["SubcategoriaNombre"])
                            };
                        }

                        ticket.TicketEstatus = new TicketEstatus()
                        {
                            Id = MapearPorpiedades<int>(reader["TicketEstatusId"]),
                            Nombre = MapearPorpiedades<string>(reader["EstatusNombre"]),
                            Color = MapearPorpiedades<string>(reader["EstatusColor"])
                        };

                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets;
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

        public ModelResponse ObtenerTicketsPorUrgencia(int urgencia, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var tickets = GetObjects("ObtenerTicketsPorUrgencia", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Urgencia", urgencia),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Ticket>((reader) =>
                    {
                        var ticket = LlenarEntidad<Ticket>(reader);

                        ticket.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        ticket.Categoria = new Categoria()
                        {
                            Id = MapearPorpiedades<long>(reader["CategoriaId"]),
                            Nombre = MapearPorpiedades<string>(reader["CategoriaNombre"])
                        };

                        if (reader["SubcategoriaId"] != DBNull.Value)
                        {
                            ticket.Subcategoria = new Categoria()
                            {
                                Id = MapearPorpiedades<long>(reader["SubcategoriaId"]),
                                Nombre = MapearPorpiedades<string>(reader["SubcategoriaNombre"])
                            };
                        }

                        ticket.TicketEstatus = new TicketEstatus()
                        {
                            Id = MapearPorpiedades<int>(reader["TicketEstatusId"]),
                            Nombre = MapearPorpiedades<string>(reader["EstatusNombre"]),
                            Color = MapearPorpiedades<string>(reader["EstatusColor"])
                        };

                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets;
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

        public ModelResponse ObtenerTicketsPorEstatus(int ticketEstatusId, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var tickets = GetObjects("ObtenerTicketsPorEstatus", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@TicketEstatusId", ticketEstatusId),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Ticket>((reader) =>
                    {
                        var ticket = LlenarEntidad<Ticket>(reader);

                        ticket.Area = new Area()
                        {
                            Id = MapearPorpiedades<long>(reader["AreaId"]),
                            Nombre = MapearPorpiedades<string>(reader["AreaNombre"])
                        };

                        ticket.Categoria = new Categoria()
                        {
                            Id = MapearPorpiedades<long>(reader["CategoriaId"]),
                            Nombre = MapearPorpiedades<string>(reader["CategoriaNombre"])
                        };

                        if (reader["SubcategoriaId"] != DBNull.Value)
                        {
                            ticket.Subcategoria = new Categoria()
                            {
                                Id = MapearPorpiedades<long>(reader["SubcategoriaId"]),
                                Nombre = MapearPorpiedades<string>(reader["SubcategoriaNombre"])
                            };
                        }

                        ticket.TicketEstatus = new TicketEstatus()
                        {
                            Id = MapearPorpiedades<int>(reader["TicketEstatusId"]),
                            Nombre = MapearPorpiedades<string>(reader["EstatusNombre"]),
                            Color = MapearPorpiedades<string>(reader["EstatusColor"])
                        };

                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets;
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
        public ModelResponse ObtenerTicketEstatus()
        {
            var modelResponse = new ModelResponse();

            try
            {
                var estatus = GetObjects("ObtenerTicketEstatus", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                    new Func<IDataReader, TicketEstatus>((reader) =>
                    {
                        var e = LlenarEntidad<TicketEstatus>(reader);
                        return e;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = estatus;
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
    }
}