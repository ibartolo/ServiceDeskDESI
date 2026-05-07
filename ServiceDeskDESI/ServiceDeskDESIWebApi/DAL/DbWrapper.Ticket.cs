using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
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
        public ModelResponse ObtenerTickets()
        {
            var modelResponse = new ModelResponse();

            try
            {
                var tickets = GetObjects("ObtenerTickets", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
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

                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets;
                modelResponse.Message = "Tickets obtenidos correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los tickets";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerTicketPorId(long id)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }

                var ticket = GetObject("ObtenerTicketPorId", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) },
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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el ticket";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarTicket(Ticket t)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                if (t.Area == null || t.Area.Id <= 0) { throw new ArgumentException("El área es requerida."); }
                if (t.Categoria == null || t.Categoria.Id <= 0) { throw new ArgumentException("La categoría es requerida."); }
                if (t.Urgencia <= 0 || t.Urgencia > 4) { throw new ArgumentException("La urgencia debe ser un valor entre 1 y 4."); }
                if (string.IsNullOrWhiteSpace(t.Titulo)) { throw new ArgumentException("El título es requerido."); }
                if (t.Titulo.Length > 250) { throw new ArgumentException("El título no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(t.Descripcion)) { throw new ArgumentException("La descripción es requerida."); }
                if (string.IsNullOrWhiteSpace(t.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

                var parametros = ObtenerParametrosSQL(t).ToArray();
                var ticketId = ExecuteScalar("GuardarOActualizarTicket", CommandType.StoredProcedure, parametros);
                t.Id = Convert.ToInt64(ticketId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = t;
                modelResponse.Message = "Ticket guardado correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el ticket";
            }

            return modelResponse;
        }

        public ModelResponse EliminarTicket(long id, string modificadoPor, DateTime fechaModificacion)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }

                ExecuteNonQuery("EliminarTicket", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@FechaModificacion", fechaModificacion)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Ticket eliminado correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el ticket";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerTicketsPorArea(long areaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (areaId <= 0) { throw new ArgumentException("El ID del área es requerido."); }

                var tickets = GetObjects("ObtenerTicketsPorArea", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@AreaId", areaId) },
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

                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets;
                modelResponse.Message = "Tickets obtenidos correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los tickets por área";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerTicketsPorUsuario(string creadoPor)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(creadoPor)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var tickets = GetObjects("ObtenerTicketsPorUsuario", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@CreadoPor", creadoPor) },
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

                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets;
                modelResponse.Message = "Tickets obtenidos correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los tickets por usuario";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerTicketsPorUrgencia(int urgencia)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (urgencia <= 0 || urgencia > 4) { throw new ArgumentException("La urgencia debe ser un valor entre 1 y 4."); }

                var tickets = GetObjects("ObtenerTicketsPorUrgencia", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Urgencia", urgencia) },
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

                        return ticket;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tickets;
                modelResponse.Message = "Tickets obtenidos correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los tickets por urgencia";
            }

            return modelResponse;
        }
    }
}