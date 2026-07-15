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
        TokenCookie token;
        public CatalogsController() : base()
        {
            token = SessionHelper.GetSessionUser();
        }
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
                var response = await httpClientConnection.ObtenerCompaniaPorId(id,tokenCookie.EmpresaID);
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

        public  async Task<ActionResult> TypeActive(long id = 0)
        {
            var tipoactivo = new TipoActivo();
            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerTipoActivoPorId(id);
                if (response.IsSuccess && response.Response !=null)
                {
                    tipoactivo = JsonConvert.DeserializeObject<TipoActivo>(response.Response.ToString());

                }
                else
                {
                    ViewBag.ErrorMessage = response.Message;
                }
            }
            return View(tipoactivo);
        }

        public async Task<ActionResult> Role (long id = 0)
        {
            var rol = new Rol();
            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerRolPorId(id);
                if (response.IsSuccess && response.Response != null)
                {
                    rol = JsonConvert.DeserializeObject<Rol>(response.Response.ToString());
                }
                else
                {
                    ViewBag.ErrorMessage = response.Message;
                }
            }
            return View(rol);
        }
        public async Task<ActionResult> Branch(long id = 0)
        {
            var sucursal = new Sucursal();
            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerSucursalPorId(id);
                if (response.IsSuccess && response.Response !=null)
                {
                    sucursal = JsonConvert.DeserializeObject<Sucursal>(response.Response.ToString());
                }
                else 
                {
                    ViewBag.ErrorMessage = response.Message;
                }                
            }
            return View(sucursal);
        }

        public async Task<ActionResult>Active(long id = 0)
        {
            // ING AQUI TAMBIEN LE PUSE 0 POR QUE ME MARCABA ERROR 
            var activo = new Activo();
            // cargar tipo activo, modelo, marca
            var tipoactivoResponse = await httpClientConnection.ObtenerTodosLosTipoActivos();
            var tipoactivoList = new List<TipoActivo>();
            if (tipoactivoResponse.IsSuccess && tipoactivoResponse.Response != null)
            {
                ViewBag.TipoActivoss = MappingPropertiToDropDownList(JsonConvert.DeserializeObject<List<TipoActivo>>(tipoactivoResponse.Response.ToString()), "Id", "Nombre");
                tipoactivoList = JsonConvert.DeserializeObject<List<TipoActivo>>(tipoactivoResponse.Response.ToString());
            }
            


            var modeloResponse = await httpClientConnection.ObtenerTodosLosModelos();
            var modeloList = new List<Modelo>();
            if (modeloResponse.IsSuccess && modeloResponse.Response != null)
            {
                ViewBag.Modelos = MappingPropertiToDropDownList(JsonConvert.DeserializeObject<List<Modelo>>(modeloResponse.Response.ToString()), "Id", "Nombre");
                modeloList = JsonConvert.DeserializeObject<List<Modelo>>(modeloResponse.Response.ToString());
            }

            var marcaResponse = await httpClientConnection.ObtenerTodosLasMarcas();
            var marcasList = new List<Marca>();
            if (marcaResponse.IsSuccess && marcaResponse.Response != null)
            {
                ViewBag.Marcass = MappingPropertiToDropDownList(JsonConvert.DeserializeObject<List<Marca>>(marcaResponse.Response.ToString()), "Id", "Nombre");
                marcasList = JsonConvert.DeserializeObject<List<Marca>>(marcaResponse.Response.ToString());
            }
            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerActivoPorId(id);

                if (response.IsSuccess && response.Response != null)
                {
                    activo = JsonConvert.DeserializeObject<Activo>(response.Response.ToString());
                }
                else
                {
                    ViewBag.ErrorMessage = response.Message;
                }
            }
            // Después de obtener el objeto activo, setea el valor seleccionado
            var selectListTipo = MappingPropertiToDropDownList(tipoactivoList, "Id", "Nombre");
            if (activo.TipoActivo != null && activo.TipoActivo.Id > 0)
            {
                foreach (var item in selectListTipo)
                {
                    if (item.Value == activo.TipoActivo.Id.ToString())
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
            ViewBag.TipoActivoss = selectListTipo;

            var selectLista = MappingPropertiToDropDownList(modeloList, "Id", "Nombre");
            if (activo.Modelo != null && activo.Modelo.Id > 0)
            {
                //seleccionar item correspondiente
                foreach (var item in selectLista)
                {
                    if (item.Value == activo.Modelo.Id.ToString())
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
            ViewBag.Modelos = selectLista;

            return View(activo);        
        }
        public async Task<ActionResult> Model(long id = 0)
        {
            var modelo = new Modelo();
            var marcaResponse = await httpClientConnection.ObtenerTodosLasMarcas();
            var marcasList = new List<Marca>();

            if (marcaResponse.IsSuccess && marcaResponse.Response != null)
            {
                ViewBag.Marcas = MappingPropertiToDropDownList(JsonConvert.DeserializeObject<List<Marca>>(marcaResponse.Response.ToString()), "Id", "Nombre");
                marcasList = JsonConvert.DeserializeObject<List<Marca>>(marcaResponse.Response.ToString());
            }

            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerModeloPorId(id);
                if (response.IsSuccess && response.Response != null)
                {
                    modelo = JsonConvert.DeserializeObject<Modelo>(response.Response.ToString());
                }
                else
                {
                    ViewBag.ErrorMessage = response.Message;
                }
            }

            // Asignar el DropDownList con el valor seleccionado si existe
            var selectList = MappingPropertiToDropDownList(marcasList, "Id", "Nombre");
            if (modelo.Marca != null && modelo.Marca.Id > 0)
            {
                // Seleccionar el item correspondiente
                foreach (var item in selectList)
                {
                    if (item.Value == modelo.Marca.Id.ToString())
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }

            ViewBag.Marcas = selectList;

            return View(modelo);
        }
        public async Task<ActionResult> Mark(long id = 0)
        {
            var marca = new Marca();
            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerMarcaPorId(id);
                if (response.IsSuccess && response.Response != null)
                {
                       marca = JsonConvert.DeserializeObject<Marca>(response.Response.ToString());
                }
                else
                {
                    ViewBag.ErrorMessage = response.Message;
                }
            }
            return View(marca);
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
        //public async Task<string> ConsultarTodosLosRoles()
        //{
        //    var response = await httpClientConnection.ObtenerTodosLosRoles();
        //    return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        //}
        //public async Task<string> ConsultarRolPorId(long id)
        //{
        //    var response = await httpClientConnection.ObtenerRolPorId(id);
        //    return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        //}

        //public async Task<string> GuardarOActualizarRol(Rol r)
        //{
        //    var response = await httpClientConnection.GuardarOActualizarRol(r);
        //    return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        //}
        //public async Task<string> EliminarRol(Rol r)
        //{
        //    var response = await httpClientConnection.EliminarRol(r);
        ////    return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        //}
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
        public async Task<string> ConsultarTodasCategorias()
        {
            var response = await httpClientConnection.ObtenerCategorias();
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTodasLasCompanias()
        {
            var response = await httpClientConnection.ObtenerTodasCompanias(tokenCookie.EmpresaID);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarCompaniasPorId(long id)
        {
            var response = await httpClientConnection.ObtenerCompaniaPorId(id,tokenCookie.EmpresaID);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string>GuardarOActualizarCompanias(Compania c)
        {
            var response = await httpClientConnection.GuardarActualizarCompania(c, tokenCookie.EmpresaID);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string>EliminarCompanias(Compania c)
        {
            var response = await httpClientConnection.EliminarCompania(c, tokenCookie.EmpresaID);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarModelosPorMarca(long marcaId)
        {
            var response = await httpClientConnection.ObtenerTodosLosModelos();
            var listModels = JsonConvert.DeserializeObject<List<Modelo>>(response.Response.ToString());
            var modelosPorMarca = listModels.Where(m => m.Marca.Id == marcaId).ToList();
            mr.Response = modelosPorMarca;
            mr.IsSuccess = true;

            return JsonConvert.SerializeObject(mr);
        }
        #endregion

        #region Catalogos
        public async Task<string> ConsultarTodasLasSucursales()
        {
            var response = await httpClientConnection.ObtenerTodasLasSucursales();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodasLasSucursalesPorId(long id)
        {
            var response = await httpClientConnection.ObtenerSucursalPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> GuardarActualizarSucursales(Sucursal s)
        {
            var response = await httpClientConnection.GuardarActualizarSucursal(s);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> EliminarSucurales(Sucursal s)
        {
            var response = await httpClientConnection.EliminarSucursal(s);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTodosLosTipoActivos()
        {
            var response = await httpClientConnection.ObtenerTodosLosTipoActivos();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }        
        public async Task<string> ConsultarTodosLosTipoActivoPorId(long id)
        {
            var response = await httpClientConnection.ObtenerTipoActivoPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }     
        public async Task<string>GuardarOActualizarTipoActivo(TipoActivo t)
        {
            var response = await httpClientConnection.GuardarOActualizarTipoActivo(t);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }       
        public async Task<string>EliminarTipoActivo(TipoActivo t)
        {
            var response = await httpClientConnection.EliminarTipoActivo(t);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        
        public async Task<string> ConsultarTodosLosModelos()
        {
            var response = await httpClientConnection.ObtenerTodosLosModelos();
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodosModelosPorId(long id)
        {
            var response = await httpClientConnection.ObtenerModeloPorId(id);
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> GuardarOActualizarModelos(Modelo m)
        {
            m.Estatus = true;
            var response = await httpClientConnection.GuardarOActualizarModelo(m);
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> EliminarModelos (Modelo m)
        {
            var response = await httpClientConnection.EliminarModelo(m);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTodosLasMarcas()
        {
            var response = await httpClientConnection.ObtenerTodosLasMarcas();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodasMarcasPorId(long id)
        {
            var response = await httpClientConnection.ObtenerMarcaPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> GuardarOActualizarMarca(Marca m)
        {
            var response = await httpClientConnection.GuardarOActualizarMarca(m);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> EliminarMarcas(Marca m)
        {
            var response = await httpClientConnection.EliminarMarca(m);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTodosLosActivo()
        {
            var response = await httpClientConnection.ObtenerTodosLosActivos();
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodosLosActivosPorId(long id)
        {
            var response = await httpClientConnection.ObtenerActivoPorId(id);
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> GuardarOActualizarActivos(Activo a)
        {
            var response = await httpClientConnection.GuardarOActualizarActivo(a);
            return JsonConvert.SerializeObject(response);

        }
        public async Task<string> EliminarActivos(Activo a)
        {
            var response = await httpClientConnection.EliminarActivo(a);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTodosLosRoles()
        {
            var response = await httpClientConnection.ObtenerTodosLosRoles();
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodosLosRolesPorId(long id)
        {
            var response = await httpClientConnection.ObtenerRolPorId(id);
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> GuardarOActualizarRol(Rol r)
        {
            var response = await httpClientConnection.GuardarOActualizarRol(r);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> EliminarRol(Rol r)
        {
            var response = await httpClientConnection.EliminarRol(r);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}