using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.Helpers;
using ServiceDeskDESIMVC.Models;
using ServiceDeskDESIMVC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Management;
using System.Web.Mvc;
using ServiceDeskDESIMVC.Filters;

namespace ServiceDeskDESIMVC.Controllers
{
    public class CatalogsController : BaseController
    {
        TokenCookie token;
        private readonly AreaService _areaService;
        private readonly CompaniaService _companiaService;
        private readonly TipoActivoService _tipoActivoService;
        private readonly SucursalService _sucursalService;
        private readonly ActivoService _activoService;
        private readonly ModeloService _modeloService;
        private readonly MarcaService _marcaService;
        private readonly CategoriaService _categoriaService;
        private readonly UsuarioService _usuarioService;
        private readonly CategoriaResponsableService _categoriaResponsableService;  
        private readonly RolService _rolService;
        private readonly PuestoService _puestoService;
        private readonly PersonaService _personaService;
        public CatalogsController() : base()
        {
            token = SessionHelper.GetSessionUser();
            _areaService = new AreaService(httpClientConnection);
            _companiaService = new CompaniaService(httpClientConnection);
            _tipoActivoService = new TipoActivoService(httpClientConnection);
            _sucursalService = new SucursalService(httpClientConnection);
            _activoService = new ActivoService(httpClientConnection);
            _modeloService = new ModeloService(httpClientConnection);
            _marcaService = new MarcaService(httpClientConnection);
            _categoriaService = new CategoriaService(httpClientConnection);
            _usuarioService = new UsuarioService(httpClientConnection);
            _categoriaResponsableService = new CategoriaResponsableService(httpClientConnection);
            _rolService = new RolService(httpClientConnection);
            _puestoService = new PuestoService(httpClientConnection);
            _personaService = new PersonaService(httpClientConnection);

        }

        #region Views
        public async Task<ActionResult> WorkArea(long id = 0)
        {
            // 1. Obtener permisos
            var permisos = await _areaService.ObtenerPermisosParaArea();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            // 3. Obtener el área si tiene ID
            var area = new Area();
            if (id > 0)
            {
                var areaResponse = await _areaService.ObtenerAreaPorId(id);
                if (areaResponse != null)
                {
                    area = areaResponse;
                }
            }

            // 4. Pasar permisos a la vista
            ViewBag.Permisos = permisos;

            return View(area);
        }
        public async Task<ActionResult> Company(long id = 0)
        {
            // 1. Obtener permisos
            var permisos = await _companiaService.ObtenerPermisosParaCompania();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            var compania = new Compania();

            if (id > 0)
            {
                var companiaResponse = await _companiaService.ObtenerCompaniaPorId(id);
                if (companiaResponse != null)
                {
                    compania = companiaResponse;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se encontró la compañía.";
                }
            }

            ViewBag.Permisos = permisos;
            return View(compania);
        }
        public async Task<ActionResult> Tipped (long id = 0)
        {
           var permisos = await _puestoService.ObtenerPermisosParaPuesto();
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }
            var puesto = new Puesto();
            if (id > 0)
            {
                var response = await _puestoService.ObtenerPuestoPorId(id);
                if (response != null)
                {
                    puesto = response;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se encontró el puesto.";
                }
            }
            ViewBag.Permisos = permisos;
            return View(puesto);
        }
        public  async Task <ActionResult> People(long id = 0)
        {
            var permisos = await _personaService.ObtenerPermisosParaPersona();
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }
            var persona = new Persona();
            if (id > 0)
            {
                var response = await _personaService.ObtenerPersonaPorId(id);
                if (response != null)
                {
                    persona = response;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se encontró la persona.";
                }
            }
            ViewBag.Permisos = permisos;
            return View(persona);
        }

        public async Task<ActionResult> TypeActive(long id = 0)
        {
            // 1. Obtener permisos para la página "Tipo Activo"
            var permisos = await _tipoActivoService.ObtenerPermisosParaTipoActivo();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            var tipoActivo = new TipoActivo();

            if (id > 0)
            {
                var response = await _tipoActivoService.ObtenerTipoActivoPorId(id);
                if (response != null)
                {
                    tipoActivo = response;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se encontró el tipo de activo.";
                }
            }

            // 3. Pasar permisos a la vista
            ViewBag.Permisos = permisos;

            return View(tipoActivo);
        }
        public async Task<ActionResult> Branch(long id = 0)
        {
            // 1. Obtener permisos para la página "Sucursales"
            var permisos = await _sucursalService.ObtenerPermisosParaSucursal();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            var sucursal = new Sucursal();

            if (id > 0)
            {
                var response = await _sucursalService.ObtenerSucursalPorId(id);
                if (response != null)
                {
                    sucursal = response;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se encontró la sucursal.";
                }
            }

            // 3. Pasar permisos a la vista
            ViewBag.Permisos = permisos;

            return View(sucursal);
        }
        public async Task<ActionResult> Active(long id = 0)
        {
            // 1. Obtener permisos para la página "Activos"
            var permisos = await _activoService.ObtenerPermisosParaActivo();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            var activo = new Activo();

            // Cargar tipo activo, modelo, marca
            var tipoactivoResponse = await _tipoActivoService.ConsultarTodosLosTipoActivos();
            var tipoactivoList = new List<TipoActivo>();
            if (tipoactivoResponse.IsSuccess && tipoactivoResponse.Response != null)
            {
                ViewBag.TipoActivoss = MappingPropertiToDropDownList(tipoactivoResponse.Response, "Id", "Nombre");
                tipoactivoList = tipoactivoResponse.Response;
            }

            var modeloResponse = await _modeloService.ConsultarTodosLosModelos();
            var modeloList = new List<Modelo>();
            if (modeloResponse.IsSuccess && modeloResponse.Response != null)
            {
                ViewBag.Modelos = MappingPropertiToDropDownList(modeloResponse.Response.Cast<Modelo>().ToList(), "Id", "Nombre");
                modeloList = modeloResponse.Response.Cast<Modelo>().ToList();
            }

            var marcaResponse = await _marcaService.ConsultarTodosLasMarcas();
            var marcasList = new List<Marca>();
            if (marcaResponse.IsSuccess && marcaResponse.Response != null)
            {
                ViewBag.Marcass = MappingPropertiToDropDownList(marcaResponse.Response, "Id", "Nombre");
                marcasList = marcaResponse.Response;
            }

            if (id > 0)
            {
                var activoResponse = await _activoService.ObtenerActivoPorId(id);
                if (activoResponse != null)
                {
                    activo = activoResponse;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se encontró el activo.";
                }
            }

            // Después de obtener el objeto activo, setea el valor seleccionado
            var selectListTipo = MappingPropertiToDropDownList(tipoactivoList, "Id", "Nombre");
            if (activo.TipoActivoId.HasValue && activo.TipoActivoId.Value > 0)
            {
                foreach (var item in selectListTipo)
                {
                    if (item.Value == activo.TipoActivoId.Value.ToString())
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
            ViewBag.TipoActivoss = selectListTipo;

            var selectListModelo = MappingPropertiToDropDownList(modeloList, "Id", "Nombre");
            if (activo.ModeloId.HasValue && activo.ModeloId.Value > 0)
            {
                foreach (var item in selectListModelo)
                {
                    if (item.Value == activo.ModeloId.Value.ToString())
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
            ViewBag.Modelos = selectListModelo;

            var selectListMarca = MappingPropertiToDropDownList(marcasList, "Id", "Nombre");
            if (activo.MarcaId.HasValue && activo.MarcaId.Value > 0)
            {
                foreach (var item in selectListMarca)
                {
                    if (item.Value == activo.MarcaId.Value.ToString())
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
            ViewBag.Marcass = selectListMarca;

            // 3. Pasar permisos a la vista
            ViewBag.Permisos = permisos;

            return View(activo);
        }
        public async Task<ActionResult> Model(long id = 0)
        {
            // 1. Obtener permisos para la página "Modelos"
            var permisos = await _modeloService.ObtenerPermisosParaModelo();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            var modelo = new Modelo();

            if (id > 0)
            {
                var modeloResponse = await _modeloService.ObtenerModeloPorId(id);
                if (modeloResponse != null)
                {
                    modelo = modeloResponse;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se encontró el modelo.";
                }
            }

            // Cargar marcas
            var marcaResponse = await _marcaService.ConsultarTodosLasMarcas();
            var marcasList = new List<Marca>();

            if (marcaResponse.IsSuccess && marcaResponse.Response != null)
            {
                marcasList = marcaResponse.Response;
                // Asignar el DropDownList con el valor seleccionado si existe
                var selectList = MappingPropertiToDropDownList(marcasList, "Id", "Nombre");
                if (modelo.MarcaId.HasValue && modelo.MarcaId.Value > 0)
                {
                    foreach (var item in selectList)
                    {
                        if (item.Value == modelo.MarcaId.Value.ToString())
                        {
                            item.Selected = true;
                            break;
                        }
                    }
                }

                ViewBag.Marcas = selectList;
            }

            // 3. Pasar permisos a la vista
            ViewBag.Permisos = permisos;

            return View(modelo);
        }
        public async Task<ActionResult> Mark(long id = 0)
        {
            // 1. Obtener permisos para la página "Marcas"
            var permisos = await _marcaService.ObtenerPermisosParaMarca();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            var marca = new Marca();

            if (id > 0)
            {
                var marcaResponse = await _marcaService.ObtenerMarcaPorId(id);
                if (marcaResponse != null)
                {
                    marca = marcaResponse;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se encontró la marca.";
                }
            }

            // 3. Pasar permisos a la vista
            ViewBag.Permisos = permisos;

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

            var usuario = await _usuarioService.ObtenerUsuarioPorId(tokenCookie.UserID);
            if (usuario == null)
            {
                usuario = new Usuario();
                ViewBag.ErrorMessage = "No se pudo obtener el usuario.";
            }

            // Cargar listas para los dropdowns
            var sucursalesResponse = await _sucursalService.ConsultarTodasSucursales();
            if (sucursalesResponse.IsSuccess && sucursalesResponse.Response != null)
            {
                ViewBag.Sucursales = sucursalesResponse.Response;
            }

            var areasResponse = await _areaService.ConsultarTodasAreas();
            if (areasResponse.IsSuccess && areasResponse.Response != null)
            {
                ViewBag.Areas = areasResponse.Response;
            }

            return View(usuario);
        }

        public async Task<ActionResult> Category(long id = 0)
        {
            // 1. Obtener permisos para la página "Categorías"
            var permisos = await _categoriaService.ObtenerPermisosParaCategoria();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            var categoria = new Categoria();

            if (id > 0)
            {
                var categoriaResponse = await _categoriaService.ObtenerCategoriaPorId(id);
                if (categoriaResponse != null)
                {
                    categoria = categoriaResponse;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se encontró la categoría.";
                }
            }

            // Cargar áreas para el dropdown
            var areasResponse = await _areaService.ConsultarTodasAreas();
            if (areasResponse.IsSuccess && areasResponse.Response != null)
            {
                ViewBag.Areas = areasResponse.Response;
            }

            // Cargar categorías padres para el dropdown (solo categorías principales)
            if (ViewBag.Areas != null)
            {
                var areaId = categoria.AreaId;
                if (areaId > 0)
                {
                    var categoriasResponse = await _categoriaService.ConsultarTodasCategorias();
                    if (categoriasResponse.IsSuccess && categoriasResponse.Response != null)
                    {
                        ViewBag.CategoriasPadre = categoriasResponse.Response
                            .Where(x => x.CategoriaPadreId == null && x.AreaId == areaId)
                            .Cast<Categoria>()
                            .ToList();
                    }
                }
            }

            // 3. Pasar permisos a la vista
            ViewBag.Permisos = permisos;

            return View(categoria);
        }

        public async Task<ActionResult> CategoriaResponsable(long categoriaId = 0)
        {
            // 1. Obtener permisos para la página "Categorías"
            var permisos = await _categoriaService.ObtenerPermisosParaCategoria();

            // 2. Validar permiso de lectura
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            var model = new CategoriaResponsableViewModel
            {
                CategoriaId = categoriaId,
                Responsables = new List<CategoriaResponsable>()
            };

            // Obtener la categoría para mostrar información
            if (categoriaId > 0)
            {
                var categoriaResponse = await _categoriaService.ObtenerCategoriaPorId(categoriaId);
                if (categoriaResponse != null)
                {
                    model.Categoria = categoriaResponse;
                }
            }

            // Cargar listas para dropdowns
            // 1. Categorías disponibles (para asignar responsables)
            var categoriasResponse = await _categoriaService.ConsultarTodasCategorias();
            if (categoriasResponse.IsSuccess && categoriasResponse.Response != null)
            {
                    // Filtrar solo categorías padre (sin padre)
                    var categoriasPadre = categoriasResponse.Response.Where(c => c.CategoriaPadreId == null).Cast<Categoria>().ToList();
                ViewBag.Categorias = categoriasPadre;
            }

            // 2. Usuarios que pueden atender tickets (con rol que tiene PuedeAtenderTickets = true)
            var usuariosResponse = await _usuarioService.ConsultarTodosLosUsuarios();
            if (usuariosResponse.IsSuccess && usuariosResponse.Response != null)
            {
                // Filtrar solo usuarios que pueden atender tickets (con rol PuedeAtenderTickets = true)
                // Esto requiere que el usuario tenga la información del rol
                // Por ahora asumimos que el servicio ya filtra
                ViewBag.Usuarios = usuariosResponse.Response;
            }

            // Obtener responsables de la categoría si existe
            if (categoriaId > 0)
            {
                var responsablesResponse = await _categoriaResponsableService.ObtenerResponsablesPorCategoria(categoriaId);
                if (responsablesResponse.IsSuccess && responsablesResponse.Response != null)
                {
                    model.Responsables = responsablesResponse.Response.Cast<CategoriaResponsable>().ToList();
                }
            }

            ViewBag.Permisos = permisos;

            return View(model);
        }

        #endregion

        #region Data Access

        #region Area
        public async Task<string> ConsutlarTodasAreas()
        {
            var response = await _areaService.ConsultarTodasAreas();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarAreaPorId(long id)
        {
            var response = await _areaService.ObtenerAreaPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        [Permiso("Áreas")]
        public async Task<string> GuardarOActualizarArea(Area a)
        {
            var response = await _areaService.GuardarOActualizarArea(a);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        [Permiso("Áreas", "Eliminar")]
        public async Task<string> EliminarArea(Area a)
        {
            var response = await _areaService.EliminarArea(a);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        #endregion

        #region Mi Perfil
        [Permiso("Mi Perfil", "Editar")]
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
                    SucursalId = Convert.ToInt64(Request.Form["SucursalId"]),
                    AreaId = Convert.ToInt64(Request.Form["AreaId"]),
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

                var response = await _usuarioService.GuardarOActualizarUsuarioAdmin(usuario);
                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el perfil";
                return JsonConvert.SerializeObject(modelResponse);
            }
        }
        #endregion

        #region Categoria
        [Permiso("Categorías")]
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

            var response = await _categoriaService.GuardarOActualizarCategoria(categoria);
            return JsonConvert.SerializeObject(response);
        }
        [Permiso("Categorías", "Eliminar")]
        public async Task<string> EliminarCategoria(Categoria categoria)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            categoria.ModificadoPor = tokenCookie?.UserName ?? "system";
            categoria.FechaModificacion = DateTime.Now;

            var response = await _categoriaService.EliminarCategoria(categoria);
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodasCategorias()
        {
            var response = await _categoriaService.ConsultarTodasCategorias();
            return JsonConvert.SerializeObject(response);
        }
        #endregion

        #region Compañia
        public async Task<string> ConsultarTodasLasCompanias()
        {
            var response = await _companiaService.ConsultarTodasCompanias();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarCompaniasPorId(long id)
        {
            var response = await _companiaService.ObtenerCompaniaPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        [Permiso("Compañías")]
        public async Task<string> GuardarOActualizarCompanias(Compania c)
        {
            var response = await _companiaService.GuardarOActualizarCompania(c);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        [Permiso("Compañías", "Eliminar")]
        public async Task<string> EliminarCompanias(Compania c)
        {
            var response = await _companiaService.EliminarCompania(c);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        #endregion

        #region Sucursal
        public async Task<string> ConsultarTodasLasSucursales()
        {
            var response = await _sucursalService.ConsultarTodasSucursales();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodasLasSucursalesPorId(long id)
        {
            var response = await _sucursalService.ObtenerSucursalPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        [Permiso("Sucursales")]
        public async Task<string> GuardarActualizarSucursales(Sucursal s)
        {
            var response = await _sucursalService.GuardarOActualizarSucursal(s);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        [Permiso("Sucursales", "Eliminar")]
        public async Task<string> EliminarSucurales(Sucursal s)
        {
            var response = await _sucursalService.EliminarSucursal(s);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        #endregion
        #region Puesto
        public async Task<string> ConsultarTodosLosPuestos()
        {
            var response = await _puestoService.ConsultarTodosLosPuestos();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarPuestoPorId(long id)
        {
            var response = await _puestoService.ObtenerPuestoPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        [Permiso("Tipped")]
        public async Task<string> GuardarOActualizarPuesto(Puesto p)
        {
            var response = await _puestoService.GuardarOActualizarPuesto(p);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        [Permiso("Tipped", "Eliminar")]
        public async Task<string> EliminarPuesto(Puesto p)
        {
            var response = await _puestoService.EliminarPuesto(p);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        #endregion
        #region persona
        public async Task<string> ConsultarTodasLasPersonas()
        {
            var response = await _personaService.ConsultarTodasLasPersonas();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarPersonaPorId(long id)
        {
            var response = await _personaService.ObtenerPersonaPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        [Permiso("People")]
        public async Task<string> GuardarOActualizarPersona(Persona p)
        {
            var response = await _personaService.GuardarOActualizarPersona(p);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        [Permiso("People", "Eliminar")]
        public async Task<string> EliminarPersona(Persona p)
        {
            var response = await _personaService.EliminarPersona(p);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        #endregion

        #region Tipo Activo
        public async Task<string> ConsultarTodosLosTipoActivos()
        {
            var response = await _tipoActivoService.ConsultarTodosLosTipoActivos();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodosLosTipoActivoPorId(long id)
        {
            var response = await _tipoActivoService.ObtenerTipoActivoPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        [Permiso("Tipo Activo")]
        public async Task<string> GuardarOActualizarTipoActivo(TipoActivo t)
        {
            var response = await _tipoActivoService.GuardarOActualizarTipoActivo(t);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        [Permiso("Tipo Activo", "Eliminar")]
        public async Task<string> EliminarTipoActivo(TipoActivo t)
        {
            var response = await _tipoActivoService.EliminarTipoActivo(t);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        #endregion

        #region Modelo
        public async Task<string> ConsultarTodosLosModelos()
        {
            var response = await _modeloService.ConsultarTodosLosModelos();
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodosModelosPorId(long id)
        {
            var response = await _modeloService.ObtenerModeloPorId(id);
            return JsonConvert.SerializeObject(response);
        }
        [Permiso("Modelos")]
        public async Task<string> GuardarOActualizarModelos(Modelo m)
        {
            m.Estatus = true;
            var response = await _modeloService.GuardarOActualizarModelo(m);
            return JsonConvert.SerializeObject(response);
        }
        [Permiso("Modelos", "Eliminar")]
        public async Task<string> EliminarModelos(Modelo m)
        {
            var response = await _modeloService.EliminarModelo(m);
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarModelosPorMarca(long marcaId)
        {
            var response = await httpClientConnection.ObtenerTodosLosModelos();
            var listModels = response.Response.Cast<Modelo>().ToList();
            var modelosPorMarca = listModels.Where(m => m.MarcaId == marcaId).ToList();
            mr.Response = modelosPorMarca;
            mr.IsSuccess = true;

            return JsonConvert.SerializeObject(mr);
        }
        #endregion

        #region Marca
        public async Task<string> ConsultarTodosLasMarcas()
        {
            var response = await _marcaService.ConsultarTodosLasMarcas();
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodasMarcasPorId(long id)
        {
            var response = await _marcaService.ObtenerMarcaPorId(id);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        [Permiso("Marcas")]
        public async Task<string> GuardarOActualizarMarca(Marca m)
        {
            var response = await _marcaService.GuardarOActualizarMarca(m);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        [Permiso("Marcas", "Eliminar")]
        public async Task<string> EliminarMarcas(Marca m)
        {
            var response = await _marcaService.EliminarMarca(m);
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
        
        #endregion

        #region Activo
        public async Task<string> ConsultarTodosLosActivo()
        {
            var response = await _activoService.ConsultarTodosLosActivos();
            return JsonConvert.SerializeObject(response);
        }
        public async Task<string> ConsultarTodosLosActivosPorId(long id)
        {
            var response = await _activoService.ObtenerActivoPorId(id);
            return JsonConvert.SerializeObject(response);
        }
        [Permiso("Activos")]
        public async Task<string> GuardarOActualizarActivos(Activo a)
        {
            var response = await _activoService.GuardarOActualizarActivo(a);
            return JsonConvert.SerializeObject(response);

        }
        [Permiso("Activos", "Eliminar")]
        public async Task<string> EliminarActivos(Activo a)
        {
            var response = await _activoService.EliminarActivo(a);
            return JsonConvert.SerializeObject(response);
        }
        #endregion

        public async Task<string> ConsultarResponsablesPorCategoria(long categoriaId)
        {
            var response = await _categoriaResponsableService.ObtenerResponsablesPorCategoria(categoriaId);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarCategoriasPorResponsable(long usuarioId)
        {
            var response = await _categoriaResponsableService.ObtenerCategoriasPorResponsable(usuarioId);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarTodosLosResponsables()
        {
            var response = await _categoriaResponsableService.ObtenerTodosLosResponsables();
            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Responsables por Categoría", "Crear")]
        public async Task<string> GuardarOActualizarCategoriaResponsable(CategoriaResponsable categoriaResponsable)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            if (categoriaResponsable.Id == 0)
            {
                categoriaResponsable.CreadoPor = tokenCookie?.UserName ?? "system";
                categoriaResponsable.FechaCreacion = DateTime.Now;
            }
            else
            {
                categoriaResponsable.ModificadoPor = tokenCookie?.UserName ?? "system";
                categoriaResponsable.FechaModificacion = DateTime.Now;
            }
            categoriaResponsable.Estatus = true;

            var response = await _categoriaResponsableService.GuardarOActualizarCategoriaResponsable(categoriaResponsable);
            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Responsables por Categoría", "Eliminar")]
        public async Task<string> EliminarCategoriaResponsable(CategoriaResponsable categoriaResponsable)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            categoriaResponsable.ModificadoPor = tokenCookie?.UserName ?? "system";
            categoriaResponsable.FechaModificacion = DateTime.Now;

            var response = await _categoriaResponsableService.EliminarCategoriaResponsable(categoriaResponsable);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarUsuariosQuePuedenAtender()
        {
            var response = await _usuarioService.ConsultarTodosLosUsuarios();

            if (response.IsSuccess && response.Response != null)
            {
                // Filtrar usuarios que pueden atender tickets
                // Nota: Para esto necesitas que el objeto Usuario tenga la información del rol
                // o necesitas obtener los roles de cada usuario
                // Por ahora, filtramos por los usuarios que tengan roles que pueden atender
                var usuariosFiltrados = new List<UsuarioDTO>();

                foreach (var usuario in response.Response)
                {
                    // Obtener roles del usuario
                    var rolesResponse = await _rolService.ObtenerRolesPorUsuario(usuario.Id);
                    if (rolesResponse.IsSuccess && rolesResponse.Response != null)
                    {
                        // Verificar si alguno de sus roles permite atender tickets
                        if (rolesResponse.Response.Any(r => r.PuedeAtenderTickets))
                        {
                            usuariosFiltrados.Add(usuario);
                        }
                    }
                }

                response.Response = usuariosFiltrados;
            }

            return JsonConvert.SerializeObject(response);
        }

        #endregion


    }
}