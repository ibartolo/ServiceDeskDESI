using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    //[Authorize]
    [RoutePrefix("api/Ticket")]
    public class TicketController : BaseController
    {
        /// <summary>
        /// Obtiene todos los tickets
        /// </summary>
        /// <returns>Lista de tickets</returns>
        [HttpGet, Route("Lista")]
        public ModelResponse ObtenerTickets()
        {
            var result = dbWrapper.ObtenerTickets();
            return result;
        }

        /// <summary>
        /// Obtiene un ticket por su ID
        /// </summary>
        /// <param name="id">ID del ticket</param>
        /// <returns>Ticket encontrado</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerTicketPorId(long id)
        {
            var result = dbWrapper.ObtenerTicketPorId(id);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un ticket
        /// </summary>
        /// <param name="ticket">Objeto ticket con los datos</param>
        /// <returns>Ticket guardado con su ID actualizado</returns>
        [HttpPost, Route("")]
        public ModelResponse GuardarOActualizarTicket(Ticket ticket)
        {
            var result = dbWrapper.GuardarOActualizarTicket(ticket);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un ticket
        /// </summary>
        /// <param name="ticket">Ticket a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("")]
        public ModelResponse EliminarTicket(Ticket ticket)
        {
            ticket.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarTicket(ticket.Id, ticket.ModificadoPor, ticket.FechaModificacion.Value);
            return result;
        }

        /// <summary>
        /// Obtiene tickets por área
        /// </summary>
        /// <param name="areaId">ID del área</param>
        /// <returns>Lista de tickets del área</returns>
        [HttpGet, Route("Area/{areaId:long}")]
        public ModelResponse ObtenerTicketsPorArea(long areaId)
        {
            var result = dbWrapper.ObtenerTicketsPorArea(areaId);
            return result;
        }

        /// <summary>
        /// Obtiene tickets por usuario creador
        /// </summary>
        /// <param name="creadoPor">Nombre de usuario</param>
        /// <returns>Lista de tickets del usuario</returns>
        [HttpGet, Route("Usuario/{creadoPor}")]
        public ModelResponse ObtenerTicketsPorUsuario(string creadoPor)
        {
            var result = dbWrapper.ObtenerTicketsPorUsuario(creadoPor);
            return result;
        }

        /// <summary>
        /// Obtiene tickets por nivel de urgencia
        /// </summary>
        /// <param name="urgencia">Nivel de urgencia (1=Baja, 2=Media, 3=Alta, 4=Crítica)</param>
        /// <returns>Lista de tickets con esa urgencia</returns>
        [HttpGet, Route("Urgencia/{urgencia:int}")]
        public ModelResponse ObtenerTicketsPorUrgencia(int urgencia)
        {
            var result = dbWrapper.ObtenerTicketsPorUrgencia(urgencia);
            return result;
        }
    }
}
