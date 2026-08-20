using ServiceDeskDESIEntities;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ServiceDeskDESIMVC.DAL
{
    public partial class HttpClientConnection : HttpClientBase
    {
        private TokenCookie token;
        public HttpClientConnection(string baseUrl = "") : base(baseUrl)
        {
            token = SessionHelper.GetSessionUser();
        }
        public BaseObject MappingColumSecurity(BaseObject o)
        {
            if (o.Id == 0 || o.Id == -1)
            {
                o.CreadoPor = SessionHelper.GetSessionUser().UserName;
                o.FechaCreacion = SessionHelper.GetDateCenterMexico();
            }
            else
            {
                o.ModificadoPor = SessionHelper.GetSessionUser().UserName;
                o.FechaModificacion = SessionHelper.GetDateCenterMexico();
            }

            return o;
        }
        public async Task<Token> GetToken(string user, string pass)
        {
            return await TokenAsync<Token>("token",
                new[]
                    {
                        new KeyValuePair<string, string>("grant_type","password"),
                        new KeyValuePair<string, string>("UserName",user),
                        new KeyValuePair<string, string>("Password",pass),
                        new KeyValuePair<string, string>("client_id", ConfigurationManager.AppSettings["client_id"]),
                        new KeyValuePair<string, string>("client_secret", ConfigurationManager.AppSettings["client_secret"])
                    }, "application/x-www-form-urlencoded");
        }
    }
}