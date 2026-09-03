using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
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

        /// <summary>
        /// Obtiene el EmpresaId del usuario autenticado desde el claim del token
        /// ("empresaId", establecido en Startup al emitir el token).
        /// Devuelve 0 si el claim no existe o no es válido.
        /// </summary>
        public long ObtenerEmpresaIdDesdeClaim()
        {
            var identity = User.Identity as ClaimsIdentity;
            var claim = identity?.FindFirst("empresaId");

            long empresaId;
            if (claim != null && long.TryParse(claim.Value, out empresaId))
                return empresaId;

            return 0;
        }
    }
}
