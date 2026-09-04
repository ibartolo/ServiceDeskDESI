# Design: Mejoras de Roles, Activos y Permisos

- **Change**: `mejoras-rol-activos-permisos`
- **Fase**: design
- **Fecha**: 2026-08-31
- **Entradas**: `proposal.md` (D1/D2 autoritativas), `explore.md`, specs `PEU-001..005`, `SUA-001..005`, `MTA-001..006`, `CAM-001..004`.

## Resumen del diseño

Siete mejoras sobre módulos ya desplegados, respetando la arquitectura N-capas existente (MVC → HttpClient → WebApi → DbWrapper por reflexión → SPs). Los principios rectores: **(1)** el ítem 1 se resuelve reutilizando el sistema de Permisos (acción "Editar") sin tocar `Rol`; **(2)** el ítem 4 replica el patrón vertical de `PersonaActivo` (tabla + 2 SPs + partial modal); **(3)** el ítem 6 elimina el N+1 con un único SP agrupado; **(4)** el ítem 7 sobreescribe el `<style>` inline del chooser con reglas `body.dark-theme` en el CSS global.

---

## Decisiones técnicas (tabla)

| # | Decisión | Alternativa rechazada | Razón |
|---|----------|----------------------|-------|
| 1 | Ítem 1 vía `PermisosViewModel` del usuario logueado (sin flag en `Rol`) | Flag `PuedeModificarUsuariosPersonas` en `Rol` | Ya existe `[Permiso("Usuarios")]`/`[Permiso("Personas","Editar")]` server-side; un flag sería un mecanismo paralelo inconsistente |
| 2 | Reutilizar `UsuarioService.ObtenerPermisosParaUsuario()` (ya existente, filtra `PaginaNombre=="Usuarios"`) en vez de crear `ObtenerPermisosParaUsuarios()` | Crear método duplicado | El método ya existe y está sin usar; solo falta poblarlo en `UserController.Users()` |
| 3 | Normalizar `Serial` vacío a `NULL` en el SP (`NULLIF`) | Confiar en `IS NOT NULL` del índice | La vista envía `""` (no `NULL`) al dejar el campo vacío; sin normalizar, dos activos con serial vacío violarían el índice único |
| 4 | Duplicado de serial comparando contra el `EmpresaId` derivado de `@Usuario` (subconsulta) | Añadir `@EmpresaId` a la firma del SP | El SP no recibe `@EmpresaId`; mantiene el patrón de derivación existente en todo el SP |
| 5 | Modal mantenimiento = captura (comentario) + histórico; `Fecha` visible en input `disabled` | Captura de fecha editable | Decisión D2 del usuario: fecha auto `GETDATE()`, no editable, evita validación de fechas |
| 6 | Conteo de páginas con 1 SP agrupado `GROUP BY RolId` | Consulta rol por rol (N+1) | Elimina el bug y el N+1 en una sola ronda |
| 7 | Overrides `body.dark-theme` en `TemplatePage.css` (conservar `<style>` inline light) | Migrar chooser a variables CSS | Menor riesgo; el inline queda como base light y el dark sobreescribe |

---

## Arquitectura por capa (por ítem)

### Ítem 1 — Bloqueo edición Usuarios/Personas (sin flag)

- **MVC** `Controllers/UserController.cs` → `Users(long id=0)`: añadir `var permisos = await _usuarioService.ObtenerPermisosParaUsuario();` y `ViewBag.Permisos = permisos;` antes del `return View(usuario)`. **No** se añade redirect por `!PuedeLeer` (el menú ya filtra la página; el guardado queda cubierto por `[Permiso("Usuarios")]`).
- **MVC** `Services/UsuarioService.cs`: sin cambio (el método `ObtenerPermisosParaUsuario()` ya existe; espejo de `PersonaService.ObtenerPermisosParaPersona()`).
- **Vista** `Views/User/Users.cshtml`: adoptar el patrón estándar (espejo exacto de `Active.cshtml`/`Persona.cshtml`):
  - Cabecera: `var permisos = ViewBag.Permisos as PermisosViewModel;`
  - JS: `var permisosGlobal = @Html.Raw(JsonConvert.SerializeObject(permisos));`
  - Botón Guardar: `@if (permisos != null && ((Model.Id == 0 && permisos.PuedeCrear) || (Model.Id > 0 && permisos.PuedeEditar))) { <input ... onclick="GuardarActualizarUsuario()" /> }`
  - Inputs en modo edición sin permiso: `disabled = "disabled"` cuando `Model.Id > 0 && !permisos.PuedeEditar` (aplicar a `txtNombre`, `txtApellido`, `txtNombreUsuario`, `Correo`, `Contrasena`, `Celular`, `RFC`, `ddlSucursal`, `ddlArea`, `ddlRol`).
  - DataTable `Acciones`: gate de `Editar`/`Eliminar` con `permisosGlobal.PuedeEditar/PuedeEliminar` (hoy no hay gate).
- **Vista** `Views/Catalogs/Persona.cshtml`: extender la condición de los 4 campos (`Nombre/Apellido/Correo/Telefono`) de `estaVinculada` a `estaVinculada || (Model.Id > 0 && !permisos.PuedeEditar)`. También en JS `AplicarBloqueoSincronizado()` añadir `|| (personaIdEdicion > 0 && !permisosGlobal.PuedeEditar)` (nueva variable `personaIdEdicion = @Model.Id`).

**Verificación del patrón**: `PermisosViewModel` = `{ PaginaId, PaginaNombre, Direccion, PuedeLeer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar }`. `Persona.cshtml` ya recibe `ViewBag.Permisos` (línea 5) y `Users.cshtml` lo replicará idéntico.

### Ítem 2 — Serial único por empresa

- **BD** `migration.sql`: índice filtrado (ver Esquema BD).
- **SP** `GuardarOActualizarActivo` (rewrite DROP/CREATE): al inicio del `BEGIN`, antes de las validaciones de tenant:
  ```sql
  SET @Serial = NULLIF(LTRIM(RTRIM(@Serial)), '');
  IF @Serial IS NOT NULL AND EXISTS (
      SELECT 1 FROM Activo
      WHERE Serial = @Serial
        AND Estatus = 1
        AND Id <> @Id
        AND EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
  )
  BEGIN
      SELECT -2;
      RETURN;
  END
  ```
  - **Orden de validación**: (1) duplicado → `-2`; (2) validaciones tenant existentes → `0`; (3) `UPDATE`/`INSERT` → `@Id`/`SCOPE_IDENTITY()`.
- **WebApi** `DAL/DbWrapper.Activo.cs` → `GuardarOActualizarActivo`: tras `ExecuteScalar`, añadir rama antes del chequeo de `0`:
  ```csharp
  var activoIdLong = Convert.ToInt64(activoId);
  if (activoIdLong == -2) { modelResponse.IsSuccess = false;
      modelResponse.Message = "Ya existe un activo con ese No. de Serie"; return modelResponse; }
  if (activoIdLong == 0) { /* mensaje actual de permisos */ }
  ```
- **Vista** `Active.cshtml`: sin cambio (el Swal ya muestra `response.Message` del fallo en `GuardarActualizarActivo`).

### Ítem 3 — SerieLocal

- **Entidad** `ServiceDeskDESIEntities/Catalogos/Activo.cs`: `public string SerieLocal { get; set; }` (el DTO `ActivoDTO` hereda).
- **Vista** `Views/Catalogs/Active.cshtml`: añadir `@Html.TextBoxFor(x => x.SerieLocal, new { @class = "form-control", placeholder = "Serie local" })` + `SerieLocal: $("#SerieLocal").val()` en el objeto `activo`.
- **WebApi** `DAL/DbWrapper.Activo.cs` → `parametrosObj`: añadir `a.SerieLocal` (la reflexión `ObtenerParametrosSQL` genera `@SerieLocal`).
- **SP** `GuardarOActualizarActivo`: añadir `@SerieLocal NVARCHAR(100) = NULL` a la firma y al `UPDATE`/`INSERT`.
- **Lectura**: auto vía `a.*` (confirmado en `ObtenerActivos`/`ObtenerActivoPorId`).
- **Validación** `WebApi/Services/ActivoServices.cs`: opcional `if (activo.SerieLocal != null && activo.SerieLocal.Length > 100)`.

### Ítem 4 — Mantenimientos (espejo de PersonaActivo)

**Entidad** `ServiceDeskDESIEntities/Catalogos/Mantenimiento.cs` (nuevo; hereda `BaseObject`):
```csharp
public class Mantenimiento : BaseObject {
    public long ActivoId { get; set; }
    public string Comentario { get; set; }
    public DateTime Fecha { get; set; }
    public long EmpresaId { get; set; }
}
```
Sin DTO (el historial mapea `m.*` → `Mantenimiento`).

**BD** (ver Esquema BD): tabla `Mantenimiento` + SPs `GuardarMantenimiento`, `ObtenerMantenimientosPorActivo`.

**WebApi**:
- `DAL/DbWrapper.Mantenimiento.cs` (nuevo partial): `GuardarMantenimiento(Mantenimiento m, string usuario)` → `ExecuteScalar("GuardarMantenimiento", ...)`; `ObtenerMantenimientosPorActivo(long activoId, string usuario)` → `GetObjects("ObtenerMantenimientosPorActivo", ...)` con `LlenarEntidad<Mantenimiento>`.
- `Services/MantenimientoService.cs` (nuevo): validar `ActivoId > 0`, `Comentario` requerido/`<=500`, `CreadoPor`/`usuario` requeridos.
- `Controllers/MantenimientoController.cs` (nuevo, `[Authorize]`, `RoutePrefix("api/Mantenimiento")`):
  - `[HttpGet, Route("PorActivo/{activoId:long}")] [Permiso("Activos","Leer")] ObtenerMantenimientosPorActivo(long activoId)`
  - `[HttpPost, Route("Guardar")] [Permiso("Activos","Editar")] GuardarMantenimiento(Mantenimiento m)` — asigna `m.EmpresaId`/`m.Fecha` no se setean (lo hace el SP vía `GETDATE()` y derivación de empresa).

**MVC**:
- `DAL/HttpClientConnection.Mantenimiento.cs` (nuevo partial): `ObtenerMantenimientosPorActivo(long)` → GET `api/Mantenimiento/PorActivo/{id}`; `GuardarMantenimiento(Mantenimiento)` → POST `api/Mantenimiento/Guardar` (con `MappingColumSecurity`).
- `Services/MantenimientoService.cs` (nuevo): orquestación.
- `Controllers/CatalogsController.cs` (añadir región "Mantenimiento"): `[HttpGet] ObtenerMantenimientosPorActivo(long activoId)` y `[HttpPost][Permiso("Activos","Editar")] GuardarMantenimiento(Mantenimiento m)`.
- `Controllers/CatalogsController.cs` → constructor: inyectar `_mantenimientoService`.

**Vista**:
- `Views/Catalogs/_MantenimientoActivo.cshtml` (nuevo, modal `modalMantenimientoActivo`), espejo de `_AsignarActivoPersona.cshtml`:
  - `input` `Fecha` visible con `value="@DateTime.Now.ToString("yyyy-MM-dd HH:mm")"` + `readonly disabled`.
  - `textarea` comentario (`id="mantenimientoComentario"`).
  - lista historial (`tblMantenimientos`) poblada por `CargarMantenimientos(activoId)`.
  - JS: `AbrirMantenimientos(id)` (setea `activoIdMantenimiento`, llama `CargarMantenimientos`), `GuardarMantenimiento()` (PostMVC + recarga historial).
- `Views/Catalogs/Active.cshtml`:
  - `@Html.Partial("_MantenimientoActivo")` al final del formulario.
  - Botón "Mantenimientos" por fila en la columna `Acciones` del DataTable (junto a Editar/Eliminar, gateado por `permisosGlobal.PuedeLeer`): `onclick="AbrirMantenimientos(' + data + ')"`.

**Tenant end-to-end**: el SP deriva `EmpresaId` desde `@Usuario`; el MVC/WebApi nunca pasa `EmpresaId` explícito (patrón existente en `PersonaActivo`).

### Ítem 5 — Notas → textarea 250

- **Vista** `Active.cshtml`: reemplazar `@Html.TextBoxFor(x => x.Notas, ...)` por `@Html.TextAreaFor(x => x.Notas, new { @class = "form-control", placeholder = "Notas de Activo", rows = 3 })`. En `jquery.validate` añadir regla `"Notas": { maxlength: 250 }` + mensaje. Sin cambio de BD (ya `NVARCHAR(250)`).

### Ítem 6 — Contador de páginas asignadas (bug)

- **BD** SP nuevo `ObtenerConteoPaginasPorRol`:
  ```sql
  CREATE PROCEDURE dbo.ObtenerConteoPaginasPorRol
  AS
  BEGIN
      SET NOCOUNT ON;
      SELECT RolId, COUNT(*) AS TotalPaginas
      FROM RolPaginaAccion
      WHERE Estatus = 1
      GROUP BY RolId;
  END
  ```
- **WebApi**:
  - `DAL/DbWrapper.Permisos.cs`: `ObtenerConteoPaginasPorRol()` → `GetObjects` con `LlenarEntidad<RolConteoPaginasDTO>`.
  - `Services/PermisosService.cs`: `ObtenerConteoPaginasPorRol()`.
  - `Controllers/PermisosController.cs`: `[HttpGet, Route("ConteoPaginasPorRol")]` → `ModelResponse<List<RolConteoPaginasDTO>>`.
- **Entidad** `Seguridad/RolConteoPaginasDTO.cs` (nuevo, simple): `{ long RolId; int TotalPaginas; }`.
- **MVC**:
  - `DAL/HttpClientConnection.Permisos.cs`: `ObtenerConteoPaginasPorRol()` → GET `api/Permisos/ConteoPaginasPorRol`.
  - `Services/PermisosService.cs`: wrapper.
  - `Controllers/SecurityController.cs`: `public async Task<string> ConsultarConteoPaginasPorRol()` → serializa el response.
- **Vista** `Permisos.cshtml` JS:
  - `var conteoByRol = {};`
  - `CargarRoles()`: tras `tablaRoles.draw()`, llamar `CargarConteoPaginas()`.
  - `CargarConteoPaginas()`: GET `/Security/ConsultarConteoPaginasPorRol`; `result.Response.forEach(c => conteoByRol[c.RolId] = c.TotalPaginas);` luego `ActualizarBadges()`.
  - `ActualizarBadges()`: si `rolId === rolSeleccionadoId` → `paginasByRol.asignadas.length` (vivo); si no → `conteoByRol[rolId] || 0`.
  - Render de columna "Páginas Asignadas" en `InicializarTablaRoles`: conservar `<span class="badge-paginas">0</span>` (se actualiza por `ActualizarBadges`).

### Ítem 7 — Tema oscuro del chooser

- **CSS** `CSS/Comun/TemplatePage.css`: añadir bloque `body.dark-theme` (al final de la sección TEMA OSCURO), espejando las variables ya definidas (`--primary:#6d8df0`, fondos `#1e1e2e`/`#232334`, bordes `#3a3a52`, texto `#d5d7e3`/`#9aa0b5`):
  - `.chooser-column` → `background:#1e1e2e; border-color:#3a3a52;`
  - `.chooser-item` → `background:#232334; border-color:#3a3a52;` (`.chooser-item:hover` → `border-color:var(--primary)`)
  - `.item-nombre` → `color:#e4e6ef`
  - `.item-direccion`, `.empty-message`, `.text-muted-small` → `color:#9aa0b5`
  - `.badge-paginas` → `background:var(--primary); color:#fff`
  - `.chooser-item.disponible` → `border-left-color:#9aa0b5`; `.chooser-item.asignada` → `border-left-color:var(--primary)`
  - `.permisos-checkboxes` → `color:#d5d7e3`; `.permisos-checkboxes input[type="checkbox"]` → `accent-color:var(--primary);`
  - (los checkboxes usan clase `permiso-check`, no `form-check-input`, por eso se estilan explícitamente).

---

## Esquema de BD (migration.sql — idempotente, contra esquema real hosted)

```sql
-- 1. SerieLocal (ítem 3)
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.Activo') AND name = N'SerieLocal')
    ALTER TABLE dbo.Activo ADD SerieLocal NVARCHAR(100) NULL;
GO

-- 2. Índice único filtrado por empresa (ítem 2)
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_Activo_EmpresaSerial'
                 AND object_id = OBJECT_ID(N'dbo.Activo'))
    CREATE UNIQUE INDEX UX_Activo_EmpresaSerial ON dbo.Activo (EmpresaId, Serial)
        WHERE Serial IS NOT NULL AND Estatus = 1;
GO

-- 3. Tabla Mantenimiento (ítem 4)
IF OBJECT_ID(N'dbo.Mantenimiento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Mantenimiento (
        Id                BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ActivoId          BIGINT NOT NULL,
        Comentario        NVARCHAR(500) NOT NULL,
        Fecha             DATETIME NOT NULL,          -- auto GETDATE()
        CreadoPor         NVARCHAR(25) NOT NULL,
        FechaCreacion     DATETIME NOT NULL,
        ModificadoPor     NVARCHAR(25) NULL,
        FechaModificacion DATETIME NULL,
        Estatus           BIT NOT NULL CONSTRAINT DF_Mantenimiento_Estatus DEFAULT (1),
        EmpresaId         BIGINT NOT NULL,
        CONSTRAINT FK_Mantenimiento_Activo FOREIGN KEY (ActivoId) REFERENCES dbo.Activo (Id)
    );
END
GO

-- 4. Rewrite GuardarOActualizarActivo (DROP/CREATE): +@SerieLocal, +chequeo -2, NULLIF serial
--    (firma y cuerpo conforme a la sección Ítem 2/3)

-- 5. SP GuardarMantenimiento
IF OBJECT_ID(N'dbo.GuardarMantenimiento', N'P') IS NOT NULL DROP PROCEDURE dbo.GuardarMantenimiento;
GO
CREATE PROCEDURE dbo.GuardarMantenimiento
(
    @ActivoId      BIGINT,
    @Comentario    NVARCHAR(500),
    @CreadoPor     NVARCHAR(25),
    @FechaCreacion DATETIME,
    @Usuario       NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Activo a
                   WHERE a.Id = @ActivoId AND a.Estatus = 1
                     AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
    BEGIN
        SELECT 0;
        RETURN;
    END
    INSERT INTO Mantenimiento (ActivoId, Comentario, Fecha, CreadoPor, FechaCreacion, Estatus, EmpresaId)
    VALUES (@ActivoId, @Comentario, GETDATE(), @CreadoPor, @FechaCreacion, 1,
            (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1));
    SELECT SCOPE_IDENTITY();
END
GO

-- 6. SP ObtenerMantenimientosPorActivo
IF OBJECT_ID(N'dbo.ObtenerMantenimientosPorActivo', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerMantenimientosPorActivo;
GO
CREATE PROCEDURE dbo.ObtenerMantenimientosPorActivo
(
    @ActivoId BIGINT,
    @Usuario  NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.*
    FROM Mantenimiento m
    INNER JOIN Activo a ON m.ActivoId = a.Id
    WHERE m.ActivoId = @ActivoId AND m.Estatus = 1 AND m.Fecha IS NOT NULL
      AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY m.Fecha DESC;
END
GO

-- 7. SP ObtenerConteoPaginasPorRol (ítem 6)
IF OBJECT_ID(N'dbo.ObtenerConteoPaginasPorRol', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerConteoPaginasPorRol;
GO
CREATE PROCEDURE dbo.ObtenerConteoPaginasPorRol
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RolId, COUNT(*) AS TotalPaginas
    FROM RolPaginaAccion
    WHERE Estatus = 1
    GROUP BY RolId;
END
GO
```

**rollback.sql** (orden inverso): `DROP PROCEDURE` de los 3 SPs nuevos → `DROP TABLE Mantenimiento` → `DROP INDEX UX_Activo_EmpresaSerial` → `DROP COLUMN Activo.SerieLocal` → restaurar definición previa de `GuardarOActualizarActivo`.

**Registro en `.csproj`** (legacy, sin SDK → `<Compile Include>` manual): añadir `Mantenimiento.cs` y `RolConteoPaginasDTO.cs` en `ServiceDeskDESIEntities.csproj`; `DbWrapper.Mantenimiento.cs`, `Services/MantenimientoService.cs`, `Controllers/MantenimientoController.cs` en `ServiceDeskDESIWebApi.csproj`; `HttpClientConnection.Mantenimiento.cs`, `Services/MantenimientoService.cs` en `ServiceDeskDESIMVC.csproj`.

---

## Secuencia de implementación sugerida

1. **BD** (`migration.sql`/`rollback.sql`): SerieLocal + índice + tabla + 4 SPs (incluye rewrite `GuardarOActualizarActivo`). Aplicar y verificar (query `sys.columns`/`sys.indexes`/`sys.objects`).
2. **Entidades**: `Activo.SerieLocal`, `Mantenimiento.cs`, `RolConteoPaginasDTO.cs` (+ csproj).
3. **WebApi**: `DbWrapper.Activo.cs` (SerieLocal + `-2`), `DbWrapper.Mantenimiento.cs`, `MantenimientoService`, `MantenimientoController`, `DbWrapper.Permisos.cs`/`PermisosService`/`PermisosController` (conteo).
4. **MVC DAL/Services**: `HttpClientConnection.Mantenimiento.cs`, `MantenimientoService.cs`, `HttpClientConnection.Permisos.cs` (conteo), `PermisosService.cs` (conteo).
5. **MVC Controllers**: `CatalogsController` (mantenimiento + inyectar service), `UserController.Users()` (ViewBag.Permisos), `SecurityController` (conteo).
6. **Vistas**: `Active.cshtml` (SerieLocal + Notas textarea + botón Mantenimientos + partial), `_MantenimientoActivo.cshtml`, `Users.cshtml` (patrón permisos), `Persona.cshtml` (extender disabled), `Permisos.cshtml` (conteo).
7. **CSS**: `TemplatePage.css` (overrides dark-theme chooser).
8. **Verificación**: `ServiceDeskDESI.sln` MSBuild (0 errores) + revisión estática de los 26 escenarios.

---

## Riesgos técnicos

| Riesgo | Prob. | Mitigación |
|--------|-------|------------|
| Serial vacío (`""`) colisiona en el índice único | Alta | `NULLIF(LTRIM(RTRIM(@Serial)),'')` en el SP; el índice excluye `NULL` |
| Duplicados legacy de serial al crear el índice | Media | Pre-paso de dedup/revisión en migración; el SP devuelve `-2` antes de fallar |
| `-2` colisiona con retornos del SP (hoy `0`/`@Id`/`SCOPE_IDENTITY`) | Baja | Rama explícita `-2` antes de `0` en `DbWrapper`; sin colisión real |
| `Users.cshtml` sin `ViewBag.Permisos` exige cambio mayor | Media | Replicar patrón exacto de `Active.cshtml`/`Persona.cshtml` |
| `ObtenerConteoPaginasPorRol` sin filtro tenant (`RolPaginaAccion` no tiene `EmpresaId`) | Baja | Sin fuga: `Permisos()` solo renderiza roles de `ObtenerTodosLosRoles()` (scoped por empresa) |
| Nuevas entidades/partials no compiladas (csproj legacy) | Media | `<Compile Include>` manual en los 3 csproj |
| Ítem 7 deja otros `<style>` inline sin dark-theme (ej. `.is-invalid-dropdown`) | Baja | Fuera de alcance explícito (proposal) |

---

## Open questions / supuestos

- [ ] Ítem 1: `Users()` no tiene `[Permiso("Usuarios")]` ni gate de lectura (a diferencia de `Active`/`Persona`). Diseño asume **no** añadir redirect `!PuedeLeer` (solo gate de UI + `[Permiso]` en el guardado ya existente). Confirmar si se desea el redirect por consistencia.
- [ ] Ítem 4: el permiso del modal se reutiliza como `Activos` (Leer para historial, Editar para guardar). Confirmar que no se requiere una página "Mantenimientos" propia en el chooser.
- [ ] Ítem 2: la normalización de serial vacío→NULL es una decisión de diseño (el índice `WHERE Serial IS NOT NULL` requiere que el vacío no cuente). Confirmar aceptación.
- [ ] Ítem 6: `TotalPaginas` cuenta filas de `RolPaginaAccion` (una por página asignada, con al menos `PuedeLeer`), no páginas con acciones específicas. Coincide con la semántica de "páginas asignadas".
