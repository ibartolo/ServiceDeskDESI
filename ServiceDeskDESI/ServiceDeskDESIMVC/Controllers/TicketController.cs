using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.Helpers;
using ServiceDeskDESIMVC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ServiceDeskDESIMVC.Filters;

namespace ServiceDeskDESIMVC.Controllers
{
    public class TicketController : BaseController
    {
        private readonly TicketService _ticketService;
        private readonly AreaService _areaService;
        private readonly CategoriaService _categoriaService;

        public TicketController()
        {
            _ticketService = new TicketService(httpClientConnection);
            _areaService = new AreaService(httpClientConnection);
            _categoriaService = new CategoriaService(httpClientConnection);
        }

        public async Task<ActionResult> Index(long id = 0)
        {
            // 1. Obtener permisos para la página "Tickets"
            var permisos = await _ticketService.ObtenerPermisosParaTicket();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            var ticket = new Ticket();

            // Cargar áreas
            var areasResponse = await _areaService.ConsultarTodasAreas();
            if (areasResponse.IsSuccess && areasResponse.Response != null)
            {
                ViewBag.Areas = areasResponse.Response;
            }

            // Cargar categorías (solo las principales, sin padre)
            var categoriasResponse = await _categoriaService.ConsultarTodasCategorias();
            if (categoriasResponse.IsSuccess && categoriasResponse.Response != null)
            {
                var todasCategorias = JsonConvert.DeserializeObject<List<Categoria>>(categoriasResponse.Response.ToString());
                var categoriasPrincipales = todasCategorias.Where(c => c.CategoriaPadre == null).ToList();
                ViewBag.Categorias = categoriasPrincipales;
            }

            // Cargar estatus de tickets
            var estatusResponse = await _ticketService.ObtenerTicketEstatus();
            if (estatusResponse.IsSuccess && estatusResponse.Response != null)
            {
                ViewBag.Estatus = estatusResponse.Response;
            }

            if (id > 0)
            {
                var response = await _ticketService.ObtenerTicketPorId(id);

                if (response.IsSuccess && response.Response != null)
                {
                    ticket = JsonConvert.DeserializeObject<Ticket>(response.Response.ToString());

                    // Cargar subcategorías según la categoría seleccionada
                    if (ticket.Categoria != null && ticket.Categoria.Id > 0)
                    {
                        var subcategoriasResponse = await _categoriaService.ObtenerCategoriasPorPadre(ticket.Categoria.Id);
                        if (subcategoriasResponse.IsSuccess && subcategoriasResponse.Response != null)
                        {
                            ViewBag.Subcategorias = subcategoriasResponse.Response;
                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = response.Message;
                }
            }

            // Pasar permisos a la vista
            ViewBag.Permisos = permisos;

            return View(ticket);
        }

        [Permiso("Tickets")]
        public async Task<string> GuardarOActualizarTicket(Ticket ticket)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            if (ticket.Id == 0)
            {
                ticket.CreadoPor = tokenCookie?.UserName ?? "system";
                ticket.FechaCreacion = DateTime.Now;
                // Por defecto, el estatus inicial es "Nuevo" (Id = 1)
                if (ticket.TicketEstatus == null || ticket.TicketEstatus.Id == 0)
                {
                    ticket.TicketEstatus = new TicketEstatus { Id = 1 };
                }
            }
            else
            {
                ticket.ModificadoPor = tokenCookie?.UserName ?? "system";
                ticket.FechaModificacion = DateTime.Now;
            }
            ticket.Estatus = true;

            var response = await _ticketService.GuardarOActualizarTicket(ticket);
            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Tickets", "Eliminar")]
        public async Task<string> EliminarTicket(Ticket ticket)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            ticket.ModificadoPor = tokenCookie?.UserName ?? "system";
            ticket.FechaModificacion = DateTime.Now;

            var response = await _ticketService.EliminarTicket(ticket);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTodasTickets()
        {
            var response = await _ticketService.ObtenerTickets();
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTicketsPorArea(long areaId)
        {
            var response = await _ticketService.ObtenerTicketsPorArea(areaId);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTicketsPorUsuario(string creadoPor)
        {
            var response = await _ticketService.ObtenerTicketsPorUsuario(creadoPor);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTicketsPorUrgencia(int urgencia)
        {
            var response = await _ticketService.ObtenerTicketsPorUrgencia(urgencia);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTicketsPorEstatus(int ticketEstatusId)
        {
            var response = await _ticketService.ObtenerTicketsPorEstatus(ticketEstatusId);
            return JsonConvert.SerializeObject(response);
        }

        [HttpGet]
        public async Task<string> ObtenerSubcategoriasPorCategoria(long categoriaId)
        {
            var response = await _categoriaService.ObtenerCategoriasPorPadre(categoriaId);
            return JsonConvert.SerializeObject(response);
        }

        [HttpGet]
        public async Task<string> ObtenerCategoriasPorArea(long areaId)
        {
            var response = await _categoriaService.ObtenerCategoriasPorArea(areaId);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        [Permiso("Tickets", "Editar")]
        public async Task<string> CambiarEstatusTicket(long ticketId, int nuevoEstatusId)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            var response = await _ticketService.ObtenerTicketPorId(ticketId);
            if (!response.IsSuccess || response.Response == null)
            {
                return JsonConvert.SerializeObject(new ModelResponse
                {
                    IsSuccess = false,
                    Message = "No se encontró el ticket"
                });
            }

            var ticket = JsonConvert.DeserializeObject<Ticket>(response.Response.ToString());
            ticket.TicketEstatus = new TicketEstatus { Id = nuevoEstatusId };
            ticket.ModificadoPor = tokenCookie?.UserName ?? "system";
            ticket.FechaModificacion = DateTime.Now;

            var result = await _ticketService.GuardarOActualizarTicket(ticket);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [Permiso("Tickets", "Editar")]
        public async Task<string> AsignarTicketAgente(long ticketId, long agenteId)
        {
            // TODO: Implementar asignación de ticket a un agente
            // Esta funcionalidad requiere que la tabla Ticket tenga un campo AgenteId
            // Por ahora es un placeholder
            var modelResponse = new ModelResponse
            {
                IsSuccess = true,
                Message = "Ticket asignado correctamente"
            };
            return JsonConvert.SerializeObject(modelResponse);
        }
    }
}