using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServiceDeskDESIMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Autenticacion()
        { 
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }
    }
}