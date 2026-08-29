using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class TicketService
    {
        private readonly DbWrapper _dbWrapper;

        public TicketService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<TicketDTO>> ObtenerTickets(string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerTickets(usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTickets para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTickets para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los tickets."
                };
            }
        }

        public ModelResponse<TicketDTO> ObtenerTicketPorId(long id, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerTicketPorId(id, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTicketPorId para usuario {Usuario}", usuario);
                return new ModelResponse<TicketDTO> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketPorId para usuario {Usuario}", usuario);
                return new ModelResponse<TicketDTO>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener el ticket."
                };
            }
        }

        public ModelResponse<Ticket> GuardarOActualizarTicket(Ticket ticket, string usuario)
        {
            try
            {
                if (ticket.AreaId <= 0) { throw new ArgumentException("El área es requerida."); }
                if (ticket.CategoriaId <= 0) { throw new ArgumentException("La categoría es requerida."); }
                if (ticket.Urgencia <= 0 || ticket.Urgencia > 4) { throw new ArgumentException("La urgencia debe ser un valor entre 1 y 4."); }
                if (string.IsNullOrWhiteSpace(ticket.Titulo)) { throw new ArgumentException("El título es requerido."); }
                if (ticket.Titulo.Length > 250) { throw new ArgumentException("El título no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(ticket.Descripcion)) { throw new ArgumentException("La descripción es requerida."); }
                if (ticket.TicketEstatusId <= 0) { throw new ArgumentException("El estatus del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(ticket.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.GuardarOActualizarTicket(ticket, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarTicket para usuario {Usuario}", usuario);
                return new ModelResponse<Ticket> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.GuardarOActualizarTicket para usuario {Usuario}", usuario);
                return new ModelResponse<Ticket>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar el ticket."
                };
            }
        }

        public ModelResponse EliminarTicket(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.EliminarTicket(id, modificadoPor, fechaModificacion, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.EliminarTicket para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al eliminar el ticket."
                };
            }
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorArea(long areaId, string usuario)
        {
            try
            {
                if (areaId <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerTicketsPorArea(areaId, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTicketsPorArea para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketsPorArea para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los tickets por área."
                };
            }
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorUsuario(string creadoPor, string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(creadoPor)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerTicketsPorUsuario(creadoPor, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTicketsPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketsPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los tickets por usuario."
                };
            }
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorUrgencia(int urgencia, string usuario)
        {
            try
            {
                if (urgencia <= 0 || urgencia > 4) { throw new ArgumentException("La urgencia debe ser un valor entre 1 y 4."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerTicketsPorUrgencia(urgencia, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTicketsPorUrgencia para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketsPorUrgencia para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los tickets por urgencia."
                };
            }
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorEstatus(int ticketEstatusId, string usuario)
        {
            try
            {
                if (ticketEstatusId <= 0) { throw new ArgumentException("El ID del estatus es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerTicketsPorEstatus(ticketEstatusId, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTicketsPorEstatus para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketsPorEstatus para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los tickets por estatus."
                };
            }
        }

        public ModelResponse<List<TicketEstatus>> ObtenerTicketEstatus()
        {
            try
            {
                return _dbWrapper.ObtenerTicketEstatus();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketEstatus");
                return new ModelResponse<List<TicketEstatus>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los estatus de tickets."
                };
            }
        }
    }
}
