# Exploración — Módulo "Configuración de Empresa" (días/horario laboral + logotipo)

- **Change**: `configuracion-empresa`
- **Fase**: explore (solo lectura)
- **Fecha**: 2026-09-07
- **Fuentes**: `openspec/basededatosservicedesk.txt` (esquema + 107 SPs), `openspec/changes/metricas-desempeno/explore.md` (Anexo A — horario laboral, reutilizado), `openspec/changes/archive/2026-08-26-personal-administracion/explore.md` (seed real de `Pagina`), código `ServiceDeskDESIEntities` / `ServiceDeskDESIWebApi` / `ServiceDeskDESIMVC`.

---

## Resumen ejecutivo

No hay que inventar nada estructural: se replica el patrón MVC → HttpClient → WebApi → `DbWrapper` → SP. El cambio añade **una página de menú nueva e independiente** `ConfiguracionEmpresa` (llave sin acento, `NombreVisible='Configuración de Empresa'`) donde el rol autorizado **ve** los datos generales de su `Empresa` (tenant) y **edita** dos criterios: (a) **días y horario laboral** (tabla hija nueva `EmpresaHorarioLaboral`, ya diseñada en el Anexo A de `metricas-desempeno`) y (b) **logotipo** de la empresa (nueva columna `Empresa.LogoUrl` + archivo en disco bajo MVC, espejando el patrón de foto de perfil). No hay eliminación. El footer "by DESi" se añade como markup **estático** en `_Layout.cshtml` (hoy no existe ningún footer). Acceso por `RolPaginaAccion` (PuedeLeer/PuedeEditar); seed por defecto solo al rol **Administrador** (los demás roles se conceden luego desde la UI de Permisos existente).

---

## 1. Estado actual (cómo funciona hoy)

- **Arquitectura** (confirmada): MVC 5 (.NET 4.8) → `HttpClientConnection` → WebApi 2 (OWIN/OAuth2) → `DbWrapper` (ADO.NET) → SPs. `ModelResponse<T>` (`IsSuccess`, `Message`, `Response`). `DbWrapper.LlenarEntidad<T>` mapea por nombre de columna (case-insensitive) con `Convert.ChangeType`; `ObtenerParametrosSQL<T>` por reflexión (`DbWrapper.cs:41-90`).
- **Auth/tenant**: token OAuth2 emite claims `Name` (NombreUsuario), `usuarioId`, `empresaId`, `ClaimTypes.Role`. MVC guarda `TokenCookie` (en `ServiceDeskDESIEntities/Seguridad/TokenCookie.cs`: `Token, UserID, EmpresaID, UserName, ProfileImage, UserAvatar`) en cookie FormsAuth. WebApi `BaseController.ObtenerEmpresaIdDesdeClaim()` lee el claim `empresaId`.
- **Menú**: `_Layout.cshtml:52` → `$("#sidebar").load("/Home/MenusUser")` → `ObtenerPaginasPorUsuario` → `Views/Home/MenusUser.cshtml` (único render; muestra `NombreVisible ?? Nombre`, ícono `<i class="fas @menu.Logo">`).
- **Permisos**: `[Permiso("Pagina","Accion")]` MVC (`Filters/PermisoAttribute.cs`) y WebApi (`Filters/PermisoAttribute.cs`). Ambos llaman `PermisosService.TienePermiso`/`ValidarPermisoUsuario` → SP `ObtenerPaginaPorNombre` (`WHERE Nombre = @Nombre AND Estatus = 1`, `basededatosservicedesk.txt:4337-4346`) → SP `ValidarPermisoUsuario` (`:5186-5217`), que mapea la acción a la columna de `RolPaginaAccion` (Leer→PuedeLeer, Editar→PuedeEditar, Crear→PuedeCrear, Eliminar→PuedeEliminar, Exportar→PuedeExportar) con `MAX(...)` sobre todos los roles del usuario (basta que UN rol tenga el flag).

---

## 2. Respuestas a las preguntas (Q1–Q9)

### Q1. Datos de la empresa (display)

- **`dbo.Empresa`** (`basededatosservicedesk.txt:201-225`) — columnas: `Id bigint IDENTITY` (PK, es el tenant), `NombreComercial`, `RazonSocial`, `RFC`, `Responsable`, `Direccion` (NOT NULL); `Ciudad`, `Estado`, `CodigoPostal`, `Telefono` (NULL); `CorreoContacto`, `FechaVigenciaInicio`, `FechaVigenciaFin`, `EsPeriodoPrueba bit` (default 1), `Estatus bit`, auditoría (`CreadoPor`, `FechaCreacion`, `ModificadoPor`, `FechaModificacion`). **No existe columna Logo/Imagen ni en `Empresa` ni en `Compania`.**
- **`dbo.Compania`** (`:179-194`): `Nombre`, `Acronimo`, `RFC`, `Direccion` + auditoría + `Estatus`. Catálogo GLOBAL de "Razón Social" (CRUD propio `CompaniaController`/`DbWrapper.Compania.cs`, UI `Views/Catalogs/Company.cshtml`), **sin** `EmpresaId`. No confundir con `Empresa`.
- POCO: `ServiceDeskDESIEntities/Catalogos/Empresa.cs` (todos los campos salvo `LogoUrl`, que no existe). `Compania` en `Catalogos/Compania.cs`.
- **Qué mostraría hoy "ver los datos de la empresa"**: NombreComercial, RazonSocial, RFC, Responsable, Dirección (Calle/Ciudad/Estado/C.P.), Teléfono, Correo de contacto, y vigencia (FechaVigenciaInicio/Fin, EsPeriodoPrueba). Son los campos de `Empresa`; `EmpresaService.ObtenerEmpresaPorId` ya los devuelve (`WebApi/Services/EmpresaService.cs:24-51` → SP `ObtenerEmpresaPorId`), y MVC `Services/EmpresaService.cs:20-28` lo desenvuelve a `Empresa`.
- **`Home/Configuration` hoy** (`HomeController.Configuration` `:162-196` + `Views/Home/Configuration.cshtml`): (1) card "Tema" (light/dark, guarda en cookie), y (2) si `esJefeArea` (`Area.UsuarioResponsableId == UserID`), card "Licencia" de solo lectura con `NombreComercial`, `diasRestantes`, `EsPeriodoPrueba`. No es entrada del menú `Pagina` (se llega por dropdown del usuario, `_Layout.cshtml:212`). **La página nueva debe mostrar un superconjunto** (todos los campos de `Empresa`) sin duplicar la card de licencia: ésta queda en `Home/Configuration` (o se puede omitir; la licencia no es parte del alcance de edición).

### Q2. Logo / imaginería DESI por defecto

**No existe ningún archivo de imagen del logo DESI.** El "logo" es ícono FontAwesome + texto:

| Lugar | Archivo:línea | Contenido actual |
|---|---|---|
| Sidebar (header) | `Views/Home/MenusUser.cshtml:3-7` | `<i class="fas fa-headset">` + `<h3>Service Desk DESI</h3>` + `<p>Sistema de Gestión de Tickets</p>` |
| Login (header) | `Views/Home/Autentication.cshtml:34-38` | `<i class="fas fa-headset"></i>` + `<h2>Service Desk DESI</h2>` |
| `<title>` | `Views/Shared/_Layout.cshtml:6` | `Service Desk DESI - @ViewBag.Title` |
| Navbar (avatar usuario) | `_Layout.cshtml:193-205` | `usuario.ProfileImage` (imagen) o `usuario.UserAvatar` (iniciales) |

- **Carpetas de estáticos hoy**: `ServiceDeskDESIMVC/Content/` (solo `datatables/i18n/es-ES.json`), `ServiceDeskDESIMVC/Uploads/Perfiles/{userId}/{guid}.ext` (fotos de perfil), `ServiceDeskDESIWebApi/Evidencias/{empresaId}/{ticketId}/{guid}.ext` (evidencias). No hay carpeta de logos.
- **Dónde reemplazar por logo de empresa**: en la práctica, **solo el sidebar** (`MenusUser.cshtml:4`) es el punto visible para una empresa autenticada. Sustituir `<i class="fas fa-headset">` por `<img src="@logo" ...>` con fallback al ícono. El login es pre-auth (sin `EmpresaId`) → mantiene el ícono/logo por defecto. No hay plantillas de correo con imagen (el `Template/Template_AltaEmpresa.html` es texto). Recomendación: per-company logo se muestra en el sidebar; login usa default.

### Q3. Pipeline de subida del logo

**Dos precedentes de subida coexisten:**

1. **Evidencias (WebApi-side, "canónico")** — `EvidenciaController.Guardar` (multipart, `[Permiso("Tickets","Leer")]`) → `EvidenciaService.GuardarEvidencias(ticketId, files, usuario, empresaId)`: valida extensión (`EvidenciasExtensionesPermitidas`) y peso (`EvidenciasMaxTamanoMB`) desde `WebApi/Web.config` (`:32-34`), escribe en `~/Evidencias/{empresaId}/{ticketId}/{guid}.ext` (`HostingEnvironment.MapPath`), guarda `RutaArchivo` (relativa) en BD. Se sirve vía descarga por API.
2. **Foto de perfil (MVC-side)** — `UserController.ActualizarPerfilUsuario` (`:86-134`, `HttpPostedFileBase`): guarda en `~/Uploads/Perfiles/{usuario.Id}/{guid}.ext` (MVC), setea `usuario.ImagenPerfil = "/Uploads/Perfiles/.../{file}"`, lo persiste en `Usuarios.ImagenPerfil` vía `ActualizarPerfilUsuario`. Se sirve como **archivo estático** de MVC. **No valida MIME/peso hoy** (sin appSettings).

**Recomendación de almacenamiento (Q3):** opción **(a) archivo en disco + ruta relativa en BD**, espejando el patrón de **foto de perfil (MVC)**, que es el consistente para una imagen servida inline (no de descarga):

- Carpeta: `ServiceDeskDESIMVC/Uploads/Logos/{empresaId}/{guid}.ext` (crear subcarpeta por empresa).
- URL servida: `/Uploads/Logos/{empresaId}/{guid}.ext` (servida por el manejador de estáticos de MVC, igual que las fotos de perfil).
- Persistir la ruta relativa en una nueva columna `Empresa.LogoUrl NVARCHAR(500) NULL`.
- Añadir validación explícita + claves appSettings (espejo de `Evidencias*` pero en `ServiceDeskDESIMVC/Web.config`, ya que la subida es MVC-side): `LogoEmpresaMaxTamanoMB` (default `2`), `LogoEmpresaExtensionesPermitidas` (default `png,jpg,jpeg`).
- Opciones descartadas: **(b) varbinary/image en BD** — sin precedente en el repo y peor para servir inline; **(c)** ya descrito. La subida del binario NO debe pasar por WebApi (evita round-trip de binario y reutiliza el patrón de foto de perfil); solo la persistencia del path (`Empresa.LogoUrl`) va por WebApi → SP con `[Permiso("ConfiguracionEmpresa","Editar")]`.

### Q4. Footer

- **`Views/Shared/_Layout.cshtml` NO tiene footer.** Termina en `@RenderBody()` (`:223`) → cierre de `main-content`/`content`/`wrapper` → `</body></html>` (`:222-228`). Es el único layout compartido. (`Autentication.cshtml` y `NewCompany.cshtml` son standalone con `Layout = null` y tienen `login-footer`/`footer-links` propios, no un footer "by DESi".)
- **Dónde añadirlo**: en `_Layout.cshtml`, inmediatamente después de `@RenderBody()` (línea 223) y antes de cerrar `.main-content`, añadir un `<footer>` estático. Recomendación mínima (español, estático):
  ```html
  <footer class="app-footer">
      <span>© @DateTime.Now.Year Service Desk by <strong>DESi</strong></span>
      <a href="/Home/Ayuda">Ayuda</a> · <a href="#">Términos y Condiciones</a> · <a href="#">Política de Privacidad</a>
  </footer>
  ```
  No se toca el menú ni los permisos (markup estático, no editable desde la página).

### Q5. Horario laboral (reutiliza Anexo A de `metricas-desempeno/explore.md`)

- **Diseño confirmado (Anexo A.4)**: tabla hija `EmpresaHorarioLaboral` con `Id bigint IDENTITY`, `EmpresaId bigint NOT NULL`, `DiaSemana tinyint NOT NULL` (1=Lun..7=Dom), `HoraInicio`, `HoraFin`, `Estatus bit DEFAULT 1`, auditoría, `UNIQUE (EmpresaId, DiaSemana)`, `FK → Empresa(Id)`. **7 filas por empresa** (idempotente), `Estatus=1` laborable / `0` no laborable. El editor = lista fija de 7 checkboxes + 2 horas por día.
- **Tipo de hora**: `time` (nativo) mapea a `TimeSpan` vía `LlenarEntidad<T>` (`Convert.ChangeType(TimeSpan, TimeSpan)` funciona; `SqlDataReader` devuelve `TimeSpan` para columnas `time`). Recomendado `time` (facilita el cálculo en el SP de métricas). Alternativa `nvarchar(5)` 'HH:mm' con `CONVERT(time,...)` en SQL si se prefiere cero fricción JSON/UI (Newtonsoft serializa `TimeSpan` como `"HH:mm:ss"`). **Decisión fija: `time` + `TimeSpan`** (ya era la recomendación del Anexo A; si al implementar hay fricción en el JSON de la UI, exponer helper `string` HH:mm en el DTO).
- **SPs recomendados**:
  - `ObtenerHorarioLaboral (@Usuario NVARCHAR(25))` → `SELECT DiaSemana, HoraInicio, HoraFin, Estatus FROM EmpresaHorarioLaboral WHERE EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario=@Usuario AND Estatus=1) ORDER BY DiaSemana`.
  - **Guardado**: dado el UX (un solo botón "Guardar" = semana completa), se recomienda **un SP por día dentro de una transacción en C#** (patrón exacto de `GuardarPermisosRolMasivo`, `DbWrapper.Permisos.cs:303-367`, y del `foreach` de `EmpresaService`): `GuardarHorarioLaboralDia (@Usuario NVARCHAR(25), @DiaSemana TINYINT, @HoraInicio TIME, @HoraFin TIME, @EsLaboral BIT)` → valida tenant (`@EmpresaId` desde `@Usuario`), `UPDATE ... WHERE EmpresaId=@EmpresaId AND DiaSemana=@DiaSemana`; si no existe, `INSERT`; el WebApi itera 7 días bajo `DbWrapper.BeginTransaction()`. (Un único SP de reemplazo de semana completa con TVP se descarta: **no hay precedente de TVP** en el repo; un solo `GuardarHorarioLaboral` con parámetros delimitados sería una desviación. Si se quisiera, una alternativa válida es `DELETE` + `INSERT` de 7 filas en un solo SP con 7×3 parámetros, pero es feo; preferir el loop transaccional.)
- **Editor vive en la página NUEVA** (`ConfiguracionEmpresa`), no en `Home/Configuration` (requisito explícito del cambio).
- **Defaults y migración** (Anexo A.6): Lun–Vie 09:00–17:00 (`Estatus=1`), Sáb/Dom `Estatus=0`. Backfill idempotente (espeja `foliador-tickets/migration.sql:176-181`):
  ```sql
  INSERT INTO [dbo].[EmpresaHorarioLaboral] (EmpresaId, DiaSemana, HoraInicio, HoraFin, Estatus, CreadoPor, FechaCreacion)
  SELECT e.Id, d.Dia, d.Inicio, d.Fin, CASE WHEN d.Dia BETWEEN 1 AND 5 THEN 1 ELSE 0 END, N'migracion', GETDATE()
  FROM [dbo].[Empresa] e
  CROSS JOIN (VALUES (1,CAST('09:00' AS time),CAST('17:00' AS time)), ... (7,CAST('09:00' AS time),CAST('17:00' AS time))) d(Dia,Inicio,Fin)
  WHERE NOT EXISTS (SELECT 1 FROM [dbo].[EmpresaHorarioLaboral] h WHERE h.EmpresaId = e.Id);
  ```
- **Empresas nuevas**: hook en `EmpresaService.GuardarNuevaEmpresaConDatosIniciales` (`ServiceDeskDESIWebApi/Services/EmpresaService.cs:267-686`), como **"PASO 5.2"** (tras PASO 5 roles, antes de PASO 6), dentro de la misma transacción: `foreach (día 1..7) GuardarHorarioLaboralDia(empresaGuardada.Id, dia, 09:00, 17:00, dia∈1..5)`. **No usar trigger** (sin precedente: grep `CREATE TRIGGER` = 0; la transacción ya centraliza la siembra). `RegistrarEmpresa` (`:172-235`) ya delega en `GuardarNuevaEmpresaConDatosIniciales`, por lo que la cobertura es automática.

### Q6. Nueva página + permisos + gating de roles

- **Seed de página** (patrón exacto `archive/2026-08-26-vinculacion-persona-usuario/migration.sql:339-354`):
  ```sql
  IF NOT EXISTS (SELECT 1 FROM Pagina WHERE Nombre = N'ConfiguracionEmpresa')
  BEGIN
      INSERT INTO Pagina (Nombre, NombreVisible, Descripcion, Tipo, Direccion, PermisosPadreId, Logo, OrdenB, Estatus)
      VALUES (N'ConfiguracionEmpresa', N'Configuración de Empresa', N'Datos, horario laboral y logotipo de la empresa', N'Menu', N'/ConfiguracionEmpresa', NULL, N'fa-cog', 9, 1);
  END
  GO
  INSERT INTO RolPaginaAccion (RolId, PaginaId, PuedeLeer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, CreadoPor, FechaCreacion, Estatus)
  SELECT r.Id, p.Id, 1, 0, 1, 0, 0, N'migracion', GETDATE(), 1
  FROM Rol r CROSS JOIN Pagina p
  WHERE p.Nombre = N'ConfiguracionEmpresa' AND r.Nombre = N'Administrador' AND r.Estatus = 1
    AND NOT EXISTS (SELECT 1 FROM RolPaginaAccion rpa WHERE rpa.RolId = r.Id AND rpa.PaginaId = p.Id);
  GO
  ```
  ⚠️ `Rol` es **por empresa** (`Rol.EmpresaId`, sembrado por `GuardarRolParaNuevaEmpresa`); el CROSS JOIN `r.Nombre='Administrador'` cubre el rol Administrador de **todas** las empresas existentes. Las empresas **nuevas** lo reciben automáticamente porque `GuardarNuevaEmpresaConDatosIniciales` PASO 7 (`:552-594`) itera `ObtenerPaginas()` e inserta todos los flags en `true` para el rol Administrador. ⚠️ Verificar columnas reales de `Pagina`/`RolPaginaAccion` en la BD hosted antes de ejecutar (mismo aviso de `MisActivos`).
- **Roles por defecto**: **solo Administrador** (`PuedeLeer=1, PuedeEditar=1, PuedeCrear=0, PuedeEliminar=0`). No hay delete en el módulo. Otros roles se conceden después desde la **UI de Permisos existente** (`SecurityController.Permisos` → `Views/Security/Permisos.cshtml` → `GuardarPermisosRolMasivo`), confirmada presente.
- **Enforcement**: MVC `[Permiso("ConfiguracionEmpresa")]` / `[Permiso("ConfiguracionEmpresa","Editar")]`; WebApi `[Permiso("ConfiguracionEmpresa","Leer")]` en lecturas y `[Permiso("ConfiguracionEmpresa","Editar")]` en guardados. La vista Index puede además leer `PermisosViewModel` (patrón `SecurityController.Role`/`UserController.Users`) para ocultar botones y redirigir a `Home/AccesoDenegado` si `PuedeLeer=0`. **Leer → `PuedeLeer`; Guardar (editar horario/logo) → `PuedeEditar`** (no `PuedeCrear`, pues no se crean entidades nuevas). Sin acciones de Eliminar (no aplica).

### Q7. Archivos exactos a crear/tocar

Replicando `Dashboard`+`MisActivos`+`Evidencias`+foto de perfil (lista estilo `metricas` §4):

| Archivo | Acción |
|---|---|
| `openspec/changes/configuracion-empresa/migration.sql` (+ `rollback.sql`) | Nuevo: CREATE TABLE `EmpresaHorarioLaboral` + backfill + `ALTER TABLE Empresa ADD LogoUrl NVARCHAR(500) NULL` + INSERT `Pagina`/`RolPaginaAccion` + SPs `ObtenerHorarioLaboral`, `GuardarHorarioLaboralDia`, `GuardarLogoEmpresa` |
| `ServiceDeskDESIEntities/Catalogos/HorarioLaboral.cs` (+ `HorarioLaboralDTO.cs` si aplica) | POCO nuevo: `Id, EmpresaId, DiaSemana(int), HoraInicio(TimeSpan), HoraFin(TimeSpan), Estatus` + BaseObject |
| `ServiceDeskDESIEntities/Catalogos/Empresa.cs` | Añadir `public string LogoUrl { get; set; }` |
| `ServiceDeskDESIEntities/ServiceDeskDESIEntities.csproj` | Registrar POCO(s) nuevos (`<Compile Include>` — obligatorio, old-style) |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.HorarioLaboral.cs` | Nuevo (partial): `GetObjects`/`ExecuteScalar` + `LlenarEntidad` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Empresa.cs` | Añadir `GuardarLogoEmpresa` (o en el partial nuevo) |
| `ServiceDeskDESIWebApi/Services/HorarioLaboralService.cs` | Nuevo: `ModelResponse<T>` + Serilog + try/catch |
| `ServiceDeskDESIWebApi/Controllers/HorarioLaboralController.cs` (o `EmpresaController`) | `[Authorize] [RoutePrefix("api/HorarioLaboral")]`, `[Permiso("ConfiguracionEmpresa","Leer"/"Editar")]`, `var usuario = User.Identity.Name` |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.HorarioLaboral.cs` | Nuevo (partial): `RequestAsync<T>` |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.Empresa.cs` | Añadir `GuardarLogoEmpresa` |
| `ServiceDeskDESIMVC/Services/HorarioLaboralService.cs` | Nuevo |
| `ServiceDeskDESIMVC/Controllers/ConfiguracionEmpresaController.cs` | Nuevo: hereda `BaseController`; `Index()` valida `PuedeLeer` y carga empresa+horario; acciones AJAX `[Permiso(...)]` → `JsonConvert.SerializeObject`; acción de subida de logo (`HttpPostedFileBase`, espeja `UserController.ActualizarPerfilUsuario`) |
| `ServiceDeskDESIMVC/Views/ConfiguracionEmpresa/Index.cshtml` | Nuevo (**UTF-8 CON BOM**): datos de empresa (solo lectura) + editor horario (7 checkboxes + horas) + subida/preview logo |
| `ServiceDeskDESIMVC/ServiceDeskDESIMVC.csproj` | Registrar los 3 .cs MVC nuevos (lista explícita) |
| `ServiceDeskDESIMVC/Web.config` | `LogoEmpresaMaxTamanoMB`, `LogoEmpresaExtensionesPermitidas` (appSettings) |
| `ServiceDeskDESIMVC/Views/Shared/_Layout.cshtml` | Añadir footer estático "by DESi" |

- **`.csproj` old-style**: ambos `.csproj` usan `<Compile Include>` explícito (MVC 64 includes, Entities 52). Cualquier `.cs` no registrado no compila (error ya visto con `ThemeHelper`).
- **No hay test project**: verificación = MSBuild 0 errores + revisión estática.

### Q8. Correctitud multi-tenant

- **MVC**: `TokenCookie.EmpresaID` (de `SessionHelper.GetSessionUser()`; seteado en `HomeController.LogIn` `:309` desde `usuarioAutenticado.EmpresaId`). Patrón: `UserController.GuardarOActualizarUsuarioAdmin` `:278` fuerza `usuario.EmpresaId = tokenCookie.EmpresaID` (nunca del cliente).
- **WebApi**: `BaseController.ObtenerEmpresaIdDesdeClaim()` (claim `empresaId`), o los SPs resuelven `@EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)`. Los SPs nuevos de horario/logo **resuelven el tenant desde `@Usuario`** (consistente con el resto de `Obtener*`), **nunca** desde el body/query del cliente.
- **`Empresa` no tiene columna de logo hoy.** Dado que el usuario quiere extensibilidad futura, se evaluó: **(a)** columna tipada `Empresa.LogoUrl` + tabla hija `EmpresaHorarioLaboral`, vs **(b)** tabla genérica `EmpresaConfiguracion` clave/valor.
- **Recomendación concreta: enfoque tipado (a)**. El patrón del repo es **tablas hijas tipadas por empresa** (`Foliador`, `TicketAsignacion`, `CategoriaResponsable`) y **columnas explícitas**; **no existe** tabla genérica `Configuracion`/`Parametros`/`EmpresaConfiguracion` (grep = 0), ni uso de JSON en BD. El logo es una propiedad 1:1 natural del tenant → columna `LogoUrl` en `Empresa`; el horario es una colección → tabla hija `EmpresaHorarioLaboral`. Una tabla clave/valor perdería tipado, complicaría los SPs de métricas y sería una desviación del estilo. Configuraciones futuras se añaden como **columnas tipadas o tablas hijas nuevas**, no como blob genérico.

### Q9. Convenciones de menú (etiqueta/ícono/orden)

- **Llave**: `Pagina.Nombre = 'ConfiguracionEmpresa'` (sin acento). El `[Permiso]` matchea por igualdad exacta en `ObtenerPaginaPorNombre` (`WHERE Nombre = @Nombre`, collation-dependiente) → llave sin tilde elimina ambigüedad (Decisión 1 de `metricas`). **Etiqueta**: `NombreVisible = 'Configuración de Empresa'` (con tilde). El render ya hace `NombreVisible ?? Nombre` (`MenusUser.cshtml:23/34/46`).
- **Ícono** (`Pagina.Logo`, `nvarchar(25)`): el render es `<i class="fas @menu.Logo">`, así que `Logo` debe llevar **solo** la clase de icono sin prefijo `fas`. Recomendado **`fa-cog`** (coherente con el enlace "Configuración" del dropdown `_Layout.cshtml:212`, que usa `fa-cog`). Nota: el seed de `MisActivos` usó `fas fa-laptop` (doble prefijo `fas fas fa-laptop`, inofensivo en CSS), pero la convención de las filas existentes es icono sin prefijo.
- **Orden** (`OrdenB`): página independiente (`Tipo='Menu'`, `PermisosPadreId=NULL`, hoja). Orden actual: Dashboard=1, Tickets=2, Catálogos=3, Usuarios=4, Seguridad=5, Personas=6, Activos=7, Mi Perfil=8, MisActivos=99. Recomendado **`OrdenB = 9`** (al final del menú principal, después de Mi Perfil y antes de MisActivos=99), independiente.

---

## 3. Áreas afectadas (resumen)

| Área | Impacto |
|---|---|
| BD (`migration.sql`) | Tabla `EmpresaHorarioLaboral` + columna `Empresa.LogoUrl` + página/permiso + SPs |
| Entities | `HorarioLaboral` POCO + `Empresa.LogoUrl` + registro `.csproj` |
| WebApi | `DbWrapper.HorarioLaboral` (+ `GuardarLogoEmpresa`), `HorarioLaboralService`, `HorarioLaboralController` |
| MVC | `HttpClientConnection.HorarioLaboral` (+ `GuardarLogoEmpresa`), `HorarioLaboralService`, `ConfiguracionEmpresaController`, `Views/ConfiguracionEmpresa/Index.cshtml`, registro `.csproj`, `Web.config`, `_Layout.cshtml` (footer) |

---

## 4. Enfoques (alternativas)

1. **Logo como columna tipada `Empresa.LogoUrl` + archivo en disco MVC** (RECOMENDADO).
   - Pros: consistente con foto de perfil; simple; sirve inline vía estáticos de MVC; tipado.
   - Cons: toca el POCO `Empresa` y el SP `ObtenerEmpresaPorId`/`GuardarOActualizarEmpresa` (o un SP dedicado `GuardarLogoEmpresa`).
   - Esfuerzo: Bajo.

2. **Tabla genérica `EmpresaConfiguracion` clave/valor** (logo + futuros settings).
   - Pros: extensible sin migraciones de esquema.
   - Cons: sin precedente; pierde tipado; complica SPs de métricas; desviación del estilo.
   - Esfuerzo: Medio.

3. **Logo en BD como varbinary**.
   - Pros: transaccional con la fila de empresa.
   - Cons: sin precedente; peor para servir `<img>` (requiere handler); infla la BD.
   - Esfuerzo: Medio.

4. **Guardado de horario**: (a) `GuardarHorarioLaboralDia` × 7 en transacción C# (RECOMENDADO, espeja `GuardarPermisosRolMasivo`) vs (b) un solo SP de reemplazo de semana con TVP/parámetros delimitados.
   - (a) Pros: sin TVP (sin precedente), reutiliza `BeginTransaction`, simple. Cons: 7 round-trips (misma conexión).
   - (b) Pros: 1 round-trip. Cons: desviación (TVP/JSON no existe en repo).

**Recomendación**: enfoque **1 + 4a**. Logo = columna `LogoUrl` + archivo en `~/Uploads/Logos/{empresaId}/`; horario = tabla hija + `GuardarHorarioLaboralDia` en transacción.

---

## 5. Riesgos

1. **Seed de `Pagina`/`RolPaginaAccion` fuera de repo**: el menú vive solo en la BD hosted; la migración debe aplicarse y opcionalmente reflejarse en `openspec/basededatosservicedesk.txt` (drift ya documentado). Verificar columnas reales antes de ejecutar.
2. **Collation**: mitigado usando llave `ConfiguracionEmpresa` sin tilde + `NombreVisible` con tilde.
3. **Mapeo `time` → `TimeSpan`**: `LlenarEntidad<T>` usa `Convert.ChangeType`; validar el mapeo real al implementar (o usar `nvarchar(5)` con conversión en SQL si hay fricción JSON/UI).
4. **Subida de logo MVC-side sin límites previos**: la foto de perfil no valida MIME/peso; añadir appSettings `LogoEmpresa*` y validación server-side (evitar subida de ejecutables/oversize).
5. **`.csproj` old-style**: cualquier `.cs` nuevo no registrado no compila.
6. **Sin test project**: verificación = MSBuild + revisión estática.
7. **Zona horaria**: el horario se guarda sin zona; los timestamps usan `GETDATE()` del servidor (riesgo heredado del Anexo A.7.5, aplica al futuro SP de métricas).

---

## 6. Listo para propuesta

**Sí.** Quedan documentados: esquema `Empresa`/`Compania`, ausencia de logo (icono `fa-headset` + texto), precedentes de subida (Evidencias/foto de perfil), ausencia de footer, diseño `EmpresaHorarioLaboral` (Anexo A reutilizado), patrón de seed de página+permiso, enforcement `[Permiso]`/`ValidarPermisoUsuario`, correctitud multi-tenant, y la lista exacta de archivos. La propuesta (`sdd-propose`) y la spec (`sdd-spec`) pueden proceder sin re-leer código.
