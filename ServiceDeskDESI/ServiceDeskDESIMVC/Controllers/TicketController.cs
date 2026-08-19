using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
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
        private readonly RolService _rolService;
        private readonly UsuarioService _usuarioService;

        public TicketController()
        {
            _ticketService = new TicketService(httpClientConnection);
            _areaService = new AreaService(httpClientConnection);
            _categoriaService = new CategoriaService(httpClientConnection);
            _rolService = new RolService(httpClientConnection);
            _usuarioService = new UsuarioService(httpClientConnection);
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
                var categoriasPrincipales = categoriasResponse.Response.Where(c => c.CategoriaPadreId == null).Cast<Categoria>().ToList();
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
                    ticket = response.Response;

                    // Cargar subcategorías según la categoría seleccionada
                    if (ticket.CategoriaId > 0)
                    {
                        var subcategoriasResponse = await _categoriaService.ObtenerCategoriasPorPadre(ticket.CategoriaId);
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

            // Determinar si el usuario es agente (tiene un rol con PuedeAtenderTickets)
            bool esAgente = false;
            var tokenCookie = SessionHelper.GetSessionUser();
            if (tokenCookie != null && tokenCookie.UserID > 0)
            {
                var rolesResponse = await _rolService.ObtenerRolesPorUsuario(tokenCookie.UserID);
                esAgente = rolesResponse.IsSuccess && rolesResponse.Response != null && rolesResponse.Response.Any(r => r.PuedeAtenderTickets);
            }
            ViewBag.EsAgente = esAgente;

            ViewBag.UsuarioActualId = tokenCookie.UserID;
            ViewBag.UsuarioActualNombre = tokenCookie.UserName;
            bool esResponsableArea = false;
            var usuarioActual = await _usuarioService.ObtenerUsuarioPorId(tokenCookie.UserID);
            if (usuarioActual != null && usuarioActual.AreaId.HasValue)
            {
                var areaActual = await _areaService.ObtenerAreaPorId(usuarioActual.AreaId.Value);
                esResponsableArea = areaActual != null && areaActual.UsuarioResponsableId == tokenCookie.UserID;
            }
            ViewBag.EsResponsableArea = esResponsableArea;

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
                if (ticket.TicketEstatusId == 0)
                {
                    ticket.TicketEstatusId = 1;
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
        public async Task<string> TomarTicket(long ticketId, string comentario)
        {
            var response = await _ticketService.TomarTicket(ticketId, comentario);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        [Permiso("Tickets", "Editar")]
        public async Task<string> ResolverTicket(long ticketId, string comentario)
        {
            var response = await _ticketService.ResolverTicket(ticketId, comentario);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        [Permiso("Tickets", "Leer")]
        public async Task<string> CerrarTicket(long ticketId, string comentario)
        {
            var response = await _ticketService.CerrarTicket(ticketId, comentario);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        [Permiso("Tickets", "Leer")]
        public async Task<string> RechazarTicket(long ticketId, string comentario)
        {
            var response = await _ticketService.RechazarTicket(ticketId, comentario);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        [Permiso("Tickets", "Editar")]
        public async Task<string> RetomarTicket(long ticketId)
        {
            var response = await _ticketService.RetomarTicket(ticketId);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        [Permiso("Tickets", "Editar")]
        public async Task<string> ReasignarTicket(long ticketId, long nuevoUsuarioId, string comentario)
        {
            var response = await _ticketService.ReasignarTicket(ticketId, nuevoUsuarioId, comentario);
            return JsonConvert.SerializeObject(response);
        }

        [HttpGet]
        public async Task<string> ObtenerUsuariosArea(long areaId)
        {
            var response = await _ticketService.ObtenerUsuariosArea(areaId);
            return JsonConvert.SerializeObject(response);
        }

        [HttpGet]
        public async Task<string> ObtenerTicketAsignaciones(long ticketId)
        {
            var response = await _ticketService.ObtenerTicketAsignaciones(ticketId);
            return JsonConvert.SerializeObject(response);
        }
    }
}