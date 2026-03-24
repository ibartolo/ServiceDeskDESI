using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
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
    public class CatalogsController : BaseController
    {
        #region Views
        public async Task<ActionResult> WorkArea(long id = 0)
        {
            var area = new Area();

            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerAreaPorId(id);

                if (response.IsSuccess && response.Response != null)
                {
                    area = JsonConvert.DeserializeObject<Area>(response.Response.ToString());
                }
                else
                {
                    ViewBag.ErrorMessage = response.Message;
                }
            }

            return View(area);
        }

        public ActionResult Company(long id = 0)
        {
            var compania = new Compania();
            return View(compania);
        }

        #endregion

        #region Data Access
        public async Task<string> ConsutlarTodasAreas()
        {
            var response = await httpClientConnection.ObtenerAreas();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarAreaPorId(long id)
        {
            var response = await httpClientConnection.ObtenerAreaPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> GuardarOActualizarArea(Area a)
        {
            var response = await httpClientConnection.GuardarOActualizarArea(a);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> EliminarArea(Area a)
        {
            var response = await httpClientConnection.EliminarArea(a);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        #endregion

    }
}