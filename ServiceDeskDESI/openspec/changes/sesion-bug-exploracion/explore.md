# Exploración: Bug de sesión (dashboard/menús vacíos + "Acceso Denegado" tras inactividad)

- **Cambio**: `sesion-bug-exploracion` (exploración READ-ONLY, no modifica código)
- **Fecha**: 2026-09-01
- **Proyecto**: ServiceDeskDESI (ASP.NET MVC 5 + Web API 2, OWIN/Katana OAuth Bearer, .NET 4.8)
- **Síntoma reportado**: login OK → dashboard OK → el usuario se va a otras pestañas/ventanas unos minutos → al regresar y REFRESCAR el dashboard no carga y los menús quedan vacíos → al refrescar OTRA vez redirige a "Acceso Denegado" → debe cerrar sesión y volver a entrar.

---

## 1. Flujo completo de creación de sesión (paso a paso)

### 1.1 Login (vista → controller)

1. `Scripts/Comun/TempAutentication.js:39` — `$.ajax POST /Home/LogIn` con `{ user, pass }`.
2. `Controllers/HomeController.cs:231` — acción `LogIn(string user, string pass)`:
   - **Paso 1** (`HomeController.cs:241`): `_autenticacionService.AutenticarUsuario(...)` autentica contra el WebApi y devuelve el `Usuario` (Id, EmpresaId, ImagenPerfil…). Si falla, devuelve el mensaje del backend.
   - **Paso 2** (`HomeController.cs:256`): `httpClientConnection.GetToken(user, pass)` obtiene el token OAuth.
3. `DAL/HttpClientConnection.cs:45` — `GetToken(...)` → `TokenAsync<Token>("token", ...)` hace POST al endpoint `/token` del WebApi con `grant_type=password`, `client_id` y `client_secret`.
4. `ServiceDeskDESIWebApi/App_Start/Startup.cs:186` — `GrantResourceOwnerCredentials` valida al usuario (BD) y emite el token con claims (`ClaimTypes.Name`, `usuarioId`, `empresaId`, roles).
5. El WebApi responde con `access_token`, `token_type`, `expires_in` (21600 s) → se deserializa en `Entities/Seguridad/Token.cs`.
6. **`HomeController.cs:268`** — `token.ExpirationDate = DateTime.Now.AddSeconds(token.expires_in);` → **hora local del servidor MVC + 6 h**.
7. `HomeController.cs:272-282` — se arma `TokenCookie { Token, UserID, EmpresaID, UserName, ProfileImage, UserAvatar }` y se llama `SessionHelper.CreateSession(JsonConvert.SerializeObject(tokenCookie))`.

### 1.2 Creación de la cookie de sesión (FormsAuthentication, NO Session[])

`Helpers/SessionHelper.cs:60-74` — `CreateSession`:
- `FormsAuthentication.GetAuthCookie("token", persist: true)`.
- `cookie.Name = FormsAuthentication.FormsCookieName` → **"autentication"** (coincide con el `<forms name>` del Web.config).
- `cookie.Expires = tokenCookie.Token.ExpirationDate` (login + 6 h).
- Descifra el ticket por defecto y crea un `FormsAuthenticationTicket` nuevo con `Expiration = cookie.Expires` (6 h) y **`UserData = JSON del TokenCookie`** (donde vive el `access_token` + `ExpirationDate`).
- Encripta y agrega la cookie a `Response.Cookies`.

> **Importante**: NO se usa `Session[]` / `sessionState` de ASP.NET. Todo el estado de sesión vive en la **cookie de FormsAuthentication** (`UserData`). Confirmado: `grep Session[` no devuelve usos.

### 1.3 Redirección post-login

`Scripts/Comun/TempAutentication.js:59` — `window.location.href = '/Home/Index';` (dashboard).

---

## 2. Validación de sesión en cada request

### 2.1 Filtro global

`App_Start/FilterConfig.cs` — `RegisterGlobalFilters` agrega `new AuthenticationFilter()` (línea 13). Es un `IAuthorizationFilter` "seguro por defecto".

- `PublicActions` (FilterConfig.cs:25-38): `Home.Autentication`, `Home.LogIn`, `Home.RecoverPassword`, `Home.VerAsignacion`, `Home.ValidarToken`, `Home.RestablecerContrasenia`, `Home.ValidarRecetearContrasenia`, `Home.NewCompany`, `Home.GuardarNuevaEmpresa`, `Home.AccesoDenegado`.
- Si la acción NO está en la allowlist → `SessionHelper.EixstSession()` (FilterConfig.cs:61).
- Si `false` → `RedirectToRouteResult` a `Home/Autentication` (login), **no** a AccesoDenegado.

### 2.2 `SessionHelper` (Helpers/SessionHelper.cs)

- `GetSessionUser()` (línea 43): lee `HttpContext.Current.User.Identity` como `FormsIdentity`, obtiene el `Ticket` y deserializa `ticket.UserData` → `TokenCookie`.
- `EixstSession()` (línea 13): `true` **solo si** `GetSessionUser() != null` **y** `token.Token != null` **y** `token.Token.ExpirationDate >= DateTime.Now`.
- `CloseSession()` (línea 38): `FormsAuthentication.SignOut()`.
- `GetDateCenterMexico()` (línea 75): devuelve la hora "Central Standard Time" (usada solo para auditar `CreadoPor/FechaCreacion`, no para expiración).

### 2.3 Dónde nace "Acceso Denegado"

"Acceso Denegado" **NO** lo dispara el filtro de sesión (ese manda a login). Lo disparan **checks de permisos**:

1. `Filters/PermisoAttribute.cs:57-63` (MVC) — si `TienePermiso(...)` es `false` → `RedirectToRouteResult` a `Home/AccesoDenegado`.
2. Redirecciones explícitas en controllers (patrón `permisos == null || !PuedeLeer`):
   - `Controllers/SecurityController.cs:38` y `:70`
   - `Controllers/TicketController.cs:47`
   - `Controllers/UserController.cs:145`
   - `Controllers/CatalogsController.cs` (múltiples: 70, 100, 126, 149, 183, 214, 245, 344, 399, 462, 518)
3. `Home/AccesoDenegado` (HomeController.cs:157) → `Views/Home/AccesoDenegado.cshtml`.

Cadena que convierte un **401 del WebApi** en "Acceso Denegado":
`PermisoAttribute.OnActionExecuting` → `PermisosService.TienePermiso` (`Services/PermisosService.cs:60`) → `_httpClient.ValidarPermisoUsuario` (`DAL/HttpClientConnection.Permisos.cs:20`) → `api/Permisos/Validar` con bearer token → si el WebApi devuelve 401/error, `RequestAsync` devuelve `ModelResponse { IsSuccess=false, Response=default }` (`DAL/HttpClientBase.cs:93-122`) → `TienePermiso` devuelve `false` → redirect `AccesoDenegado`.

---

## 3. Configuración exacta de expiración

| Elemento | Valor | Ubicación |
|---|---|---|
| Expiración token OAuth (WebApi) | `AccessTokenExpireTimeSpan = TimeSpan.FromHours(6)` | `ServiceDeskDESIWebApi/App_Start/Startup.cs:117` |
| `expires_in` devuelto | 21600 s (6 h) | calculado por OWIN, deserializado en `Entities/Seguridad/Token.cs:13` |
| `ExpirationDate` del MVC | `DateTime.Now.AddSeconds(expires_in)` = login + 6 h | `Controllers/HomeController.cs:268` |
| Forms authentication | `<forms name="autentication" cookieless="UseCookies" protection="All"/>` (sin `timeout` → default 30 min; sin `slidingExpiration` → default true; sin `loginUrl`) | `ServiceDeskDESIMVC/Web.config:26-28` |
| Expiración real de la cookie | `cookie.Expires = tokenCookie.Token.ExpirationDate` (6 h fija, persistente) | `Helpers/SessionHelper.cs:67` |
| `machineKey` MVC | **PRESENTE y fija** (SHA1/AES) | `ServiceDeskDESIMVC/Web.config:32` |
| `machineKey` WebApi | **AUSENTE** | `ServiceDeskDESIWebApi/Web.config` (solo `compilation` + `httpRuntime`) |
| `sessionState` | No configurado (default InProc 20 min) — **no se usa** | no existe en ningún Web.config |
| Middleware OWIN MVC | `owin:AutomaticAppStartup = false` (el MVC no arranca OWIN) | `ServiceDeskDESIMVC/Web.config:17` |
| Transform publicar | Ningún `Web.Release/Debug.config` agrega `machineKey` al WebApi | ambos `Web.*.config` |

---

## 4. Por qué el dashboard y los menús quedan vacíos (sin redirect inmediato)

1. Al refrescar, `AuthenticationFilter` → `EixstSession()` es **true** (la cookie Forms se descifra bien gracias a la machineKey fija del MVC, y `ExpirationDate` aún está a futuro). → **NO** redirige a login.
2. `HomeController.Index` (`HomeController.cs:44-76`): `ObtenerRolesPorUsuario(tokenCookie.UserID)` va al WebApi con el bearer token. Si el WebApi responde **401**, `rolResponse.IsSuccess=false` → `esAgente=false` → no carga indicadores → **dashboard sin contenido**.
3. `_Layout.cshtml:52` — `$("#sidebar").empty().load("/Home/MenusUser", ...)`. `MenusUser` (`HomeController.cs:146-155`) → `httpClientConnection.ObtenerPaginasPorUsuario()` → `api/Pagina/List` con bearer token. Si 401 → `paginas` queda vacía → `PartialView(paginas)` con 0 menús → **sidebar vacío**.
4. Ningún JS global maneja el 401 con redirect (ver §6). El DAL devuelve `IsSuccess=false` en silencio, así que la página "carga" pero vacía.

> El `401` del WebApi se da **aunque el MVC crea que la sesión es válida** porque ambos validan cosas distintas: el MVC valida su cookie Forms (machineKey fija) + `ExpirationDate`; el WebApi valida el **bearer token OAuth** (cifrado con el machineKey **del WebApi**).

---

## 5. Causas raíz probables (ordenadas por probabilidad)

### H1 (MÁS PROBABLE) — WebApi sin `machineKey` fija → el bearer token se invalida al reciclar el AppPool del WebApi

- El token OAuth de OWIN/Katana se protege por defecto con `MachineKeyDataProtector`, es decir, **usa el `machineKey` de la aplicación WebApi**. Sin `machineKey` fija en `ServiceDeskDESIWebApi/Web.config`, cada reciclaje del AppPool genera claves nuevas y **todos los bearer tokens emitidos dejan de poderse descifrar** → 401.
- El MVC ya tiene machineKey fija (fix previo del cambio `sesion-expiracion`, memoria #193), por lo que su cookie Forms **sí** sobrevive al reciclaje del MVC; pero el `access_token` que guarda dentro queda inservible si el WebApi recicló.
- El hosting compartido (site4now / `SQL5105.site4now.net`) recicla con frecuencia y por **idle timeout** (IIS default ~20 min). Esto explica "unos minutos" de inactividad.
- Secuencia resultante (encaja 100 % con el síntoma):
  1. Login: token emitido con `machineKey-A` del WebApi.
  2. Inactividad → el AppPool del WebApi recicla → ahora `machineKey-B`.
  3. Refresh del dashboard: MVC válido (cookie OK) → `Index`/`MenusUser` llaman al WebApi con el token viejo → 401 → dashboard/menús vacíos.
  4. Segundo refresh / navegar a una página con `[Permiso]` o check de permisos → 401 en `api/Permisos/Validar` → `TienePermiso=false` / `permisos=null` → **"Acceso Denegado"**.

### H2 — Expiración fija (login+6h) vs slidingExpiration de la cookie → límite duro de 6 h

- `EixstSession()` compara contra `token.Token.ExpirationDate`, que es **fija** (login + 6 h) guardada en el `UserData`.
- Aunque FormsAuthentication tenga `slidingExpiration=true` (default) y renueve la cookie en cada request, el `UserData` **no** se actualiza, así que el valor de expiración del MVC queda clavado en 6 h.
- Consecuencia: tras 6 h el MVC fuerza re-login aunque la cookie siga "viva". No explica el fallo "en minutos", pero es un **bug latente** de desfase semántico (no hay renovación silenciosa del token; `Comun.js` referencia `SessionReport`/`SessionRefresh` que **ya no existen** en `HomeController` → código muerto).

### H3 — Desincronía de zona horaria MVC vs WebApi — **DESCARTADA**

- `ExpirationDate = DateTime.Now + 6h` (local MVC) y la comparación `>= DateTime.Now` usan la misma hora local; el token OAuth expira en UTC a la misma hora absoluta. No hay offset en la comparación.

### H4 — machineKey del MVC no desplegada aún en producción

- La machineKey ya está en el repo (`ServiceDeskDESIMVC/Web.config:32`). Si producción no se ha republicado, el síntoma ORIGINAL (menú con login inyectado) persistiría. Pero el síntoma ACTUAL (menús vacíos + AccesoDenegado, no login inyectado) apunta más a H1. A confirmar con despliegue/logs.

### H5 — Token con expiración corta / `expires_in` mal interpretado — **DESCARTADA**

- `AccessTokenExpireTimeSpan = 6h` confirmado; `expires_in = 21600`; `ExpirationDate = Now + 6h`. No hay token de 30 min.

---

## 6. Otras pestañas/ventanas, localStorage y manejo de 401

- **No hay** uso de `localStorage`/`sessionStorage` para la sesión (grep en `*.js` no arroja resultados). La sesión vive solo en la cookie `autentication` (compartida entre pestañas del mismo navegador).
- **No hay** handler AJAX global de 401 ni redirect automático: `GetMVC`/`PostMVC`/`PostViewMVC` (`Scripts/Comun/Comun.js:3-87`) llaman `callBackResult(e)` en error, sin `window.location`.
- Única redirección explícita en JS: login exitoso → `/Home/Index` (`TempAutentication.js:59`) y logout → `/Home/Index` (`Comun.js:91`).
- `Comun.js:95-138` (`SessionReport`/`SessionRefresh`/`secondPassed` → `window.location.reload()`) es **código muerto**: llama a `/Home/SessionReport` y `/Home/SessionRefresh`, acciones que no existen en `HomeController` actual.

---

## 7. Archivos y líneas clave (resumen)

| Punto | Archivo:Línea |
|---|---|
| Registro filtro global | `ServiceDeskDESIMVC/App_Start/FilterConfig.cs:13` |
| Allowlist + redirect a login | `ServiceDeskDESIMVC/App_Start/FilterConfig.cs:25-68` |
| `EixstSession` (valida `ExpirationDate`) | `ServiceDeskDESIMVC/Helpers/SessionHelper.cs:13-36` |
| `GetSessionUser` (lee cookie Forms) | `ServiceDeskDESIMVC/Helpers/SessionHelper.cs:43-58` |
| `CreateSession` (cookie persistente 6 h) | `ServiceDeskDESIMVC/Helpers/SessionHelper.cs:60-74` |
| Login MVC (autentica + token + sesión) | `ServiceDeskDESIMVC/Controllers/HomeController.cs:231-294` |
| `ExpirationDate = Now + expires_in` | `ServiceDeskDESIMVC/Controllers/HomeController.cs:268` |
| `GetToken` (POST /token) | `ServiceDeskDESIMVC/DAL/HttpClientConnection.cs:45-56` |
| Emisión token + `AccessTokenExpireTimeSpan=6h` | `ServiceDeskDESIWebApi/App_Start/Startup.cs:111-124` |
| `GrantResourceOwnerCredentials` | `ServiceDeskDESIWebApi/App_Start/Startup.cs:186-227` |
| 401 → `ModelResponse IsSuccess=false` | `ServiceDeskDESIMVC/DAL/HttpClientBase.cs:93-122` |
| `TienePermiso` → false si no success | `ServiceDeskDESIMVC/Services/PermisosService.cs:60-68` |
| Redirect AccesoDenegado (permiso) | `ServiceDeskDESIMVC/Filters/PermisoAttribute.cs:57-63` |
| Redirect AccesoDenegado (controllers) | `SecurityController.cs:38,70`; `TicketController.cs:47`; `UserController.cs:145`; `CatalogsController.cs` (varios) |
| Menú por AJAX | `ServiceDeskDESIMVC/Views/Shared/_Layout.cshtml:52` |
| `MenusUser` (lista vacía si 401) | `ServiceDeskDESIMVC/Controllers/HomeController.cs:146-155` |
| `<forms name="autentication">` | `ServiceDeskDESIMVC/Web.config:26-28` |
| `machineKey` MVC (presente) | `ServiceDeskDESIMVC/Web.config:32` |
| `machineKey` WebApi (AUSENTE) | `ServiceDeskDESIWebApi/Web.config` |

---

## 8. Datos a confirmar con el usuario

1. **Tiempo exacto** entre el login y el fallo (¿> 20 min? ¿5 min? ¿> 6 h?). Si es ~20 min, refuerza H1 (idle timeout del AppPool).
2. ¿Ocurre **también** si se queda quieto en la **misma pestaña** sin abrir otras ventanas? (para descartar multi-pestaña como causa).
3. Precisar el síntoma visual: ¿**menú vacío** (sidebar sin opciones), **página en blanco**, o **formulario de login inyectado** dentro del sidebar? Cada uno apunta a una causa distinta.
4. ¿El "Acceso Denegado" aparece al refrescar el **mismo dashboard** o al refrescar **otra pestaña/página** (Tickets, Usuarios, Roles, Catálogos)? Si es otra página con `[Permiso]`, confirma H1.
5. ¿Está desplegada la última versión (con `machineKey` en el MVC)? Verificar `Web.config` publicado.
6. Solicitar **logs del WebApi** (`App_Data/logs/log-*.txt`) en el momento del fallo: buscar 401 en `api/Pagina/List`, `api/Permisos/Validar` o mensajes de reinicio/inicio del WebApi.
7. ¿Se reproduce en **dos navegadores/dispositivos** distintos o solo en uno? (descarta caché/cookies corruptas del navegador).

---

## 9. Hallazgo sorprendente / relevante

- El arreglo previo (`sesion-expiracion`, memoria Engram #193) solo puso `machineKey` en el **MVC**, no en el **WebApi**. Como el token OAuth se cifra con el machineKey **del WebApi**, la mitigación fue incompleta: protegió la cookie Forms pero dejó el bearer token expuesto a los reciclajes del AppPool del WebApi. Es la pieza que explica por qué el síntoma "cambió" de "login inyectado" a "menús vacíos + Acceso Denegado".
- El token OAuth expira en **6 h** (no 30 min), así que NO es expiración natural del token; el fallo "en minutos" es consistente con reciclaje/idle del AppPool del WebApi.
- No existe renovación silenciosa (refresh token) ni handler de 401; el cliente MVC asume que un 401 del WebApi es un error de negocio y sigue pintando la página vacía.
