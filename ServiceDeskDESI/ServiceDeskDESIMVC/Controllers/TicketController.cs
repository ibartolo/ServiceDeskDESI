using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static ServiceDeskDESIMVC.Helpers.FiltersHelper;

namespace ServiceDeskDESIMVC.Controllers
{
    [Autenticated]
    public class TicketController : BaseController
    {
        public async Task<ActionResult> Index(long id = 0)
        {
            var ticket = new Ticket();

            // Cargar áreas
            var areasResponse = await httpClientConnection.ObtenerAreas();
            if (areasResponse.IsSuccess && areasResponse.Response != null)
            {
                ViewBag.Areas = areasResponse.Response;
            }

            // Cargar categorías (solo las principales, sin padre)
            var categoriasResponse = await httpClientConnection.ObtenerCategorias();
            if (categoriasResponse.IsSuccess && categoriasResponse.Response != null)
            {
                var todasCategorias = JsonConvert.DeserializeObject<List<Categoria>>(categoriasResponse.Response.ToString());
                var categoriasPrincipales = todasCategorias.Where(c => c.CategoriaPadre == null).ToList();
                ViewBag.Categorias = categoriasPrincipales;
            }

            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerTicketPorId(id);

                if (response.IsSuccess && response.Response != null)
                {
                    ticket = JsonConvert.DeserializeObject<Ticket>(response.Response.ToString());

                    // Cargar subcategorías según la categoría seleccionada
                    if (ticket.Categoria != null && ticket.Categoria.Id > 0)
                    {
                        var subcategoriasResponse = await httpClientConnection.ObtenerCategoriasPorPadre(ticket.Categoria.Id);
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

            return View(ticket);
        }

        public async Task<string> GuardarOActualizarTicket(Ticket ticket)
        {
            ticket.Estatus = true;

            var response = await httpClientConnection.GuardarOActualizarTicket(ticket);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> EliminarTicket(Ticket ticket)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            ticket.ModificadoPor = tokenCookie?.UserName ?? "system";
            ticket.FechaModificacion = DateTime.Now;

            var response = await httpClientConnection.EliminarTicket(ticket);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTodasTickets()
        {
            var response = await httpClientConnection.ObtenerTickets();
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTicketsPorArea(long areaId)
        {
            var response = await httpClientConnection.ObtenerTicketsPorArea(areaId);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTicketsPorUsuario(string creadoPor)
        {
            var response = await httpClientConnection.ObtenerTicketsPorUsuario(creadoPor);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTicketsPorUrgencia(int urgencia)
        {
            var response = await httpClientConnection.ObtenerTicketsPorUrgencia(urgencia);
            return JsonConvert.SerializeObject(response);
        }

        [HttpGet]
        public async Task<string> ObtenerSubcategoriasPorCategoria(long categoriaId)
        {
            var response = await httpClientConnection.ObtenerCategoriasPorPadre(categoriaId);
            return JsonConvert.SerializeObject(response);
        }
        [HttpGet]
        public async Task<string> ObtenerCategoriasPorArea(long areaId)
        {
            var response = await httpClientConnection.ObtenerCategoriasPorArea(areaId);
            return JsonConvert.SerializeObject(response);
        }
    }
}