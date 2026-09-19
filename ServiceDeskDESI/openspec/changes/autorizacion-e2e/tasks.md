# Tasks: Autorización de Extremo a Extremo

## Phase 1: Configuración (fundación)

- [x] 1.1 `ServiceDeskDESIWebApi/Web.config`: añadir `appSettings` `AllowInsecureHttp`, `AllowedCorsOrigins`, `client_id`, `client_secret`.
- [x] 1.2 `ServiceDeskDESIWebApi/Web.Release.config`: transform que fija `AllowInsecureHttp=false` y origen CORS de producción.

## Phase 2: OAuth / CORS / claims en WebApi

- [x] 2.1 `App_Start/Startup.cs`: `ValidateClientAuthentication` compara `ClientId`/`ClientSecret` contra config; mismatch → `Rejected()`.
- [x] 2.2 `Startup.cs`: `GrantResourceOwnerCredentials` emite `ClaimTypes.Name`, `usuarioId` y un claim `Role` por rol vía `ObtenerRolesPorUsuario`; elimina `role="user"`.
- [x] 2.3 `Startup.cs`: `AllowInsecureHttp = bool.Parse(AppSettings["AllowInsecureHttp"])`; eliminar header `Access-Control-Allow-Origin: *`.
- [x] 2.4 `Startup.cs`: middleware OWIN CORS temprano que lee `AllowedCorsOrigins` (coma-separado), responde origen específico y maneja `OPTIONS` preflight.

## Phase 3: PermisoAttribute WebApi

- [x] 3.1 Crear `ServiceDeskDESIWebApi/Filters/PermisoAttribute.cs`: hereda `AuthorizeAttribute` (`System.Web.Http`), ctor `(string pagina, string accion)`; 401 sin token, 403 si `ValidarPermisoUsuario` es `false`.
- [x] 3.2 Aplicar `[Permiso("Pagina","Accion")]` a acciones de escritura del WebApi (acciones ∈ {Leer, Crear, Editar, Eliminar, Exportar}).

## Phase 4: Anonimato acotado + registro empresa

- [x] 4.1 `AutenticationController.cs`: quitar `[AllowAnonymous]` de clase; anónimas solo `autenticar`, `ValidarRecetearContrasenia`, `validarToken`, `restablecerContrasenia`; `[Permiso]` en escrituras.
- [x] 4.2 Quitar `[AllowAnonymous]` de `Empresas/List`, `Relacion/List`, `UsuarioPagina/List`, `Empresas/Nueva`, `Empresas/NuevaCompleta`.
- [x] 4.3 `EmpresaController.cs` + `EmpresaService.cs`: nuevo endpoint anónimo `POST api/Empresas/Registrar` que valida campos y unicidad (RFC/correo/nombre comercial/razón social) y crea empresa + datos iniciales vía SPs, devolviendo un `ModelResponse`.

## Phase 5: MVC seguro por defecto

- [x] 5.1 Crear `ServiceDeskDESIMVC/App_Start/FilterConfig.cs`: `IAuthorizationFilter` global con allowlist estática `HashSet<"Controller.Action">` (9 acciones de `Home`).
- [x] 5.2 `Global.asax.cs`: registrar `FilterConfig` (filtro global).
- [x] 5.3 `Helpers/FiltersHelper.cs`: retirar `AutenticatedAttribute`/`NoAutenticatedAttribute`.

## Phase 6: MVC enforcement + registro

- [x] 6.1 Crear `ServiceDeskDESIMVC/Filters/PermisoAttribute.cs` (atributo MVC) que deniega → redirect `Home/AccesoDenegado`.
- [x] 6.2 `UserController.cs` y acciones de escritura: añadir `[Permiso("Pagina","Accion")]`.
- [x] 6.3 `DAL/HttpClientConnection.cs`: `GetToken` envía `client_id`/`client_secret`; `HttpClientConnection.Empresa.cs` + `HomeController.GuardarNuevaEmpresa` apuntan a `Empresas/Registrar`.

## Phase 7: Smoke test manual (sin runner)

- [ ] 7.1 Swagger/Postman: token con roles reales, cliente inválido rechazado, 401 sin token, 403 sin permiso, origen CORS no autorizado.
- [ ] 7.2 Navegación MVC: allowlist pública accesible, acción no listada sin sesión → login, escritura sin permiso → `AccesoDenegado`.
- [ ] 7.3 Regresión: registro de empresa pre-login funciona vía `Empresas/Registrar`.
