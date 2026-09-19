# Exploration: Asignación de Activos a Personas (+ correo + confirmación + persistencia de envíos)

## Current State (verificado contra código real y BD hosted en vivo)

### ⚠️ Hallazgo principal: el núcleo del cambio YA está implementado y desplegado

El flujo **asignar/desvincular activo↔persona** ya existe completo (entidades, WebApi, MVC, vistas) y la `migration.sql` ya fue **aplicada a la BD hosted**. El commit `bd47e85` ("se realizan cambios para asignar activos a personas, se arregla activos y puestos") contiene el núcleo; el HEAD `e88aea0` ("se agrega template para confirmar activo") añadió el template de correo. **Lo que falta** es: (1) el envío del correo de notificación (el template existe pero NADA lo invoca), (2) el flujo "Confirmar Recepción" (no existe endpoint ni columna), y (3) la persistencia de intentos de envío (no existe tabla).

### 1. Base de datos (verificada en vivo vía sqlcmd `-C`)

- `dbo.PersonaActivo` **EXISTE** (object_id `702625546`) con columnas exactas a la migración: `Id, PersonaId, ActivoId, FechaInicio, FechaFin NULL, CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, Estatus, EmpresaId` → **migración ya aplicada**.
- Los 5 SPs **existen**: `AsignarActivoPersona`, `DesvincularActivoPersona`, `ObtenerActivosPorPersona`, `ObtenerActivosDisponibles`, `ObtenerActivos` (este último ya con el `LEFT JOIN PersonaActivo + Persona` para "Asignado a").
- `Activo` (en vivo): `Id, Nombre, Descripcion, TipoActivoID, Serial, MarcaID, ModeloID, Notas, FechaCompra, ...EmpresaId`. Nota: los FKs usan sufijo `ID` en mayúscula (`TipoActivoID`, `MarcaID`, `ModeloID`); los POCOs usan `Id` (`TipoActivoId`), lo cual es irrelevante porque `LlenarEntidad<T>` mapea case-insensitive.
- `Persona` (en vivo): `Id, Nombre, Apellido, Correo, Telefono, PuestoId, ...EmpresaId`.
- `Puesto`: `Id, Nombre, Descripcion, ...`. `Usuarios`: `NombreUsuario, Correo, Nombre, Apellido, ...`.
- **NO existe** ninguna tabla de tipo notificación/log/auditoría/bitácora de correo (consulta `sys.tables` con `LIKE '%Notif%|%Email%|%Correo%|%Log%|%Audit%|%Bitacora%'` = 0 filas).
- SPs relevantes (definiciones en vivo): `ObtenerPersonas`/`ObtenerPersonaPorId` devuelven `p.*` + `pu.Nombre as PuestoNombre` (incluye `Correo`, `Apellido`, `Nombre`, `PuestoId`). `ObtenerActivoPorId` devuelve `a.*` + `TipoActivoNombre, MarcaNombre, ModeloNombre`. `ObtenerActivos` devuelve `a.*` + 3 nombres + `PersonaNombre, PersonaApellido`.

### 2. Entidades (ServiceDeskDESIEntities)

- `Catalogos/Persona.cs:12-17` → `Nombre, Apellido, Correo, Telefono, PuestoId`. `PersonaDTO.cs:5-6` → `PuestoNombre, PuestoDescripcion`.
- `Catalogos/Activo.cs:11-18` → `Nombre, Descripcion, TipoActivoId?, Serial, MarcaId?, ModeloId?, Notas, FechaCompra?`. `ActivoDTO.cs:5-9` → `TipoActivoNombre, MarcaNombre, ModeloNombre, PersonaNombre, PersonaApellido`.
- `Catalogos/PersonaActivo.cs:7-10` → `PersonaId, ActivoId, FechaInicio, FechaFin?`. `PersonaActivoDTO.cs:5-6` → `ActivoNombre, ActivoSerial`.
- `BaseObject.cs:11-16` → `Id, CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, Estatus` (NO trae `EmpresaId`; el tenant se deriva server-side por `@Usuario`, patrón consistente en todo el repo).

### 3. Capas PersonaActivo (ya implementadas)

**WebApi:**
- `Controllers/PersonaActivoController.cs:15,28-71` → `[RoutePrefix("api/PersonaActivo")]`, rutas `GET ActivosPorPersona/{personaId}`, `GET Disponibles`, `POST Asignar`, `POST Desvincular`; `[Permiso("Personas", ...)]`. Request DTOs `AsignarActivoRequest`/`DesvincularActivoRequest` (`:74-83`).
- `Services/PersonaActivoService.cs:19-117` → 4 métodos con validación + Serilog; `AsignarActivoPersona`/`DesvincularActivoPersona` delegan a DbWrapper.
- `DAL/DbWrapper.PersonaActivo.cs:14-149` → `ExecuteScalar("AsignarActivoPersona"/"DesvincularActivoPersona")`; interpreta `<= -1` (ya asignado), `<= 0` (fallo). **No hay BeginTransaction** (asignación atómica manejada dentro del SP).

**MVC:**
- `DAL/HttpClientConnection.PersonaActivo.cs:15-58` → 4 métodos HTTP.
- `Services/PersonaActivoService.cs:19-37` → wrappers.
- `Controllers/CatalogsController.cs:799-826` → `ObtenerActivosPorPersona`, `ObtenerActivosDisponibles`, `AsignarActivoPersona` (`[Permiso("Personas","Editar")]` `:813`), `DesvincularActivoPersona` (`:821`). Instancia `_personaActivoService` en `:38,55`.

### 4. Frontend (vistas existentes)

- `Views/Catalogs/Persona.cshtml` → catálogo de personas con DataTable (`:149`), botón "Activos" por fila (`:160`) que llama `AbrirActivosPersona(id)` (`:318-331`) y abre el modal. Incluye el partial en `:84` → `@Html.Partial("_AsignarActivoPersona")`.
- `Views/Catalogs/_AsignarActivoPersona.cshtml:1-187` → modal "Activos de X" con dropdown de activos disponibles, tabla de asignados, y JS `AsignarActivo()`/`DesvincularActivo()` vía `PostMVC('/Catalogs/AsignarActivoPersona')`/`'/Catalogs/DesvincularActivoPersona'` (`:119`, `:159`).
- `Views/Catalogs/Active.cshtml:287` → columna "Asignado a" (`PersonaNombre + PersonaApellido`).
- Patrón de UI: Bootstrap 5.3 + DataTables 2.3.7 + SweetAlert2 + Font Awesome 6 + `GetMVC/PostMVC/MapingPropertiesDataTable`.
- Ambos `.cshtml` incluidos en `ServiceDeskDESIMVC.csproj` (`:246` Persona, `:247` _AsignarActivoPersona).

### 5. Correo (infraestructura existente) y su patrón de uso

- `Helpers/EmailHelper.cs:13` → `EnvioEmaiil(IEnumerable<string> para, asunto, mensaje, bool ssl=false, attachment="")`; SmtpClient con `EnableSsl = true` **hardcodeado** (`:69`), config de `Web.config`: `smtpClient=smtp.gmail.com`, `port=587`, `userEmail`, `passEmail` (`Web.config:13-16`). **Re-lanza la excepción** en `:83-86`.
- Patrón de envío (a replicar): leer template con `HostingEnvironment.MapPath("~/Template/...")` → `Replace("{{x}}", ...)` → `EmailHelper.EnvioEmaiil(...)`, todo envuelto en try/catch que **solo loguea** y devuelve `bool`. Ver `AutenticacionService.EnviarCorreoNuevoUsuario` (`Services/AutenticacionService.cs:504-546`) y `EmpresaService.EnviarCorreoBienvenida` (`Services/EmpresaService.cs:632-677`).
- **Al fallar el envío**: solo `Log.Error` a Serilog. **NO hay persistencia** de intentos ni reintentos. En `EmpresaService` el comentario explicita "La empresa quedó registrada sin credenciales enviadas" (`:673`).
- Template `Template/Templat_AsignacionActivo.html` (nota el **typo** "Templat" sin "e") con placeholders `{{NombreUsuario}} {{AsignadoPor}} {{NombreActivo}} {{Serial}} {{TipoActivo}} {{Marca}} {{Modelo}} {{FechaAsignacion}} {{PuestoUsuario}} {{CorreoUsuario}} {{UrlConfirmacion}}` y botón "Confirmar Recepción" (`:137-138`). Incluido en `ServiceDeskDESIWebApi.csproj:206`.

### 6. Disponibilidad de datos para los placeholders

| Placeholder | Fuente | Disponible |
|---|---|---|
| `NombreUsuario` | `Persona.Nombre` + `Apellido` | ✅ |
| `AsignadoPor` | `Usuarios.Nombre`+`Apellido` (vía `ObtenerUsuarioPorNombreUsuario`, `DbWrapper.Autenticacion.cs:88`) | ✅ |
| `NombreActivo` / `Serial` | `Activo` | ✅ |
| `TipoActivo` / `Marca` / `Modelo` | joins en `ObtenerActivoPorId` | ✅ |
| `FechaAsignacion` | `PersonaActivo.FechaInicio` (= `GETDATE()` del SP) | ✅ |
| `PuestoUsuario` | `Puesto.Nombre` (vía `ObtenerPersonaPorId`) | ✅ |
| `CorreoUsuario` | `Persona.Correo` | ✅ |
| `UrlConfirmacion` | **no existe endpoint** | ❌ |

⇒ Para enviar el correo, el servicio WebApi debe obtener (ya existen los métodos): persona (`ObtenerPersonaPorId` → Correo/Nombre/Apellido/PuestoNombre), activo (`ObtenerActivoPorId` → Nombre/Serial/TipoActivoNombre/MarcaNombre/ModeloNombre), y quien asigna (`ObtenerUsuarioPorNombreUsuario` → Nombre/Apellido).

## Key Findings

1. **El núcleo (asignar/desvincular + listado + UI) está completo y en producción.** No requiere re-implementación; el proposal debe reconocerlo y limitarse a lo faltante.
2. **La `migration.sql` ya está aplicada** a `db_9c7990_servicedeskdesi`; es idempotente para la tabla pero **hace DROP/CREATE de los 5 SPs** en cada re-ejecución.
3. **El correo de asignación NO se envía hoy.** El template existe (commit HEAD) pero ningún código lo lee/rellena/invoca. Es la pieza central pendiente.
4. **No hay persistencia de envíos de correo** en ningún flujo (solo Serilog). No existe tabla base para reaprovechar (la única "bitácora" es `TicketAsignacion`, que es de tickets, con `EsActiva`/`TipoMovimiento` — patrón conceptual a imitar si se decide persistir).
5. **"Confirmar Recepción" no está implementado**: no hay endpoint ni columna `Confirmado`/`FechaConfirmacion` en `PersonaActivo`; `{{UrlConfirmacion}}` quedaría sin resolver.
6. **Typos pre-existentes**: método `EnvioEmaiil` (doble "i") y archivo `Templat_AsignacionActivo.html` (sin "e"). Cosmético; renombrar el archivo implicaría tocar csproj + código.
7. `EmailHelper.EnvioEmaiil` **ignora su parámetro `ssl`** (siempre `EnableSsl=true`). El `AsignarActivoPersona` de DbWrapper **no envuelve en transacción** (la atomicidad está dentro del SP), así que un correo fallido tras un `INSERT` exitoso no debería revertir la asignación.

## Approaches

| Opción | Descripción | Pros | Contras | Esfuerzo |
|---|---|---|---|---|
| **A. Solo notificación por correo (RECOMENDADA)** | En `PersonaActivoService.AsignarActivoPersona` (WebApi), tras éxito del SP: obtener persona+activo+asignador, rellenar `Templat_AsignacionActivo.html` (dejar `UrlConfirmacion` como `#` o quitar el botón), llamar `EmailHelper` en try/catch (solo log). Diferir "Confirmar Recepción" y persistencia de intentos. | Cierra el hueco visible con mínimo riesgo; reutiliza métodos existentes; el fallo de correo no revierte la asignación. | No resuelve confirmación ni trazabilidad de envíos. | Baja |
| **B. A + "Confirmar Recepción"** | Añadir columna `FechaConfirmacion`/`Confirmado` a `PersonaActivo`, endpoint público de confirmación (MVC) con link `{{UrlConfirmacion}}`, actualizar estado. | Cierra el ciclo completo del template. | Nuevo endpoint (¿anónimo? ¿token?), migración aditiva, manejo de seguridad. | Media |
| **C. B + persistencia de intentos de envío** | Nueva tabla (p. ej. `EnvioCorreo`/`NotificacionCorreo`) con estado/error/reintentos, siguiendo el patrón histórico de `TicketAsignacion`. | Trazabilidad y reintentos. | Nueva tabla + entidad + DAL + SPs; decide si se reintenta y desde dónde. | Alta |

## Recommendation

**Opción A** como alcance mínimo de la siguiente fase, con **B** y **C** explícitamente decididas como in-scope o diferidas en el proposal (ver Scope Questions). El envío debe ubicarse en `ServiceDeskDESIWebApi/Services/PersonaActivoService.cs` (después de `AsignarActivoPersona` con `IsSuccess=true`), envolviendo la llamada a `EmailHelper` en try/catch que solo `Log.Error` (para no devolver "error al asignar" cuando la asignación ya se persistió). Los datos del correo se obtienen con los métodos ya existentes: `_dbWrapper.ObtenerPersonaPorId`, `_dbWrapper.ObtenerActivoPorId`, `_dbWrapper.ObtenerUsuarioPorNombreUsuario` — sin nuevos SPs ni columnas.

## Risks

- **Re-implementación innecesaria**: si el proposal asume el flujo "desde cero", se duplicará lo ya existente. El estado actual (commit `bd47e85` + `e88aea0`) debe quedar explícito.
- **Correo síncrono en el mismo request**: Gmail SMTP añade latencia al POST `Asignar`; si falla, `EmailHelper` lanza (hay que tragar la excepción). Considerar envío asíncrono/fire-and-forget.
- **`ObtenerActivos` hace DROP/CREATE** en cada corrida de la migración (no `IF NOT EXISTS`); re-aplicar es seguro para datos pero reemplaza la definición del SP.
- **Confirmación sin autenticación**: si se implementa `UrlConfirmacion` como endpoint público, expone `PersonaActivoId` y requiere token/uniqueness para evitar confirmaciones arbitrarias.
- **Typos** (`EnvioEmaiil`, `Templat_...`) pueden confundir en design/apply; decidir si se normalizan.

## Ready for Proposal

Sí. Pasar a **sdd-propose** con la Opción A como base y dejar explícito que el flujo asignar/desvincular ya está implementado, centrando el change en: (1) envío de correo de asignación, y decidiendo (2) "Confirmar Recepción" y (3) persistencia de intentos de envío.

## Scope Questions (para el proposal / usuario)

1. **"Confirmar Recepción"** — ¿in-scope ahora o diferido? (la migración actual NO tiene columna `Confirmado`; hoy `{{UrlConfirmacion}}` no tiene backend). Si se difiere, ¿se quita el botón del template o se deja con `#`?
2. **Persistencia de intentos de envío en BD** — el usuario mencionó "no sé si sea viable almacenar los intentos en la base de datos". ¿Se incluye ahora (Opción C) o se difiere?
3. **Typos** (`EnvioEmaiil`, `Templat_AsignacionActivo.html`) — ¿normalizar o conservar?
4. **Manejo de fallo de correo** — ¿asignación debe marcarse como exitosa aunque el correo falle (recomendado), o debe bloquear/compensar?
