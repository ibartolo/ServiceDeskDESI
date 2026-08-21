using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using Swashbuckle.Application;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Reflection;
using Serilog; // << añadido
using Serilog.Events; // opcional

[assembly: OwinStartup(typeof(ServiceDeskDESIWebApi.App_Start.Startup))]
namespace ServiceDeskDESIWebApi.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Inicializar Serilog (archivo diario en App_Data\logs)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "logs", "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            //Log.Information("Inicializando aplicación Web API");

            // Middleware OWIN para CORS abierto (solo pruebas): permite cualquier origen.
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Set("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Set("Access-Control-Allow-Headers", "Authorization, Content-Type");
                context.Response.Headers.Set("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

                if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = 200;
                    return;
                }

                await next.Invoke();
            });

            ConfigureOAuth(app);

            HttpConfiguration config = new HttpConfiguration();

            // Middleware OWIN simple para log de requests y errores
            app.Use(async (context, next) =>
            {
                try
                {
                    //Log.Information("Request {Method} {Path}", context.Request.Method, context.Request.Path);
                    await next.Invoke();
                    //Log.Information("Response {StatusCode} {Method} {Path}", context.Response.StatusCode, context.Request.Method, context.Request.Path);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
                    throw;
                }
            });

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
                AllowInsecureHttp = bool.Parse(ConfigurationManager.AppSettings["AllowInsecureHttp"]),
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(6),
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
            var clientId = ConfigurationManager.AppSettings["client_id"];
            var clientSecret = ConfigurationManager.AppSettings["client_secret"];

            var providedSecret = context.Parameters.Get("client_secret");
            var clientContexId = context.Parameters.Get("client_id");

            Log.Information("ValidateClientAuthentication: providedClientId={ProvidedClientId}, expectedClientId={ExpectedClientId}, secretPresent={SecretPresent}, secretMatch={SecretMatch}",
                clientContexId, clientId, !string.IsNullOrEmpty(providedSecret), string.Equals(providedSecret, clientSecret, StringComparison.Ordinal));

            if (string.Equals(clientContexId, clientId, StringComparison.Ordinal) &&
                string.Equals(providedSecret, clientSecret, StringComparison.Ordinal))
            {
                context.Validated();
                return;
            }

            Log.Warning("Cliente OAuth no válido. ClientId: {ClientId}", context.ClientId);
            context.SetError("invalid_client", "El client_id o client_secret no es válido.");
            context.Rejected();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            Log.Information("=== INICIO GrantResourceOwnerCredentials ===");
            Log.Information("Username: {Username}", context.UserName);

            var user = new DAL.DbWrapper().AutenticarUsuario(context.UserName, context.Password);
            Log.Information("GrantResourceOwnerCredentials: usuario={Usuario}, IsSuccess={IsSuccess}, Message={Message}", context.UserName, user?.IsSuccess, user?.Message);

            if (!(user != null && (user.IsSuccess && user.Response != null)))
            {
                Log.Warning("Autenticación fallida para usuario: {Username}", context.UserName);
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            Log.Information("Usuario autenticado correctamente: {Username}", context.UserName);

            var usuario = (Usuario)user.Response;

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim("usuarioId", usuario.Id.ToString()));
            if (usuario.EmpresaId != null && usuario.EmpresaId.Value > 0)
            {
                identity.AddClaim(new Claim("empresaId", usuario.EmpresaId.Value.ToString()));
            }

            var rolesResponse = new DAL.DbWrapper().ObtenerRolesPorUsuario(context.UserName);
            if (rolesResponse != null && rolesResponse.IsSuccess && rolesResponse.Response != null)
            {
                var roles = (IEnumerable<Rol>)rolesResponse.Response;
                foreach (var rol in roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, rol.Nombre));
                }
            }

            Log.Information("Claims agregados: ClaimTypes.Name = {Name}, usuarioId = {UsuarioId}", context.UserName, usuario.Id);
            Log.Information("=== FIN GrantResourceOwnerCredentials ===");

            context.Validated(identity);
        }
    }
}