using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Swashbuckle.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Reflection;

[assembly: OwinStartup(typeof(ServiceDeskDESIWebApi.App_Start.Startup))]
namespace ServiceDeskDESIWebApi.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureOAuth(app);

            HttpConfiguration config = new HttpConfiguration();

            // Configuración de rutas Web API
            config.MapHttpAttributeRoutes();

            // Configuración de Swagger
            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "ServiceDeskDESI Web API");

                // Sólo incluir comentarios XML si el archivo existe
                string xmlPath = GetXmlCommentsPath();
                if (!string.IsNullOrEmpty(xmlPath) && System.IO.File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                c.DescribeAllEnumsAsStrings();

                // Agregar soporte para token JWT en Swagger
                c.ApiKey("Authorization")
                    .Description("Token JWT. Ejemplo: 'Bearer {token}'")
                    .Name("Authorization")
                    .In("header");
            })
            .EnableSwaggerUi(c =>
            {
                c.DocumentTitle("ServiceDeskDESI API Documentation");
                c.DocExpansion(DocExpansion.List);

                // Habilitar botón de Authorize en Swagger UI
                c.EnableApiKeySupport("Authorization", "header");
            });

            app.UseWebApi(config);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new SimpleAuthorizationServerProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }

        private string GetXmlCommentsPath()
        {
            try
            {
                // Nombre del ensamblado actual (para evitar hardcoding)
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                string xmlFileName = $"{assemblyName}.XML";

                // Base directory de la aplicación
                string basePath = AppDomain.CurrentDomain.BaseDirectory;

                // Ruta en bin (normalmente donde Visual Studio coloca el XML)
                string xmlPath = System.IO.Path.Combine(basePath, "bin", xmlFileName);
                if (System.IO.File.Exists(xmlPath))
                {
                    return xmlPath;
                }

                // Intentar en la raíz de la aplicación
                xmlPath = System.IO.Path.Combine(basePath, xmlFileName);
                if (System.IO.File.Exists(xmlPath))
                {
                    return xmlPath;
                }

                // No encontrado — devolver null para que el llamador no intente abrirlo
                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            var user = new DAL.DbWrapper().AutenticarUsuario(context.UserName, context.Password);

            if (!(user != null && (user.IsSuccess && user.Response != null)))
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim("role", "user"));

            context.Validated(identity);
        }
    }
}