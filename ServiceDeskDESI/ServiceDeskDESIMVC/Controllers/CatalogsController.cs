using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
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

        public async Task<ActionResult> Company(long id = 0)
        {
            var compania = new Compania();
            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerCompaniaPorId(id);
                if (response.IsSuccess && response.Response !=null)
                {
                    compania = JsonConvert.DeserializeObject<Compania>(response.Response.ToString());
                }
                else
                {
                    ViewBag.ErrorMessage = response.Message;
                }
            }
                return View(compania);
        }

        public async Task<ActionResult> MyProfile()
        {
            // Obtener el ID del usuario desde la sesión
            var tokenCookie = SessionHelper.GetSessionUser();
            if (tokenCookie == null || tokenCookie.UserID == 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var usuario = new Usuario();
            var response = await httpClientConnection.ObtenerUsuarioPorId(tokenCookie.UserID);

            if (response.IsSuccess && response.Response != null)
            {
                usuario = JsonConvert.DeserializeObject<Usuario>(response.Response.ToString());
            }
            else
            {
                ViewBag.ErrorMessage = response.Message;
            }

            // Cargar listas para los dropdowns
            var sucursalesResponse = await httpClientConnection.ObtenerSucursales();
            if (sucursalesResponse.IsSuccess && sucursalesResponse.Response != null)
            {
                ViewBag.Sucursales = sucursalesResponse.Response;
            }

            var areasResponse = await httpClientConnection.ObtenerAreas();
            if (areasResponse.IsSuccess && areasResponse.Response != null)
            {
                ViewBag.Areas = areasResponse.Response;
            }

            return View(usuario);
        }

        public class CambioContrasenaRequest
        {
            public long Id { get; set; }
            public string ContrasenaActual { get; set; }
            public string NuevaContrasena { get; set; }
        }

        public async Task<ActionResult> Category(long id = 0)
        {
            var categoria = new Categoria();

            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerCategoriaPorId(id);

                if (response.IsSuccess && response.Response != null)
                {
                    categoria = JsonConvert.DeserializeObject<Categoria>(response.Response.ToString());
                }
                else
                {
                    ViewBag.ErrorMessage = response.Message;
                }
            }

            // Cargar áreas para el dropdown
            var areasResponse = await httpClientConnection.ObtenerAreas();
            if (areasResponse.IsSuccess && areasResponse.Response != null)
            {
                ViewBag.Areas = areasResponse.Response;
            }

            // Cargar categorías padres para el dropdown (solo categorías principales)
            if (ViewBag.Areas != null)
            {
                var areaId = categoria.Area?.Id ?? 0;
                if (areaId > 0)
                {
                    var categoriasResponse = await httpClientConnection.ObtenerCategoriasPorArea(areaId);
                    if (categoriasResponse.IsSuccess && categoriasResponse.Response != null)
                    {
                        var categorias = JsonConvert.DeserializeObject<List<Categoria>>(categoriasResponse.Response.ToString());
                        ViewBag.CategoriasPadre = categorias.Where(x => x.CategoriaPadre == null).ToList();
                    }
                }
            }

            return View(categoria);
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
        public async Task<string> GuardarPerfil()
        {
            var modelResponse = new ModelResponse();

            try
            {
                var usuario = new Usuario
                {
                    Id = Convert.ToInt64(Request.Form["Id"]),
                    NombreUsuario = Request.Form["NombreUsuario"],
                    Correo = Request.Form["Correo"],
                    Nombre = Request.Form["Nombre"],
                    Apellido = Request.Form["Apellido"],
                    Celular = Request.Form["Celular"],
                    RFC = Request.Form["RFC"],
                    Sucursal = new Sucursal { Id = Convert.ToInt64(Request.Form["SucursalId"]) },
                    Area = new Area { Id = Convert.ToInt64(Request.Form["AreaId"]) },
                    CreadoPor = Request.Form["CreadoPor"],
                    FechaCreacion = Convert.ToDateTime(Request.Form["FechaCreacion"]),
                    ModificadoPor = Request.Form["ModificadoPor"] ?? SessionHelper.GetSessionUser()?.UserName,
                    FechaModificacion = DateTime.Now,
                    Estatus = true,
                    Contrasena = Request.Form["Contrasena"]
                };

                // Manejar imagen de perfil
                if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var file = Request.Files[0];
                    var fileName = $"{usuario.Id}_{DateTime.Now.Ticks}{System.IO.Path.GetExtension(file.FileName)}";
                    var path = System.IO.Path.Combine(Server.MapPath("~/Uploads/Perfiles"), fileName);

                    if (!System.IO.Directory.Exists(Server.MapPath("~/Uploads/Perfiles")))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Uploads/Perfiles"));
                    }

                    file.SaveAs(path);
                    usuario.ImagenPerfil = $"/Uploads/Perfiles/{fileName}";
                }

                var response = await httpClientConnection.GuardarOActualizarUsuario(usuario);
                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el perfil";
                return JsonConvert.SerializeObject(modelResponse);
            }
        }
        public async Task<string> CambiarContrasena(CambioContrasenaRequest request)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var tokenCookie = SessionHelper.GetSessionUser();
                if (tokenCookie == null || tokenCookie.UserID == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Sesión no válida";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                // Validar contraseña actual
                var usuarioResponse = await httpClientConnection.ObtenerUsuarioPorId(tokenCookie.UserID);

                if (!usuarioResponse.IsSuccess || usuarioResponse.Response == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se pudo obtener la información del usuario";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                var usuario = JsonConvert.DeserializeObject<Usuario>(usuarioResponse.Response.ToString());

                if (usuario.Contrasena != request.ContrasenaActual)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "La contraseña actual es incorrecta";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                // Actualizar contraseña
                usuario.Contrasena = request.NuevaContrasena;
                usuario.ModificadoPor = tokenCookie.UserName;
                usuario.FechaModificacion = DateTime.Now;

                var response = await httpClientConnection.ActualizarContrasena(usuario);
                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al cambiar la contraseña";
                return JsonConvert.SerializeObject(modelResponse);
            }
        }
        public async Task<string> GuardarOActualizarCategoria(Categoria categoria)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            if (categoria.Id == 0)
            {
                categoria.CreadoPor = tokenCookie?.UserName ?? "system";
                categoria.FechaCreacion = DateTime.Now;
            }
            else
            {
                categoria.ModificadoPor = tokenCookie?.UserName ?? "system";
                categoria.FechaModificacion = DateTime.Now;
            }
            categoria.Estatus = true;

            var response = await httpClientConnection.GuardarOActualizarCategoria(categoria);
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> EliminarCategoria(Categoria categoria)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            categoria.ModificadoPor = tokenCookie?.UserName ?? "system";
            categoria.FechaModificacion = DateTime.Now;

            var response = await httpClientConnection.EliminarCategoria(categoria);
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodasCategoriasPorArea(long id)
        {
            var response = await httpClientConnection.ObtenerCategoriasPorArea(id);
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodasCategorias()
        {
            var response = await httpClientConnection.ObtenerCategorias();
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTodasLasCompanias()
        {
            var response = await httpClientConnection.ObtenerTodasCompanias();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarCompaniasPorId(long id)
        {
            var response = await httpClientConnection.ObtenerCompaniaPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string>GuardarOActualizarCompanias(Compania c)
        {
            var response = await httpClientConnection.GuardarActualizarCompania(c);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string>EliminarCompanias(Compania c)
        {
            var response = await httpClientConnection.EliminarCompania(c);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        #endregion

    }
}