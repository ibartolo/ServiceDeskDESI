# Exploración: mejoras-rol-activos-permisos

Investigación de código (READ-ONLY) para mapear TODOS los touchpoints de los 7 cambios solicitados. Convención de rutas: relativas a `ServiceDeskDESI/` (raíz del .sln). Arquitectura N-capas: **MVC → HttpClient → WebApi → EF-less DbWrapper (reflexión) → SQL Server (SPs)**.

---

## 1. Nuevo flag en Roles para modificar Usuarios/Personas

### Estado actual
- **Entidad Rol** (`ServiceDeskDESIEntities/Seguridad/Rol.cs:9-14`): ya tiene el flag existente `PuedeAtenderTickets` (bool). Es el modelo a replicar.
- **Tabla Rol** (`openspec/basededatosservicedesk.txt:341-356`): `PuedeAtenderTickets [bit] NOT NULL`. En BD real también tiene `EmpresaId` (añadida por `openspec/changes/tenant-estructural/migration.sql:36`), pero el dump está **desactualizado** (no la muestra).
- **Vista CRUD Roles** (`ServiceDeskDESIMVC/Views/Security/Role.cshtml:47-57`): checkbox `PuedeAtenderTickets` (`@Html.CheckBoxFor`). El JS `GuardarActualizarRol()` (línea 165-175) arma el objeto con `PuedeAtenderTickets: $("#PuedeAtenderTickets").is(':checked')` y hace `PostMVC('/Security/GuardarOActualizarRol', rol, ...)`.
- **Persistencia del flag** (cadena completa):
  - MVC `Controllers/SecurityController.cs:118-122` → `_rolService.GuardarOActualizarRol`.
  - `Services/RolService.cs:34-37` → `_httpClient.GuardarOActualizarRol`.
  - `DAL/HttpClientConnection.Rol.cs:26-30` → POST `api/Rol/Guardar`.
  - WebApi `Controllers/RolController.cs:56-63` → `_rolService.GuardarOActualizarRol`.
  - WebApi `Services/RolService.cs:67-93` (valida Nombre/Descripción; **no valida flags**).
  - `DAL/DbWrapper.Rol.cs:79-131`: construye `parametrosObj` con `PuedeAtenderTickets = rol.PuedeAtenderTickets` (línea 90) y llama SP `GuardarOActualizarRol`.
  - SP `GuardarOActualizarRol` (`basededatosservicedesk.txt:2926-2991`): firma con `@PuedeAtenderTickets BIT`, UPDATE línea 2961, INSERT línea 2984.

### Cómo se determina el rol del usuario actual en MVC
- Sesión: `SessionHelper.GetSessionUser()` → `TokenCookie` (`Helpers/SessionHelper.cs:43-58`; `Seguridad/TokenCookie.cs`) con `UserID`, `EmpresaID`, `UserName`. **NO contiene rol**.
- Patrón existente (reutilizable): `CatalogsController.ObtenerUsuariosQuePuedenAtenderLista()` (`Controllers/CatalogsController.cs:1032-1053`) itera usuarios y llama `_rolService.ObtenerRolesPorUsuario(usuario.Id)` y evalúa `r.PuedeAtenderTickets`.
- `ObtenerRolesPorUsuario(long usuarioId)`: MVC `DAL/HttpClientConnection.Rol.cs:63-66` → GET `api/Rol/Usuario/{usuarioId}` → WebApi `RolController.cs:100-106` → `RolService.ObtenerRolesPorUsuario` (`Services/RolService.cs:147-170`) → `DbWrapper.ObtenerRolesPorUsuarioId` (`DAL/DbWrapper.Rol.cs:285-311`) → SP `ObtenerRolesPorUsuarioId` (devuelve `r.*`).
- El usuario actual: `tokenCookie.UserID` (disponible en `BaseController.cs:26`).

### Touchpoints exactos para el nuevo flag
| Capa | Archivo | Cambio |
|------|---------|--------|
| Entidad | `ServiceDeskDESIEntities/Seguridad/Rol.cs` | Añadir `public bool PuedeModificarUsuariosPersonas { get; set; }` |
| Vista | `Views/Security/Role.cshtml:47-57` | Añadir checkbox nuevo + armar objeto en JS (`GuardarActualizarRol`) |
| MVC DAL | `DAL/HttpClientConnection.Rol.cs` | (auto, serializa entidad) |
| WebApi DAL | `DAL/DbWrapper.Rol.cs:86-97` | Añadir campo al `parametrosObj` |
| SP | `GuardarOActualizarRol` | Añadir `@PuedeModificarUsuariosPersonas BIT` + UPDATE/INSERT |
| Vistas Usuarios/Personas | ver §Bloqueo | Habilitar inputs solo si el rol del usuario actual tiene el flag |

### Bloqueo de inputs en Usuarios/Personas (depende del mecanismo existente)
Ver **§Nota: mecanismo Persona↔Usuario** abajo. Hoy el bloqueo se basa en `estaVinculada = Model.UsuarioId.HasValue` (Persona.cshtml) — NO en el rol. El nuevo requisito implica **reemplazar/complementar** ese criterio: en modo edición, los inputs solo se habilitan para usuarios del sistema cuyo rol tenga el flag activo.

### Riesgos/incógnitas
- **Ambigüedad de regla**: "los inputs solo se habilitan en edición para usuarios del sistema cuyo rol tiene el flag" — ¿aplica al usuario *que está editando* (sesión) o al usuario *que está siendo editado*? En `Users.cshtml` se edita a OTRO usuario; en `Persona.cshtml` se edita una persona. Necesita clarificación de si el flag del rol del **usuario logueado** gobierna el bloqueo, o si se bloquean los inputs del registro editado según el rol del usuario editado.
- `Users.cshtml` (`Views/User/Users.cshtml`) **no tiene** ningún bloqueo de inputs hoy (todos editables); no recibe `ViewBag.Permisos` (a diferencia de Persona/Active/Role). Habrá que añadir la lógica de bloqueo y el permiso de página.
- El flag debe validarse también en **server-side** (no solo ocultar el botón); hoy `GuardarOActualizarUsuarioAdmin` (`UserController.cs:261-297`) no valida rol.

---

## 2. Validación No. de Serie único en Activos

### Estado actual
- **Entidad** `ServiceDeskDESIEntities/Catalogos/Activo.cs:14`: `Serial` (string, sin restricción).
- **Vista** `Views/Catalogs/Active.cshtml`: campo `Serial` (línea 62). Validación JS (`jquery.validate`, líneas 238-274) **solo** valida Nombre/Descripcion/FechaCompra — **no hay validación de serial ni de unicidad**.
- **Servicio WebApi** `Services/ActivoServices.cs:68-99`: valida `Serial.Length <= 50` (línea 80), **no unicidad**.
- **SP `GuardarOActualizarActivo`** (`basededatosservicedesk.txt:2023-2169`): sin chequeo de unicidad de `Serial`.
- **Tabla Activo** (`basededatosservicedesk.txt:88-108`): `Serial [nvarchar](50) NULL`, **sin índice único ni constraint**. En BD real incluye `EmpresaId` (tenant-estructural).

### Touchpoints para implementar unicidad
| Capa | Archivo | Cambio |
|------|---------|--------|
| BD | migration.sql nuevo | Índice único filtrado por empresa, p.ej. `CREATE UNIQUE INDEX UX_Activo_Serial ON Activo(Serial) WHERE Serial IS NOT NULL` **+ EmpresaId** (evaluar si la unicidad es global o por empresa) |
| SP | `GuardarOActualizarActivo` | `IF EXISTS(SELECT 1 FROM Activo WHERE Serial = @Serial AND EmpresaId = @EmpresaId AND Id <> @Id AND Estatus = 1)` → devolver código de error (p.ej. `-2`) para mensaje amigable |
| WebApi DAL | `DAL/DbWrapper.Activo.cs:80-129` | Mapear código `-2` → `Message` "Ya existe un activo con ese No. de Serie" |
| Vista | `Views/Catalogs/Active.cshtml` | Mostrar el mensaje de error del response (ya lo hace vía Swal en `GuardarActualizarActivo`, línea 414-423) |
| (opcional) | `Active.cshtml` JS | Validación client-side de formato/no vacío si se desea |

### Riesgos/incógnitas
- **Serial es NULL/opcional hoy**: decidir si la unicidad aplica solo a seriales no nulos (índice filtrado) o si Serial pasa a ser requerido. Dato: hoy el campo se guarda vacío si no se captura.
- **Alcance tenant**: unicidad global vs por `EmpresaId` (multi-tenant). Recomendado: por `EmpresaId`.
- Activos con **soft-delete** (`Estatus = 0`): ¿un serial "eliminado" puede reutilizarse? (el índice debe decidir si filtra `Estatus`).

---

## 3. Nuevo campo `SerieLocal` en Activos

Campo 100% capturable por el usuario, texto plano (ej. `LAP-PR-001`), **NO calculado**, debe almacenarse.

### Touchpoints exactos
| Capa | Archivo | Cambio |
|------|---------|--------|
| Entidad | `ServiceDeskDESIEntities/Catalogos/Activo.cs` | Añadir `public string SerieLocal { get; set; }` (DTO `ActivoDTO.cs` hereda automáticamente) |
| Vista | `Views/Catalogs/Active.cshtml` | Añadir `@Html.TextBoxFor(x => x.SerieLocal, ...)` + incluir `SerieLocal: $("#SerieLocal").val()` en el objeto `activo` (JS, líneas 382-397) |
| WebApi DAL | `DAL/DbWrapper.Activo.cs:86-103` | Añadir `a.SerieLocal` al `parametrosObj` (la reflexión `ObtenerParametrosSQL` genera `@SerieLocal` automáticamente) |
| SP | `GuardarOActualizarActivo` | Añadir `@SerieLocal NVARCHAR(...)` + UPDATE (línea 2093) + INSERT (línea 2160) |
| BD | migration.sql | `ALTER TABLE Activo ADD SerieLocal NVARCHAR(100) NULL` (guard idempotente) |
| Lectura | `ObtenerActivoPorId` / `ObtenerActivos` | Devuelven `a.*` → **auto-incluyen** SerieLocal (mapeo por reflexión `LlenarEntidad`) |

### Patrón de adición de columnas (referencia)
`openspec/changes/archive/2026-08-26-asignacion-activos/migration.sql:164-173`: `IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(...) AND name = ...) ALTER TABLE ... ADD ...`. Usar el mismo guard idempotente.

---

## 4. Gestor de mantenimientos para Activos

Requisito: solo registrar (no programar). Cargar únicamente comentarios **con fecha**. Historial accesible vía **MODAL** (no en ventana principal).

### Estado actual
- **No existe** entidad/tabla `Mantenimiento` (grep en todo el repo: solo matches en un calendario demo `Scripts/Comun/Calendario.js:39-43`, no relacionado).
- **Ventana Activos** (`Views/Catalogs/Active.cshtml`): form + DataTable `tblActivo`. Las "Acciones" de cada fila (JS, líneas 289-305) ya renderizan botones Editar/Eliminar; ahí se puede añadir un botón "Mantenimientos".
- **Patrón de modal + registro hijo** existente: `PersonaActivo` (asignación de activos a persona):
  - Tabla hija `PersonaActivo` + SPs (`AsignarActivoPersona`, `DesvincularActivoPersona`, `ObtenerActivosPorPersona`, `ObtenerActivosDisponibles`).
  - Partial `Views/Catalogs/_AsignarActivoPersona.cshtml` (modal `modalAsignarActivo`) + funciones JS `AbrirActivosPersona`, `CargarActivosPersona`, `AsignarActivo`, `DesvincularActivo`.
  - Invocado desde `Persona.cshtml:100` (`@Html.Partial("_AsignarActivoPersona")`) y `AbrirActivosPersona` (línea 433-446).

### Touchpoints (diseño a seguir = patrón PersonaActivo)
| Capa | Archivo | Cambio |
|------|---------|--------|
| BD | migration.sql | `CREATE TABLE Mantenimiento` (Id, ActivoId FK, Comentario NVARCHAR(...), Fecha DATETIME, CreadoPor, FechaCreacion, Estatus, EmpresaId) + SPs `ObtenerMantenimientosPorActivo`, `GuardarMantenimiento` |
| Entidad | `ServiceDeskDESIEntities/Catalogos/` | Nueva `Mantenimiento.cs` (+ `MantenimientoDTO.cs` si se requieren joins) |
| WebApi DAL | `DAL/DbWrapper.Mantenimiento.cs` (nuevo partial) | Métodos de lectura/escritura vía SPs |
| WebApi Service | `Services/MantenimientoService.cs` (nuevo) | Validaciones |
| WebApi Controller | `Controllers/MantenimientoController.cs` (nuevo) | Rutas `api/Mantenimiento/...` |
| MVC DAL | `DAL/HttpClientConnection.Mantenimiento.cs` (nuevo partial) | Llamadas HTTP |
| MVC Service | `Services/MantenimientoService.cs` (nuevo) | Orquestación |
| MVC Controller | `Controllers/CatalogsController.cs` (o nuevo) | Endpoints `ObtenerMantenimientosPorActivo`, `GuardarMantenimiento` |
| Vista | `Views/Catalogs/Active.cshtml` + partial `_MantenimientoActivo.cshtml` (modal) | Botón por fila + modal que carga/lista historial |

### Riesgos/incógnitas
- "Cargar solo comentarios con fecha" → el filtro debe excluir registros con `Fecha IS NULL` (en SP `ObtenerMantenimientosPorActivo`).
- ¿El modal es solo **lectura** (historial) o también **captura** (agregar mantenimiento)? El texto dice "registrar" + "historial vía modal" → se infiere captura dentro del modal + lista de historial. Confirmar.
- El "fecha" del mantenimiento: ¿fecha en que se realizó (capturable) o fecha de registro (auto)? Requiere clarificación (la columna `Fecha` puede ser capturable).

---

## 5. Nuevo campo `Comentarios` en Activos (máx 250)

Ídem que §3, con límite 250. Touchpoints idénticos a `SerieLocal`:
- Entidad `Activo.cs`: `public string Comentarios { get; set; }`.
- Vista `Active.cshtml`: textbox + validación `maxlength: 250` + objeto JS.
- `DbWrapper.Activo.cs` `parametrosObj`: `a.Comentarios`.
- SP `GuardarOActualizarActivo`: `@Comentarios NVARCHAR(250)` + UPDATE/INSERT.
- BD: `ALTER TABLE Activo ADD Comentarios NVARCHAR(250) NULL`.
- Lectura: auto por `a.*`.

> Nota: `Notas` ya existe (NVARCHAR(250)) y es semánticamente similar a "Comentarios". Confirmar si `Comentarios` es distinto de `Notas` o si se trata del mismo campo renombrado.

---

## 6. Bug: contador de páginas asignadas en ventana de permisos

### Estado actual
- Vista `Views/Security/Permisos.cshtml` (chooser de páginas/acciones por rol).
- Columna "Páginas Asignadas" en `tblRoles`: render hardcodea `<span class="badge-paginas">0</span>` (línea 357-361).
- `ActualizarBadges()` (líneas 553-574): solo actualiza el badge cuando `rolId === rolSeleccionadoId` (usa `paginasByRol.asignadas.length`); para **cualquier otro rol pone `count = 0`** (comentario explícito en línea 564-566: "Para otros roles, necesitaríamos tener sus permisos en cache. Por ahora usamos 0").
- `ActualizarBadges()` se llama desde `CargarRoles()` (tras `tablaRoles.draw()`, línea 389) y desde `CargarPermisos`/`AsignarPagina`/`QuitarPagina`.
- **Causa raíz del bug**: al cargar por primera vez, `rolSeleccionadoId` es `null` (no se ha seleccionado rol), así que `ActualizarBadges()` deja **todos los badges en 0**. No existe endpoint/SP que devuelva el conteo de páginas por rol para poblar la columna al cargar. El contador solo se puebla correctamente DESPUÉS de seleccionar un rol (evento `change` del `ddlRol`).

### Touchpoints para el fix
| Capa | Archivo | Cambio |
|------|---------|--------|
| Vista | `Views/Security/Permisos.cshtml:553-574` | Reemplazar lógica de badge por datos reales por rol |
| (opcional) Backend | SP nuevo `ObtenerConteoPaginasPorRol` + endpoint | Devolver conteo de `RolPaginaAccion` por `RolId` |
| MVC | `SecurityController.Permisos()` (`Controllers/SecurityController.cs:62-99`) | Poblar ViewBag con conteos por rol, o |
| MVC DAL | `DAL/HttpClientConnection.Permisos.cs` | Nuevo método para conteos |

Alternativa mínima: en `CargarRoles`, por cada rol hacer `ObtenerPermisosPorRol(rolId)` (ya existe el endpoint `api/Permisos/Rol/{rolId}`) y cachear el conteo; o un solo endpoint masivo. **Riesgo de N+1** si se consulta rol por rol.

---

## 7. Bug: tema oscuro no se aplica al chooser de permisos

### Cómo se implementa el tema oscuro
- CSS: `CSS/Comun/TemplatePage.css` — variables CSS + selectores `body.dark-theme { ... }` (sección líneas 440-896). Cubre: form-control, form-select, form-check-input, table, modal, DataTables, sidebar, botones, etc.
- Aplicación: `Views/Shared/_Layout.cshtml:170` — `<body class="@(ThemeHelper.GetTemaClase(...))">`; `Helpers/ThemeHelper.cs` lee cookie `TemaUsuario_{UserID}` (`light`/`dark`). `Views/Home/Configuration.cshtml:90` togglea `body.dark-theme` con `CambiarTema('dark')`.
- Commit de referencia: `24b55ce` "cambio de estilos a modo oscuro".

### Causa del bug
- El chooser de `Permisos.cshtml` define su **propio `<style>` inline** (líneas 16-160) con colores claros hardcodeados: `.chooser-column` (fondo `#f8f9fc`, borde `#e3e6f0`), `.chooser-item` (fondo `white`, borde `#e3e6f0`), `.item-nombre` (`#5a5c69`), `.item-direccion` (`#858796`), `.empty-message` (`#858796`), `.badge-paginas` (fondo `#4e73df`), `.text-muted-small` (`#858796`).
- `TemplatePage.css` **NO** tiene reglas `body.dark-theme .chooser-*` ni `.badge-paginas` → en tema oscuro estas piezas quedan **claras** (fondo blanco/gris claro, texto oscuro) sobre el fondo oscuro.

### Touchpoints para el fix
| Capa | Archivo | Cambio |
|------|---------|--------|
| CSS | `CSS/Comun/TemplatePage.css` | Añadir `body.dark-theme .chooser-column`, `.chooser-item`, `.item-nombre`, `.item-direccion`, `.empty-message`, `.text-muted-small`, `.badge-paginas`, `.chooser-item.disponible/asignada` (bordes izquierdos), inputs `.permisos-checkboxes` |
| (alternativa) | `Views/Security/Permisos.cshtml` | Usar variables CSS o mover estilos al CSS global |

Riesgo: hay **otros** `.cshtml` con `<style>` inline hardcodeado (p.ej. `Active.cshtml` `.is-invalid-dropdown`) que también pueden quedar sin tema oscuro; verificar si el alcance incluye solo el chooser.

---

## Mapa consolidado de cambios de esquema DB

### Columnas nuevas en `Activo`
```sql
-- patrón idempotente (ver asignacion-activos/migration.sql:164-173)
ALTER TABLE Activo ADD SerieLocal NVARCHAR(100) NULL;      -- ítem 3 (texto libre, ej. LAP-PR-001)
ALTER TABLE Activo ADD Comentarios NVARCHAR(250) NULL;     -- ítem 5 (máx 250)
```

### Unicidad de `Serial` (ítem 2)
```sql
-- si se decide por empresa + solo seriales no nulos:
CREATE UNIQUE INDEX UX_Activo_Serial ON Activo(EmpresaId, Serial) WHERE Serial IS NOT NULL;
-- + validación en SP GuardarOActualizarActivo para mensaje amigable
```

### Nueva tabla `Mantenimiento` (ítem 4)
```sql
CREATE TABLE Mantenimiento (
  Id BIGINT IDENTITY(1,1) PRIMARY KEY,
  ActivoId BIGINT NOT NULL,            -- FK → Activo(Id)
  Comentario NVARCHAR(MAX) NULL,       -- o NVARCHAR(500)
  Fecha DATETIME NULL,                 -- fecha del mantenimiento
  CreadoPor NVARCHAR(25) NOT NULL,
  FechaCreacion DATETIME NOT NULL,
  ModificadoPor NVARCHAR(25) NULL,
  FechaModificacion DATETIME NULL,
  Estatus BIT NOT NULL,
  EmpresaId BIGINT NOT NULL            -- tenant (patrón tablas de dominio)
);
```

### Nueva columna en `Rol` (ítem 1)
```sql
ALTER TABLE Rol ADD PuedeModificarUsuariosPersonas BIT NOT NULL CONSTRAINT DF_Rol_PuedeModificarUsuariosPersonas DEFAULT 0;
-- + SP GuardarOActualizarRol (parámetro + UPDATE/INSERT)
```

---

## Nota: mecanismo Persona↔Usuario (bloqueo de inputs existente)

Relevante para el ítem 1 (el flag debe "bloquear inputs y habilitarlos solo para roles con flag activo").

- **Razor (server-side)** `Views/Catalogs/Persona.cshtml:6,35-55`: `estaVinculada = Model.UsuarioId.HasValue`; los `@Html.TextBoxFor` de Nombre/Apellido/Correo/Telefono usan `disabled = "disabled"` cuando `estaVinculada`. `UsuarioId`/`NombreUsuarioVinculado` vienen de `PersonaDTO` (`Catalogos/PersonaDTO.cs:7-8`), poblados por SP `ObtenerPersonaPorId` (LEFT JOIN Usuarios).
- **JS (client-side)** `Persona.cshtml:527-537` `AplicarBloqueoSincronizado()`: deshabilita los 4 campos si `personaUsuarioId > 0 || personaSincronizadoUsuarioId > 0`.
- **Sincronización** `Persona.cshtml:450-525`: modal `modalSincronizarUsuario` lista usuarios no asociados (`u.PersonaId == null`), `ConfirmarSincronizar` copia datos (Nombre/Apellido/Correo/`Celular`→Telefono) y bloquea; el vínculo se persiste **solo al guardar** (`GuardarPersona` → `PostMVC('/Catalogs/VincularPersonaUsuario', ...)`).
- **Vinculación** `CatalogsController.VincularPersonaUsuario` (`Controllers/CatalogsController.cs:796-802`) → `PersonaService` → POST `api/Persona/VincularUsuario` → `DbWrapper.Persona.VincularPersonaUsuario` → SP `VincularPersonaUsuario` (devuelve `-3` si ya vinculada).
- **Mecanismo reutilizable para ítem 1**: el patrón de `disabled` condicional ya existe en `Persona.cshtml`; para el flag se cambia la **condición** de `estaVinculada` a "rol del usuario actual tiene el flag" y se extiende a `Users.cshtml` (que hoy no bloquea nada).

---

## Posibles rompimientos / clarificaciones necesarias

1. **Ítem 1 — ambigüedad de sujeto del flag**: ¿el bloqueo lo gobierna el rol del **usuario logueado** (sesión `tokenCookie.UserID`) o el del **usuario editado**? Afecta `Users.cshtml` (se edita a otro usuario) y `Persona.cshtml` (se edita una persona, no un usuario).
2. **Ítem 1 — `Users.cshtml` sin permisos**: hoy la vista no recibe `ViewBag.Permisos` ni bloquea inputs; habrá que incorporar permisos + flag (cambio mayor de lo que parece).
3. **Ítem 2 — Serial opcional y soft-delete**: definir si unicidad aplica a seriales nulos y si los eliminados lógicamente (`Estatus=0`) liberan el serial.
4. **Ítem 2/3/5 — `basededatosservicedesk.txt` desactualizado**: el dump no refleja `EmpresaId` en `Activo`/`Rol`/`Persona` (drift conocido). Basar la migración en el esquema real hosted, no en el dump.
5. **Ítem 4 — rol de "Fecha" y modo del modal**: confirmar si el modal es solo-lectura (historial) o captura+historial, y si "Fecha" es capturable o auto.
6. **Ítem 5 — solapamiento con `Notas`**: confirmar si `Comentarios` es un campo nuevo distinto de `Notas`.
7. **Ítem 6 — N+1 potencial**: si se consulta rol por rol para el contador, evaluar un SP/endpoint masivo.
8. **Ítem 7 — alcance**: confirmar si el fix de tema oscuro abarca solo el chooser de Permisos o también otros `<style>` inline (p.ej. `Active.cshtml`).
9. **Multi-tenant**: todas las nuevas columnas/tablas deben respetar `EmpresaId` (patrón tenant-estructural).

---

## Anexo: lista de archivos clave

**Entidades** (`ServiceDeskDESIEntities/`)
- `Seguridad/Rol.cs`, `Seguridad/RolPaginaAccion.cs`, `Seguridad/RolPaginaAccionDTO.cs`, `Seguridad/Pagina.cs`, `Seguridad/PermisosViewModel.cs`, `Seguridad/PermisoRequest.cs`, `Seguridad/UsuarioRol.cs`, `Seguridad/TokenCookie.cs`
- `Catalogos/Activo.cs`, `Catalogos/ActivoDTO.cs`, `Catalogos/Persona.cs`, `Catalogos/PersonaDTO.cs`, `Catalogos/PersonaActivo.cs`
- `Autenticacion/Usuario.cs`, `Autenticacion/UsuarioDTO.cs`

**MVC** (`ServiceDeskDESIMVC/`)
- `Views/Catalogs/Active.cshtml`, `Views/Catalogs/Persona.cshtml`, `Views/Catalogs/_AsignarActivoPersona.cshtml`
- `Views/Security/Role.cshtml`, `Views/Security/Permisos.cshtml`, `Views/User/Users.cshtml`
- `Controllers/CatalogsController.cs`, `Controllers/SecurityController.cs`, `Controllers/UserController.cs`, `Controllers/BaseController.cs`
- `DAL/HttpClientConnection.Rol.cs`, `.Activo.cs`, `.Persona.cs`, `.Permisos.cs`
- `Services/RolService.cs`, `ActivoService.cs`, `PersonaService.cs`, `PermisosService.cs`, `PersonaActivoService.cs`
- `Helpers/SessionHelper.cs`, `Helpers/ThemeHelper.cs`, `CSS/Comun/TemplatePage.css`, `Scripts/Comun/Comun.js`, `Views/Shared/_Layout.cshtml`

**WebApi** (`ServiceDeskDESIWebApi/`)
- `Controllers/ActivoController.cs`, `RolController.cs`, `PermisosController.cs`, `PersonaController.cs`, `PersonaActivoController.cs`
- `Services/ActivoServices.cs`, `RolService.cs`, `PermisosService.cs`, `PersonaService.cs`, `PersonaActivoService.cs`
- `DAL/DbWrapper.Activo.cs`, `DbWrapper.Rol.cs`, `DbWrapper.Persona.cs`, `DbWrapper.Permisos.cs`, `DbWrapper.cs` (reflexión `LlenarEntidad`/`ObtenerParametrosSQL`)

**DB** (`openspec/`)
- `basededatosservicedesk.txt` (esquema + SPs, **parcialmente desactualizado**)
- `changes/tenant-estructural/migration.sql` (EmpresaId en 12 tablas)
- `changes/archive/2026-08-26-asignacion-activos/migration.sql` (patrón idempotente de ALTER/CREATE)
