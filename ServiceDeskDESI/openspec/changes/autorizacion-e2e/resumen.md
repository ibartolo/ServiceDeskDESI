# Resumen — Cambio `autorizacion-e2e`

- **Fecha**: 2026-08-18
- **Estado**: ✅ Implementación completa (fases 1–6). ⏳ Pendiente: `sdd-verify` + smoke test (fase 7).
- **Propuesta padre**: `openspec/changes/security-remediation/proposal.md` (Fase 1 — hallazgo CRÍTICO/URGENTE, refs **W2, W3, W5, M2**)
- **Artifact store**: openspec | **Modo**: interactivo | **strict_tdd**: false (sin runner de tests)

---

## 1. Objetivo del cambio

Cerrar la brecha **"Sin autorización real de extremo a extremo"**: `[AllowAnonymous]` de clase, claim `role="user"` hardcodeado, `AllowInsecureHttp`, `ValidateClientAuthentication` ciego, CORS `*`, sin enforcement de `[Authorize(Roles)]`, y permisos solo cosméticos en el MVC.

**No se toca** (quedan en otros cambios de `security-remediation`): contraseñas, secretos en git, tenant isolation/IDOR/`EmpresaId`, trial, info disclosure, sesión/expiración forzada, ni Fases 2/3/4.

---

## 2. Decisiones cerradas con el usuario

1. **Fuente de verdad de permisos** = `RolPaginaAccion` (server-side). `UsuarioPagina` queda SOLO para menús dinámicos.
2. **Token**: identidad real (`ClaimTypes.Name`, `usuarioId`) + roles reales (claims informativos, NO autoritativos). La autorización se resuelve server-side por request. No hay roles estáticos.
3. **MVC**: filtro global + allowlist ("seguro por defecto"), no atributo por acción.
4. **Registro de empresa**: UN nuevo endpoint anónimo `POST api/Empresas/Registrar` que valida + comprueba unicidad + crea, todo server-side, devolviendo un único `ModelResponse`.

---

## 3. Artefactos SDD generados

| Artefacto | Ruta |
|---|---|
| Propuesta | `openspec/changes/autorizacion-e2e/proposal.md` |
| Spec autenticación | `openspec/changes/autorizacion-e2e/specs/autenticacion/spec.md` |
| Spec autorización | `openspec/changes/autorizacion-e2e/specs/autorizacion/spec.md` |
| Diseño | `openspec/changes/autorizacion-e2e/design.md` |
| Tareas | `openspec/changes/autorizacion-e2e/tasks.md` |
| Estado DAG | `openspec/changes/autorizacion-e2e/state.yaml` |

---

## 4. Lo implementado (17 tareas, fases 1–6)

### Fase 1 — Configuración
- `Web.config` + `Web.Release.config` (WebApi y MVC): claves `AllowInsecureHttp`, `AllowedCorsOrigins`, `client_id`, `client_secret`.

### Fase 2 — OAuth / CORS / claims (WebApi)
- `Startup.cs`:
  - `ValidateClientAuthentication` compara `client_id`/`client_secret` contra config → `Rejected()` si no coinciden. **El secret se lee con `context.Parameters.Get("client_secret")`** (no existe `context.ClientSecret`).
  - `GrantResourceOwnerCredentials` emite `ClaimTypes.Name`, `usuarioId` y un `ClaimTypes.Role` por rol (vía `ObtenerRolesPorUsuario`). Eliminado `role="user"`.
  - `AllowInsecureHttp = bool.Parse(...)` por config.
  - Middleware OWIN CORS temprano (origen específico + preflight OPTIONS). Eliminado `Access-Control-Allow-Origin: *`.

### Fase 3 — `PermisoAttribute` WebApi
- `ServiceDeskDESIWebApi/Filters/PermisoAttribute.cs` (nuevo): `ActionFilterAttribute`, ctor `(pagina)` y `(pagina, accion)`. 403 si `ValidarPermisoUsuario` = false. Acciones ∈ {Leer, Crear, Editar, Eliminar, Exportar}.
- Aplicado a **41 acciones de escritura** en todos los controllers.

### Fase 4 — Anonimato acotado + registro
- `AutenticationController`: quitado `[AllowAnonymous]` de clase + `[Authorize]`. Anónimos solo: `autenticar`, `ValidarRecetearContrasenia`, `validarToken`, `restablecerContrasenia`.
- Quitado `[AllowAnonymous]` de `Empresas/List`, `Empresas/Nueva`, `Empresas/NuevaCompleta`, `Relacion/List`, `UsuarioPagina/List`.
- `EmpresaController.Registrar` (anónimo) + `EmpresaService.RegistrarEmpresa` (valida campos, unicidad RFC/correo/nombre/razón, setea trial/vigencia, llama a `GuardarNuevaEmpresaConDatosIniciales`).

### Fase 5 — MVC seguro por defecto
- `ServiceDeskDESIMVC/App_Start/FilterConfig.cs` (nuevo): `AuthenticationFilter` global (`IAuthorizationFilter`) con allowlist de 9 acciones públicas de `Home`.
- `Global.asax.cs`: registra `FilterConfig.RegisterGlobalFilters`.
- `Helpers/FiltersHelper.cs`: retirados `AutenticatedAttribute`/`NoAutenticatedAttribute`.

### Fase 6 — MVC enforcement + registro
- `ServiceDeskDESIMVC/Filters/PermisoAttribute.cs` (nuevo): `ActionFilterAttribute`, redirige a `Home/AccesoDenegado` si no hay permiso.
- `[Permiso]` en escrituras de `UserController`, `TicketController`, `CatalogsController`, `SecurityController`.
- `HttpClientConnection.GetToken` envía `client_id`/`client_secret`.
- `HttpClientConnection.Empresa.cs` + `HomeController.GuardarNuevaEmpresa` → `Empresas/Registrar` (se quitó la validación/dedupe client-side).

---

## 5. Mapeo de páginas (`Pagina.Nombre` — provisto por el usuario)

| Controlador | Página |
|---|---|
| Ticket | `Tickets` |
| Autentication/Usuario | `Usuarios` (perfil → `Mi Perfil`) |
| Activo | `Activos` |
| Area | `Áreas` |
| Sucursal | `Sucursales` |
| Empresa / Compania | `Compañías` |
| Categoria | `Categorías` |
| TipoActivo | `Tipo Activo` |
| Marca | `Marcas` |
| Modelo | `Modelos` |
| Rol | `Roles` |
| Permisos / Pagina / UsuarioPagina | `Permisos` |
| Puesto | `Tipped` (typo real en BD) |
| Persona | `People` (typo real en BD) |
| Relacion | `Responsables por Categoría` |

Páginas de menú (sin escritura): `Dashboard`, `Catálogos`, `Seguridad`.

---

## 6. 🔧 Bug crítico corregido durante la revisión

`PermisoAttribute` (ambos) **derivaba de `AuthorizeAttribute` y leía el `Id` en `OnAuthorization`**, pero en Web API ese método corre **antes del model binding** → `ActionArguments` vacío → todos los save-or-update (`[Permiso("Pagina")]`) habrían dado 403.

**Fix**: ambos atributos ahora derivan de `ActionFilterAttribute` y usan `OnActionExecuting` (post-binding, donde `ActionArguments`/`ActionParameters` ya tienen la entidad). La autenticación (401) la garantiza `[Authorize]` a nivel controller (WebApi) y el filtro global `AuthenticationFilter` (MVC).

Otros fixes de build: `Headers.Set` de OWIN espera `string` (no `string[]`); `context.ClientSecret` no existe (usar `context.Parameters.Get("client_secret")`).

---

## 7. Estado del build

✅ `ServiceDeskDESI.sln` compila: **0 errores, 0 advertencias** (MSBuild VS2022).

---

## 8. Pendiente (mañana)

1. **`sdd-verify`**: revisión estática del código contra los specs.
2. **Smoke test fase 7** (manual, sin runner):

   **7.1 WebApi (Swagger/Postman):**
   - `/token` con client_id/secret correctos → token con roles reales.
   - `/token` con client_id/secret incorrectos → `invalid_client`.
   - Endpoint protegido sin token → 401.
   - Escritura sin permiso → 403.
   - Origen no listado en CORS → sin header CORS.
   - `POST /api/Empresas/Registrar` sin token → crea empresa.

   **7.2 MVC:**
   - Sin sesión, `/Ticket` → redirige a login.
   - Login/registro accesibles sin sesión.
   - Escritura sin permiso → `AccesoDenegado`.

   **7.3 Regresión:** registro de empresa nuevo desde pantalla → funciona.

---

## 9. ⚠️ Puntos a vigilar en el smoke test

1. `client_id`/`client_secret` deben **coincidir** entre `ServiceDeskDESIWebApi/Web.config` y `ServiceDeskDESIMVC/Web.config`.
2. `GetToken` envía `UserName`/`Password` (mayúsculas); OWIN espera `username`/`password` minúsculas → si el login falla, normalizar las claves.
3. `AllowedCorsOrigins` debe incluir la URL exacta del MVC (ej. `http://localhost:PUERTO`).
4. **Dato a corregir luego** (no bloquea): la página `Activos` está **duplicada** en la tabla `Pagina`.
5. Typos pre-existentes: `BaseController` redirige a `~/Home/Autenticacion` (mal escrito); `AsignarTicketAgente` es un stub TODO.

---

## 10. Archivos clave (código)

**WebApi** (`ServiceDeskDESIWebApi/`):
- `App_Start/Startup.cs` (OAuth/CORS/claims)
- `Filters/PermisoAttribute.cs` (nuevo)
- `Controllers/AutenticationController.cs`, `EmpresaController.cs`, `RelacionController.cs`, `UsuarioPaginaController.cs` + todos los controllers de catálogo (41 escrituras con `[Permiso]`)
- `Services/EmpresaService.cs` (`RegistrarEmpresa`)
- `Web.config` + `Web.Release.config`

**MVC** (`ServiceDeskDESIMVC/`):
- `App_Start/FilterConfig.cs` (nuevo)
- `Filters/PermisoAttribute.cs` (nuevo)
- `Global.asax.cs`
- `Helpers/FiltersHelper.cs` (atributos retirados)
- `Controllers/{Home,Ticket,User,Catalogs,Security,Permissions}Controller.cs`
- `DAL/HttpClientConnection.cs` + `HttpClientConnection.Empresa.cs`
- `Services/EmpresaService.cs`
- `Web.config` + `Web.Release.config`
