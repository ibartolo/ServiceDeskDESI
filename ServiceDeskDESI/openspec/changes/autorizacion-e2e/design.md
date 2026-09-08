# Design: Autorización de Extremo a Extremo

## Technical Approach

Cerrar la brecha en dos capas: (1) WebApi — OAuth/CORS/claims reales en `Startup.cs` + atributo `PermisoAttribute` que fuerza `ValidarPermisoUsuario` en escrituras; (2) MVC — filtro global "seguro por defecto" con allowlist + `PermisoAttribute`. Todo server-side contra `RolPaginaAccion`; `UsuarioPagina` solo para menús.

## Architecture Decisions

### Decision: Registro de clientes OAuth vía Web.config
**Choice**: `appSettings` con `client_id`/`client_secret` (cliente único = MVC). `ValidateClientAuthentication` compara `context.ClientId`/`context.ClientSecret` contra config; mismatch → `Rejected()`.
**Alternatives considered**: tabla DB de clientes (sobredimensionado); lista hardcodeada (no desplegable por entorno).
**Rationale**: no hay tabla de clientes; config se transforma por entorno. Requiere añadir `client_id/secret` a `HttpClientConnection.GetToken()` (hoy no los envía).

### Decision: Roles como claims múltiples, informativos
**Choice**: `GrantResourceOwnerCredentials` llama `DbWrapper.ObtenerRolesPorUsuario(context.UserName)` y emite un `Claim(ClaimTypes.Role, rol.Nombre)` por rol, más `Claim("usuarioId", usuario.Id)`. Un claim `role` por rol (no coma-joined).
**Alternatives considered**: un claim con valores unidos por coma (rompe `User.IsInRole`); omitir roles.
**Rationale**: patrón estándar de `ClaimsIdentity`; claims informativos, autorización SIEMPRE contra BD.

### Decision: HTTPS y CORS dirigidos por configuración
**Choice**: `AllowInsecureHttp = bool.Parse(AppSettings["AllowInsecureHttp"])`. CORS: middleware OWIN temprano (junto al middleware de log existente) que lee `AppSettings["AllowedCorsOrigins"]` (coma-separado) y responde con el origen específico si está en la lista + maneja `OPTIONS` preflight. Eliminar el header `Access-Control-Allow-Origin: *` hardcodeado de `GrantResourceOwnerCredentials`.
**Alternatives considered**: `Microsoft.Owin.Cors` (dependencia nueva innecesaria); lógica solo en el token endpoint (no cubre preflight de endpoints REST).
**Rationale**: `Web.config` (dev) con orígenes localhost; `Web.Release.config` fija `AllowInsecureHttp=false` y origen producción.

### Decision: PermisoAttribute en WebApi
**Choice**: `PermisoAttribute : AuthorizeAttribute` (de `System.Web.Http`), constructor `(string pagina, string accion)`. En `OnAuthorization`, tras validar token (base), obtiene `principal.Identity.Name`, llama `new PermisosService().ValidarPermisoUsuario(usuario, pagina, accion)`; si `Response != true` → `context.Response = 403`.
**Alternatives considered**: mapeo convención controller/action→página (frágil: nombres no 1:1 — "Compañías"→`EmpresaController`, "Tipped"/"People" son typos); `ActionFilterAttribute` (corre después del binding, no ideal para 401/403).
**Rationale**: el atributo explícito `[Permiso("Tickets","Eliminar")]` evita convenciones frágiles; `pagina` mapea a `Pagina.Nombre` vía `ObtenerPaginaPorNombre` (ya en el service). Acciones = `Leer`/`Crear`/`Editar`/`Eliminar`/`Exportar` (verificar contra SP).

**Refinamiento (post-design, por patrón `GuardarOActualizar*`)**: los endpoints save-or-update usan `[Permiso("Pagina")]` SIN acción — el atributo auto-detecta `Crear` (entidad `Id==0`) vs `Editar` (`Id>0`) según la entidad bound, y `Eliminar` para DELETE. Constructor sobrecargado: `PermisoAttribute(pagina)` y `PermisoAttribute(pagina, accion)`.

### Decision: Filtro global MVC + allowlist estática
**Choice**: nuevo `App_Start/FilterConfig.cs` con un `IAuthorizationFilter` global (registrado en `Global.asax.cs`). Allowlist estática `HashSet<"Controller.Action">` de públicas; el resto exige `SessionHelper.EixstSession()` (sin sesión → redirect `Home/Autentication`). Escrituras se marcan con `[Permiso("Pagina","Accion")]` (atributo MVC paralelo) → denegación redirect `Home/AccesoDenegado`.
**Alternatives considered**: atributo de "pública" por acción (rechazado en propuesta — no es "seguro por defecto"); lista en `Web.config` (menos legible).
**Rationale**: allowlist estática es auditable y centralizada. Allowlist: `Home.Autentication`, `Home.LogIn`, `Home.RecoverPassword`, `Home.ValidarToken`, `Home.RestablecerContrasenia`, `Home.ValidarRecetearContrasenia`, `Home.NewCompany`, `Home.GuardarNuevaEmpresa`, `Home.AccesoDenegado`. Retirar `AutenticatedAttribute`/`NoAutenticatedAttribute` (subsumidos).

## Data Flow

```
MVC GetToken(client_id/secret) ──► /token ──► ValidateClientAuthentication(config) ──► GrantResourceOwnerCredentials ──► AutenticarUsuario + ObtenerRolesPorUsuario → claims
Acción escritura ──► [Permiso("Pagina","Accion")] ──► ValidarPermisoUsuario ──► SP RolPaginaAccion → 403 si denegado
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `ServiceDeskDESIWebApi/App_Start/Startup.cs` | Modify | Clientes, claims, HTTPS/CORS por config |
| `ServiceDeskDESIWebApi/Filters/PermisoAttribute.cs` | Create | Atributo autorización WebApi |
| `ServiceDeskDESIWebApi/Controllers/AutenticationController.cs` | Modify | Quitar `[AllowAnonymous]` de clase; `[Permiso]` en escrituras |
| `ServiceDeskDESIWebApi/Controllers/EmpresaController.cs` | Modify | Quitar `[AllowAnonymous]` suelto; añadir endpoint anónimo único `Registrar` |
| `ServiceDeskDESIWebApi/Services/EmpresaService.cs` | Modify | Unicidad RFC/correo/nombre + registro server-side → `ModelResponse` |
| `ServiceDeskDESIWebApi/Controllers/RelacionController.cs` | Modify | Quitar `[AllowAnonymous]` de `List` |
| `ServiceDeskDESIWebApi/Controllers/UsuarioPaginaController.cs` | Modify | Quitar `[AllowAnonymous]` de `List` |
| `ServiceDeskDESIWebApi/Web.config` + `Web.Release.config` | Modify | `AllowInsecureHttp`, `AllowedCorsOrigins`, `client_id/secret` |
| `ServiceDeskDESIMVC/App_Start/FilterConfig.cs` | Create | Filtro global + allowlist |
| `ServiceDeskDESIMVC/Global.asax.cs` | Modify | Registrar `FilterConfig` |
| `ServiceDeskDESIMVC/Helpers/FiltersHelper.cs` | Modify | Retirar atributos antiguos |
| `ServiceDeskDESIMVC/Filters/PermisoAttribute.cs` | Create | Atributo autorización MVC |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.cs` | Modify | `GetToken` envía `client_id/secret`; `GuardarNuevaEmpresa` → `Registrar` |
| `ServiceDeskDESIMVC/Controllers/UserController.cs` | Modify | Escrituras con `[Permiso]` |
| `ServiceDeskDESIMVC/Controllers/HomeController.cs` | Modify | `GuardarNuevaEmpresa` consume el endpoint `Registrar` |

## Interfaces / Contracts

```csharp
// WebApi (System.Web.Http.AuthorizeAttribute)
[Permiso("Tickets", "Eliminar")]  // pagina=Página.Nombre, accion∈{Crear,Editar,Eliminar,Leer,Exportar}
public class PermisoAttribute : AuthorizeAttribute { /* ctor(pagina, accion); 403 si ValidarPermisoUsuario=false */ }
```

Config keys: `AllowInsecureHttp`, `AllowedCorsOrigins`, `client_id`, `client_secret`.

## Testing Strategy

| Layer | Qué probar | Enfoque |
|-------|-----------|---------|
| Smoke WebApi | Roles reales; cliente inválido rechazado; 401/403 | Swagger/Postman (`strict_tdd=false`, sin runner) |
| Smoke MVC | Allowlist pública; redirect login; denegación escritura | Navegación manual |
| Regresión | Registro empresa pre-login funciona | Checklist por endpoint |

## Migration / Rollout

Sin migración de datos. Despliegue por transform de config (release). Rollout por entorno: dev mantiene `AllowInsecureHttp=true`; release activa HTTPS estricto y CORS productivo. Verificar matriz rol/página antes de publicar.

## Decisiones resueltas (post-design)

1. **Registro de empresa pre-login**: nuevo endpoint anónimo único `POST api/Empresas/Registrar` que orquesta server-side (validación de campos, unicidad RFC/correo/nombre, creación empresa + datos iniciales vía SPs) y devuelve un único `ModelResponse`. El MVC llama solo a este. Se elimina el anonimato de `Empresas/List`, `Relacion/List`, `UsuarioPagina/List`, `Empresas/Nueva` y `Empresas/NuevaCompleta`.
2. **Valores `@Accion`**: `Leer`, `Crear`, `Editar`, `Eliminar`, `Exportar` (confirmado en SP `ValidarPermisoUsuario`).
3. **`List` de dropdowns**: no se requieren en vistas anónimas; quedan protegidas por token.
