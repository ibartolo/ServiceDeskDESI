using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static ServiceDeskDESIMVC.Helpers.FiltersHelper;

namespace ServiceDeskDESIMVC.Controllers
{
    public class HomeController : Controller
    {
        //[NoAutenticated]
        public ActionResult Autentication()
        { 
            return View();
        }
        //[Autenticated]
        public ActionResult Index()
        {
            return View();
        }
    }
}