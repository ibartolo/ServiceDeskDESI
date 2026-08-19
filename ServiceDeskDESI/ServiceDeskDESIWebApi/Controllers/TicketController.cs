using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIWebApi.Filters;
using ServiceDeskDESIWebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Ticket")]
    public class TicketController : BaseController
    {
        private readonly TicketService _ticketService;

        public TicketController()
        {
            _ticketService = new TicketService();
        }

        /// <summary>
        /// Obtiene todos los tickets de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de tickets</returns>
        [HttpGet, Route("List")]
        public ModelResponse<List<TicketDTO>> ObtenerTickets()
        {
            var usuario = User.Identity.Name;
            var result = _ticketService.ObtenerTickets(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene un ticket por su ID
        /// </summary>
        /// <param name="id">ID del ticket</param>
        /// <returns>Ticket encontrado</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse<TicketDTO> ObtenerTicketPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = _ticketService.ObtenerTicketPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un ticket
        /// </summary>
        /// <param name="ticket">Objeto ticket con los datos</param>
        /// <returns>Ticket guardado con su ID actualizado</returns>
        [Permiso("Tickets")]
        [HttpPost, Route("Guardar")]
        public ModelResponse<Ticket> GuardarOActualizarTicket(Ticket ticket)
        {
            var usuario = User.Identity.Name;
            var result = _ticketService.GuardarOActualizarTicket(ticket, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un ticket
        /// </summary>
        /// <param name="ticket">Ticket a eliminar (debe incluir Id, ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [Permiso("Tickets", "Eliminar")]
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarTicket(Ticket ticket)
        {
            var usuario = User.Identity.Name;
            ticket.FechaModificacion = DateTime.Now;
            var result = _ticketService.EliminarTicket(ticket.Id, ticket.ModificadoPor, ticket.FechaModificacion.Value, usuario);
            return result;
        }

        /// <summary>
        /// Obtiene tickets por área
        /// </summary>
        /// <param name="areaId">ID del área</param>
        /// <returns>Lista de tickets del área</returns>
        [HttpGet, Route("Area/{areaId:long}")]
        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorArea(long areaId)
        {
            var usuario = User.Identity.Name;
            var result = _ticketService.ObtenerTicketsPorArea(areaId, usuario);
            return result;
        }

        /// <summary>
        /// Obtiene tickets por usuario creador
        /// </summary>
        /// <param name="creadoPor">Nombre de usuario</param>
        /// <returns>Lista de tickets del usuario</returns>
        [HttpGet, Route("Usuario/{creadoPor}")]
        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorUsuario(string creadoPor)
        {
            var usuario = User.Identity.Name;
            var result = _ticketService.ObtenerTicketsPorUsuario(creadoPor, usuario);
            return result;
        }

        /// <summary>
        /// Obtiene tickets por nivel de urgencia
        /// </summary>
        /// <param name="urgencia">Nivel de urgencia (1=Baja, 2=Media, 3=Alta, 4=Crítica)</param>
        /// <returns>Lista de tickets con esa urgencia</returns>
        [HttpGet, Route("Urgencia/{urgencia:int}")]
        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorUrgencia(int urgencia)
        {
            var usuario = User.Identity.Name;
            var result = _ticketService.ObtenerTicketsPorUrgencia(urgencia, usuario);
            return result;
        }

        /// <summary>
        /// Obtiene tickets por estatus
        /// </summary>
        /// <param name="ticketEstatusId">ID del estatus del ticket</param>
        /// <returns>Lista de tickets con ese estatus</returns>
        [HttpGet, Route("Estatus/{ticketEstatusId:int}")]
        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorEstatus(int ticketEstatusId)
        {
            var usuario = User.Identity.Name;
            var result = _ticketService.ObtenerTicketsPorEstatus(ticketEstatusId, usuario);
            return result;
        }

        /// <summary>
        /// Obtiene todos los estatus de tickets
        /// </summary>
        /// <returns>Lista de estatus</returns>
        [HttpGet, Route("Estatus/List")]
        public ModelResponse<List<TicketEstatus>> ObtenerTicketEstatus()
        {
            var result = _ticketService.ObtenerTicketEstatus();
            return result;
        }

        /// <summary>
        /// Asigna el ticket al agente autenticado (tomar ticket) y lo pasa a "En Progreso".
        /// </summary>
        [Permiso("Tickets", "Editar")]
        [HttpPost, Route("Tomar")]
        public ModelResponse TomarTicket([FromBody] TomarTicketRequest request)
        {
            var usuario = User.Identity.Name;
            var result = _ticketService.TomarTicket(request.TicketId, usuario, request.Comentario);
            return result;
        }

        /// <summary>
        /// Reasigna el ticket a otro agente. Solo el responsable del área puede hacerlo.
        /// </summary>
        [Permiso("Tickets", "Editar")]
        [HttpPost, Route("Reasignar")]
        public ModelResponse ReasignarTicket([FromBody] ReasignarTicketRequest request)
        {
            var usuario = User.Identity.Name;
            var result = _ticketService.ReasignarTicket(request.TicketId, request.NuevoUsuarioId, usuario, request.Comentario);
            return result;
        }

        /// <summary>
        /// Obtiene el historial de asignaciones de un ticket.
        /// </summary>
        [HttpGet, Route("Asignaciones/{ticketId:long}")]
        public ModelResponse<List<TicketAsignacionDTO>> ObtenerTicketAsignaciones(long ticketId)
        {
            var result = _ticketService.ObtenerTicketAsignaciones(ticketId);
            return result;
        }
    }

    public class TomarTicketRequest
    {
        public long TicketId { get; set; }
        public string Comentario { get; set; }
    }

    public class ReasignarTicketRequest
    {
        public long TicketId { get; set; }
        public long NuevoUsuarioId { get; set; }
        public string Comentario { get; set; }
    }
}