# Proposal: Módulo "Configuración de Empresa" (horario laboral + logotipo)

- **Change**: `configuracion-empresa`
- **Fase**: propose
- **Fecha**: 2026-09-07
- **Origen**: `explore.md` (Q1–Q9) + decisiones fijas del usuario (D1–D5) + Anexo A de `metricas-desempeno` (tabla `EmpresaHorarioLaboral`, reutilizada).

## Intent

Añadir una página de menú nueva e independiente `ConfiguracionEmpresa` (etiqueta visible "Configuración de Empresa", ícono `fa-cog`) donde el rol autorizado de la empresa autenticada **ve** los datos generales de su `Empresa` (solo lectura) y **edita** únicamente dos criterios: (a) los días y el horario laboral y (b) el logotipo de la empresa (SVG). No hay operaciones de eliminación. Además, se añade un footer estático "by DESi" al layout MVC (no editable desde la página).

## Scope

### In Scope
- Página `Pagina.Nombre='ConfiguracionEmpresa'`, `NombreVisible='Configuración de Empresa'`, `Tipo='Menu'`, `Direccion='/ConfiguracionEmpresa'`, `Logo='fa-cog'`, `OrdenB=9`, independiente (no dentro de Administración).
- Seed `RolPaginaAccion` **solo** al rol `Administrador` (`PuedeLeer=1`, `PuedeEditar=1`); otros roles se conceden luego desde la UI de Permisos existente.
- Editor de horario laboral: 7 filas (Lun–Dom) con checkbox "labora" + hora inicio/fin por día; botón único "Guardar".
- Subida de logotipo **SVG o PNG** (validación de extensión/MIME + peso server-side) y visualización en la parte superior del menú (sidebar): si `Empresa.LogoUrl` existe se muestra la imagen de la empresa; si no, se conserva el logo DESi actual (`fa-headset` + texto).
- Card de datos generales de la empresa (solo lectura) — superconjunto de `Empresa`.
- Footer estático "by DESi" (logo DESi pequeño + texto + hipervínculos) en `_Layout.cshtml`.

### Out of Scope
- UI para editar el footer (markup estático).
- Operaciones de eliminación (no existe concepto "Eliminar" en el módulo).
- Cambios de branding en correos/templates (`Template_AltaEmpresa.html`) salvo que sea trivial.
- Logo en la página de login (resuelto: el login conserva el branding DESi; el reemplazo aplica solo al sidebar).
- Tabla genérica clave/valor `EmpresaConfiguracion` (se usa columna tipada + tabla hija).
- Consumo de la tabla `EmpresaHorarioLaboral` por el módulo de métricas (pertenece al change `metricas-desempeno`, pausado).

## Capabilities

### New Capabilities
- `configuracion-empresa`: página "Configuración de Empresa" (acceso por permiso, datos de empresa solo lectura, editor de horario laboral y subida/visualización de logotipo) con resolución multi-tenant por usuario autenticado.

### Modified Capabilities
- None (ningún spec existente en `openspec/specs/` cambia a nivel de requisitos).

## Approach

Replicar el patrón MVC → HttpClient → WebApi → `DbWrapper` → SP (precedentes `Dashboard`, `MisActivos`, foto de perfil).

- **BD** (`migration.sql` + `rollback.sql`): `CREATE TABLE EmpresaHorarioLaboral` + `ALTER TABLE Empresa ADD LogoUrl NVARCHAR(500) NULL` + seed de `Pagina`/`RolPaginaAccion` + SPs idempotentes `ObtenerHorarioLaboral(@Usuario)`, `GuardarHorarioLaboralDia`, `GuardarLogoEmpresa`.
- **Horario** (tabla `EmpresaHorarioLaboral`): `Id bigint IDENTITY`, `EmpresaId`, `DiaSemana tinyint` (1=Lun..7=Dom), `HoraInicio`/`HoraFin` **`datetime`** (decisión fija del usuario, NO `time`), `Estatus bit DEFAULT 1`, auditoría `BaseObject`, `UNIQUE(EmpresaId, DiaSemana)`, FK→`Empresa`. Guardado: `GuardarHorarioLaboralDia` × 7 dentro de una transacción C# (`DbWrapper.BeginTransaction`, espeja `GuardarPermisosRolMasivo`); se descarta TVP/JSON (sin precedente en repo). Defaults Lun–Vie 09:00–17:00 laborable, Sáb/Dom no laborable; backfill idempotente `INSERT…SELECT … WHERE NOT EXISTS` para empresas existentes + hook en `EmpresaService.GuardarNuevaEmpresaConDatosIniciales` (`EmpresaService.cs:267-686`, como "PASO 5.2", dentro de la transacción; cubre `RegistrarEmpresa`). No se usa trigger (grep `CREATE TRIGGER` = 0).
- **Logo — enfoque elegido (coherente, espeja foto de perfil)**:
  - **(a) Host físico: disco del MVC.** Archivo en `ServiceDeskDESIMVC/Uploads/Logos/{empresaId}/{guid}.svg` (o `.png`) — carpeta por empresa `Logos/{empresaId}/`, confirmada por el usuario. El binario NO pasa por WebApi (evita round-trip de binario).
  - **(b) Persistencia del path**: la acción MVC sube el archivo y setea `Empresa.LogoUrl = "/Uploads/Logos/{empresaId}/{guid}.ext"` (ruta **relativa**); luego persiste SOLO el path vía `HttpClientConnection` → WebApi → SP `GuardarLogoEmpresa(@Usuario, @LogoUrl)` con `[Permiso("ConfiguracionEmpresa","Editar")]`, resolviendo `@EmpresaId` desde `@Usuario` (nunca desde el cliente).
  - **(c) Resolución del `<img>`**: el sidebar (`MenusUser.cshtml`) renderiza la imagen del logo de la empresa SOLO si `Empresa.LogoUrl` tiene valor; en caso contrario conserva el logo DESi actual (`fa-headset` + texto). Al ser ruta relativa, no se codifica la URL absoluta de la API en BD; MVC y WebApi permanecen como despliegues separados sin acoplamiento de dominio.
  - Validación: extensión + MIME (`image/svg+xml` o `image/png`) server-side; appSettings en `ServiceDeskDESIMVC/Web.config`: `LogoEmpresaMaxTamanoKB` (default 2048) y `LogoEmpresaTiposPermitidos` (default `svg,png`), espejando el patrón `EvidenciasMax*`.
- **Editor de horario en la página nueva** (`ConfiguracionEmpresa`): inputs nativos `datetime-local` (p. ej. "2026-09-07 22:01"); solo la componente hora es significativa, la fecha se normaliza a un ancla fija al guardar.
- **Multi-tenant**: MVC usa `TokenCookie.EmpresaID`; WebApi `BaseController.ObtenerEmpresaIdDesdeClaim()` / SPs resuelven `@EmpresaId` desde `@Usuario`. Nunca se acepta `EmpresaId` del input del cliente; el endpoint de subida exige permiso + propiedad (empresa del usuario autenticado).
- **Footer** estático en `_Layout.cshtml` tras `@RenderBody()`: logo DESi pequeño + "by DESi" + hipervínculos (Ayuda / Términos / Privacidad). No toca menú ni permisos.

## Definiciones de negocio (horario)

| Concepto | Regla |
|---|---|
| Filas | 7 por empresa (DiaSemana 1..7), `UNIQUE(EmpresaId, DiaSemana)`. |
| Laborable | `Estatus=1` (checkbox "labora" marcado); `Estatus=0` = no laborable. |
| Hora inicio/fin | Columnas `datetime`; solo la componente de hora es significativa. |
| Default seed | Lun–Vie 09:00–17:00 (`Estatus=1`); Sáb/Dom (`Estatus=0`). |
| Edición | Un solo botón "Guardar" = semana completa (7 filas en transacción). |

## Affected Areas

| Área | Impacto | Descripción |
|---|---|---|
| `openspec/changes/configuracion-empresa/migration.sql` + `rollback.sql` | Nuevo | CREATE `EmpresaHorarioLaboral` + backfill + `ALTER Empresa ADD LogoUrl` + seed `Pagina`/`RolPaginaAccion` + SPs |
| `ServiceDeskDESIEntities/Catalogos/HorarioLaboral.cs` | Nuevo | POCO (`Id, EmpresaId, DiaSemana, HoraInicio(DateTime), HoraFin(DateTime), Estatus` + `BaseObject`) |
| `ServiceDeskDESIEntities/Catalogos/Empresa.cs` | Mod | Añadir `LogoUrl string` |
| `ServiceDeskDESIEntities/ServiceDeskDESIEntities.csproj` | Mod | Registrar POCO nuevo (`<Compile Include>`, old-style) |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.HorarioLaboral.cs` | Nuevo | `LlenarEntidad<T>` + `ExecuteScalar` (horario) |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Empresa.cs` | Mod | `GuardarLogoEmpresa` |
| `ServiceDeskDESIWebApi/Services/HorarioLaboralService.cs` | Nuevo | `ModelResponse<T>` + Serilog |
| `ServiceDeskDESIWebApi/Controllers/HorarioLaboralController.cs` | Nuevo | `[Authorize]` + `[Permiso("ConfiguracionEmpresa",…)`; `var usuario = User.Identity.Name` |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.HorarioLaboral.cs` | Nuevo | `RequestAsync<T>` |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.Empresa.cs` | Mod | `GuardarLogoEmpresa` |
| `ServiceDeskDESIMVC/Services/HorarioLaboralService.cs` | Nuevo | Lógica MVC |
| `ServiceDeskDESIMVC/Controllers/ConfiguracionEmpresaController.cs` | Nuevo | `Index()` (PuedeLeer) + AJAX `[Permiso]` + acción subida logo (`HttpPostedFileBase`, espeja `UserController.ActualizarPerfilUsuario`) |
| `ServiceDeskDESIMVC/Views/ConfiguracionEmpresa/Index.cshtml` | Nuevo | UTF-8 **CON BOM**: card empresa + editor horario + subida/preview logo |
| `ServiceDeskDESIMVC/ServiceDeskDESIMVC.csproj` | Mod | Registrar 3 `.cs` MVC nuevos |
| `ServiceDeskDESIMVC/Web.config` | Mod | appSettings `LogoEmpresaMaxTamanoKB`, `LogoEmpresaTiposPermitidos` |
| `ServiceDeskDESIMVC/Views/Shared/_Layout.cshtml` | Mod | Footer estático "by DESi" |
| `ServiceDeskDESIMVC/Views/Home/MenusUser.cshtml` | Mod | Sustituir `fa-headset` por `<img>` del logo de empresa (fallback a ícono) |

## Risks

| Riesgo | Prob. | Mitigación |
|---|---|---|
| Seed `Pagina`/`RolPaginaAccion` vive solo en BD hosted (drift) | Media | Verificar columnas reales (`sys.columns`) antes de ejecutar; reflejar en `basededatosservicedesk.txt` |
| Collation BD no confirmada → `[Permiso]` falla con tilde | Baja | Llave sin acento `ConfiguracionEmpresa` + `NombreVisible` con tilde |
| `HoraInicio`/`HoraFin` como `datetime` (fecha irrelevante) | Media | Normalizar fecha a ancla fija al guardar; solo usar componente hora |
| Subida SVG sin validación previa (foto de perfil no valida) | Media | Validar extensión + MIME `image/svg+xml` + `LogoEmpresaMaxTamanoKB`; rechazar oversize/ejecutables |
| `.csproj` legacy sin registrar nuevos `.cs` → no compila | Media | Registrar manualmente (patrón `ThemeHelper`) |
| Sin test project (verificación = MSBuild + revisión) | Media | 0 errores de build + revisión estática |
| Zona horaria (`GETDATE()` del servidor) | Baja | Heredado del Anexo A.7.5; documentar (aplica al futuro SP de métricas) |

## Rollback Plan

- **BD**: `rollback.sql` = `DROP PROCEDURE` (horario/logo) + `DELETE` de `Pagina`/`RolPaginaAccion` "ConfiguracionEmpresa" + `DROP TABLE EmpresaHorarioLaboral` + `ALTER TABLE Empresa DROP COLUMN LogoUrl`. Todo aditivo/no destructivo sobre datos preexistentes.
- **Código**: quitar controllers/services/DAOs/views nuevos y su registro en ambos `.csproj`; quitar appSettings `LogoEmpresa*` de `Web.config`; revertir footer en `_Layout.cshtml` y el `<img>` en `MenusUser.cshtml`.
- **Archivos**: eliminar carpeta `Uploads/Logos/` (o dejarla; es inofensiva).

## Dependencies

- Migración SQL ejecutada en BD antes del despliegue.
- Precedentes existentes: foto de perfil (subida MVC-side), `GuardarPermisosRolMasivo` (transacción), seed de página `migration.sql:339-354`.
- `EmpresaHorarioLaboral` será consumida luego por `metricas-desempeno` (no bloqueante).

## Success Criteria

- [ ] La página "Configuración de Empresa" aparece en el menú solo para el rol Administrador (`PuedeLeer=1`).
- [ ] `[Permiso("ConfiguracionEmpresa","Leer")]`/`"Editar"` se aplican en MVC y WebApi; redirige a `Home/AccesoDenegado` si `PuedeLeer=0`.
- [ ] Los datos generales de la empresa se muestran solo lectura; solo horario y logo son editables.
- [ ] El editor de horario guarda los 7 días en una transacción (checkbox "labora" + hora inicio/fin `datetime`), con defaults Lun–Vie 09:00–17:00.
- [ ] Empresas nuevas reciben horario por defecto al registrarse; backfill cubre empresas existentes (idempotente).
- [ ] El logo SVG se valida (extensión/MIME/peso), se guarda en `Uploads/Logos/{empresaId}/`, se persiste `Empresa.LogoUrl` relativo y se muestra en el sidebar.
- [ ] El footer "by DESi" aparece en el layout (estático, no editable).
- [ ] `ServiceDeskDESI.sln` compila sin errores (0 errores).

## Open Questions (resueltas 2026-09-07)

1. **Logo en login** — RESUELTO: fuera de alcance; el login conserva el branding DESi. El reemplazo aplica solo al sidebar (parte superior del menú), con fallback a DESi mientras `Empresa.LogoUrl` esté vacío.
2. **Formatos de logo** — RESUELTO: permitir `svg` + `png` (default en `LogoEmpresaTiposPermitidos`).
