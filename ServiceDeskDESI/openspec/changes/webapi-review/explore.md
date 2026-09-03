# Revisión de Seguridad y Arquitectura — ServiceDeskDESIWebApi

- **Change**: `webapi-review`
- **Fase**: explore (revisión de código/arquitectura, solo lectura)
- **Fecha**: 2026-08-18
- **Origen**: volcado desde Engram (`sdd/webapi-review/explore`)

## Estado actual

ASP.NET Web API 2 (.NET 4.8) + OWIN/Katana OAuth Bearer backend. 3 proyectos: WebApi, Entities, MVC. El acceso a datos es ADO.NET crudo (`SqlConnection`/`SqlCommand` + stored procedures) vía `DAL/BaseDbWrapper.cs` y 22 partials `DbWrapper.*.cs`. Los Controllers (`Controllers/*`) llaman a Services (`Services/*`) que llaman a DbWrapper. Auth = OAuth `Resource Owner Password Credentials` en `/token` (Startup.cs `SimpleAuthorizationServerProvider`). EF Core 3.1.0, Microsoft.Extensions.DI/Configuration/Logging/Caching están referenciados pero NO usados en ningún lado (no hay DbContext, no hay contenedor DI). Swagger configurado dos veces (SwaggerConfig.cs vía PreApplicationStartMethod + Startup.cs).

## Hallazgos CRÍTICOS

1. **Secretos de producción en texto plano en git** — `ServiceDeskDESIWebApi/Web.config`: connection string `cCon` con password SQL (`..._admin;Password=Ifbc121290.01`) y app password SMTP de Gmail `passEmail=veow euow xmnl cefa`. Web.Release.config solo quita el atributo debug, no transforma la connection string. Web.config está trackeado en git.
2. **`AutenticationController` es `[AllowAnonymous]` a nivel de clase** (`Controllers/AutenticationController.cs:13`) → los endpoints de gestión de usuarios (listar, obtener por id, guardar, eliminar, actualizar perfil, guardar admin) son llamables SIN token. Los campos de auditoría `CreadoPor`/`ModificadoPor` se toman del body del request, no de la identidad autenticada.
3. **No hay autorización real** — OAuth emite un claim hardcodeado `role="user"` (Startup.cs:169); `AllowInsecureHttp=true` (Startup.cs:96); no hay `[Authorize(Roles=...)]` en ningún lado; el sistema de permisos (RolPaginaAccion, `ValidarPermisoUsuario` en DAL/DbWrapper.Permisos.cs) nunca es forzado por ningún filtro — solo lo usa MVC para ocultar menús. Cualquier token válido = acceso total.
4. **Passwords reversibles + default hardcodeado** — `Helpers/Cryptography.cs` usa Rijndael con claves hardcodeadas (`PasswordHash="P@@Sw0rd"`), no hashing. `Services/EmpresaService.cs:341` crea cada admin nuevo con `Contrasena = Cryptography.Encrypt("Admin123!")`. Longitud mínima de password 6. El password encriptado se devuelve a los clientes (AutenticarUsuario/ObtenerUsuarios retornan el Usuario completo incl. Contrasena).
5. **`ValidateClientAuthentication` hace `context.Validated()` a ciegas** (Startup.cs:144-147) — sin client id/secret; cualquier llamador obtiene un token. CORS `Access-Control-Allow-Origin: *` (Startup.cs:151).

## Hallazgos IMPORTANTES

6. Exposición anónima de datos: `EmpresaController.List` (`[AllowAnonymous]`), `RelacionController.List`, `UsuarioPaginaController.List` exponen datos de empresas/usuario-página sin auth.
7. Dependencias muertas/sin uso: EF Core + Microsoft.Extensions.* todos referenciados pero sin usar. El csproj de WebApi hace ProjectReference al frontend MVC (layering incorrecto; el dll de MVC se copia al bin de WebApi).
8. Config de Swagger duplicada (SwaggerConfig.cs + Startup.cs) + dos HttpConfigurations (GlobalConfiguration vs OWIN-local) → riesgo de rutas duplicadas.
9. `throw ex;`/`throw EX;` resetean los stack traces (BaseDbWrapper.cs:71,134,223; EmailHelper.cs:85).
10. Mapeo de filas por reflection (`LlenarEntidad<T>`, `ObtenerParametrosSQL`) — lento + frágil (hack del prefijo "Item" con Enum.Parse).
11. Sin paginación en los endpoints de listado.
12. Sin manejador global de excepciones (el middleware OWIN loguea+relanza → HTML 500). Todas las respuestas HTTP 200 con flag ModelResponse.IsSuccess.
13. Validación manual con `if` duplicada por servicio; sin DataAnnotations/ModelState; `[FromBody]` null no protegido → NRE con body null.
14. Duplicación masiva de mapeo de readers en DbWrapper.Ticket.cs (5 copias) y DbWrapper.Autenticacion.cs.

## NICE-TO-HAVE

15. Typos: `EnvioEmaiil`, `EixstSession`, `MapearPorpiedades`, `ValidarRecetearContrasenia`, `Autentication`; `josepruebaController` muerto (MVC); `ServiceDeskDESIWebApi - copia.csproj` suelto.
16. `UsuarioPaginaController.EliminarUsuarioPagina` no tiene atributo de ruta/verbo → código muerto inalcanzable.
17. Los endpoints DELETE reciben body en vez de `/{id}`; rutas mixtas ES/EN; `POST api/Empresas/RFC` debería ser GET.
18. MVC `HttpClientBase.RequestAsync` no limpia los headers cuando el token está vacío → puede filtrar Authorization obsoleto; typo de content-type `application/x-www-url-formencoded` en `GetToken`.
19. MVC `HomeController.GuardarNuevaEmpresa` carga TODAS las empresas del lado cliente para deduplicar RFC/correo (O(n) + race).
20. Sin tests, sin CI. `debug=true` en ambos Web.configs; MVC `customErrors mode=Off`.

## Fortalezas

- Solo stored procedures parametrizadas (no se encontró SQL inline / superficie de SQL injection).
- Serilog file sink con roll diario + retención + middleware de error OWIN + Log.CloseAndFlush al finalizar.
- Entidades centralizadas en proyecto compartido; campos de auditoría BaseObject consistentes; patrón de borrado lógico (soft delete).
- TransactionScope usado para escrituras multi-paso (registro de empresa, permisos masivos).
- Attribute routing + Swagger con XML comments + JSON camelCase.
- bin/obj excluidos de git (.gitignore presente).

## Siguientes pasos recomendados (top impacto)

1. Sacar los secretos de Web.config (env vars/secret store) + rotar YA las credenciales DB/SMTP filtradas.
2. Corregir autorización: quitar `[AllowAnonymous]` a nivel de clase de AutenticationController (solo endpoints autenticar/recuperar/token anónimos), forzar roles/permisos server-side vía un AuthorizeAttribute.
3. Reemplazar criptografía reversible por password hashing (PBKDF2/bcrypt) + dejar de devolver Contrasena en las respuestas.
4. Deshabilitar AllowInsecureHttp + validar clientes; añadir rate limiting en /token.
5. Añadir manejador global de excepciones + códigos HTTP correctos + validación ModelState.
