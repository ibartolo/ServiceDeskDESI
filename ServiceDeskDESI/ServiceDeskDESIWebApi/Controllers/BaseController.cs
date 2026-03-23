using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    public class BaseController : ApiController
    {
        public DbWrapper dbWrapper;

        public BaseController()
        {
            dbWrapper = new DbWrapper();
        }
    }
}
