using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Serilog; // << añadido

namespace ServiceDeskDESIWebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_End()
        {
            // Asegura que Serilog vacíe y cierre sinks al parar la app
            Log.CloseAndFlush();
        }
    }
}
