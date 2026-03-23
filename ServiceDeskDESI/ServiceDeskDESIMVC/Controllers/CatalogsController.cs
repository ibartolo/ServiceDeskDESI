using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceDeskDESIEntities.Catalogos;

namespace ServiceDeskDESIMVC.Controllers
{
    public class CatalogsController : Controller
    {
        #region Views
        public ActionResult WorkArea(long id = 0)
        {
            var area = new ServiceDeskDESIEntities.Catalogos.Area();
            return View(area);
        }

        public ActionResult Company(long id = 0)
        {
            var compania = new ServiceDeskDESIEntities.Catalogos.Compania();
            return View(compania);
        }

        #endregion

        #region Data Access
        public string ConsutlarTodasAreas()
        {
            var areas = new List<Area>();
            areas.Add(new Area() { Id = 1, Nombre = "Sistemas", Descripcion = "Area de sistemas", Correo = "" });
            areas.Add(new Area() { Id = 2, Nombre = "Recursos Humanos", Descripcion = "Area de recursos humanos", Correo = "" });
            return Newtonsoft.Json.JsonConvert.SerializeObject(areas);
        }
        #endregion

      
    }
}