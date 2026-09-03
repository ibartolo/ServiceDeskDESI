# Design: Ciclo de vida de tickets

## Technical Approach

Convertir el módulo de tickets en una máquina de estados con historial inmutable. Toda transición (Tomar, Reasignar, Resolver, Retomar, Cerrar, Rechazar) se centraliza en **un único SP `TransicionarTicket`** que: valida rol/ownership/estatus, cierra la asignación activa previa, inserta una fila de historial en `TicketAsignacion` (con `TipoMovimiento` + estatus resultante) y actualiza `Ticket.TicketEstatusId`. El frontend queda en tres modales Bootstrap y una tabla con botones por rol/estatus. Captura = solo alta (reusa `GuardarOActualizarTicket`); Editar se elimina; Eliminar se oculta en UI; "Ver" muestra detalle (desde la fila) + historial (`ObtenerTicketAsignaciones`).

---

## 1. DB Migration

Archivo: `openspec/changes/tickets-ciclo-vida/migration.sql` (idempotente).

```sql
-- 1. Renombrar estatus 4
UPDATE [dbo].[TicketEstatus] SET Nombre = 'Rechazado' WHERE Id = 4;

-- 2. Columnas aditivas en TicketAsignacion (nullable)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TicketAsignacion]') AND name = 'TipoMovimiento')
    ALTER TABLE [dbo].[TicketAsignacion] ADD [TipoMovimiento] NVARCHAR(20) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TicketAsignacion]') AND name = 'TicketEstatusId')
    ALTER TABLE [dbo].[TicketAsignacion] ADD [TicketEstatusId] INT NULL;

-- 3. Backfill de filas activas existentes (asignaciones previas = "Tomar")
UPDATE ta SET ta.TipoMovimiento = 'Tomar', ta.TicketEstatusId = t.TicketEstatusId
FROM [dbo].[TicketAsignacion] ta
INNER JOIN [dbo].[Ticket] t ON ta.TicketId = t.Id
WHERE ta.TipoMovimiento IS NULL;

-- 4. SPs (CREATE OR ALTER) — ver sección 2
```

Notas: `TipoMovimiento` NVARCHAR(20) (valores `Tomar|Reasignar|Resolver|Rechazar|Cerrar|Retomar`); `TicketEstatusId` = estatus **resultante** (NO es el bit `Estatus` de soft-delete). Se mantienen NULLABLES para el rollback (`DROP COLUMN`). El `UPDATE TicketEstatus` es reversible.

> **GOTCHA crítico**: la tabla `TicketAsignacion` y los SPs `TomarTicket`/`ReasignarTicket`/`ObtenerTicketAsignaciones`, y el LEFT JOIN de agente en `ObtenerTickets`, **NO están en ningún .sql del repo** (solo viven en la BD). Al usar `CREATE OR ALTER`, preservar el cuerpo actual (JOIN de agente) y solo añadir columnas, no reescribir desde cero.

## 2. SP Definitions

### 2.1 `TransicionarTicket` (nuevo, unificado)

```sql
CREATE OR ALTER PROCEDURE [dbo].[TransicionarTicket]
(
    @TicketId        BIGINT,
    @TipoMovimiento  NVARCHAR(20),   -- Tomar|Resolver|Retomar|Cerrar|Rechazar|Reasignar
    @Comentario      NVARCHAR(300) = NULL,
    @NuevoUsuarioId  BIGINT = NULL,  -- solo Reasignar
    @Usuario         NVARCHAR(25)    -- username del actor
)
AS
BEGIN
    DECLARE @UsuarioId BIGINT, @EmpresaId BIGINT, @EstatusActual INT, @EsAgente BIT, @Resultado INT, @AgenteFinal BIGINT;
    SELECT @UsuarioId = Id, @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @UsuarioId IS NULL BEGIN SELECT 0; RETURN; END

    SELECT @EstatusActual = TicketEstatusId FROM Ticket WHERE Id = @TicketId AND Estatus = 1 AND EmpresaId = @EmpresaId;
    IF @EstatusActual IS NULL BEGIN SELECT 0; RETURN; END

    SET @EsAgente = CASE WHEN EXISTS(
        SELECT 1 FROM UsuarioRol ur INNER JOIN Rol r ON ur.RolId = r.Id
        WHERE ur.UsuarioId = @UsuarioId AND r.PuedeAtenderTickets = 1 AND ur.Estatus = 1 AND r.Estatus = 1) THEN 1 ELSE 0 END;

    -- Validaciones por movimiento (falla => SELECT 0; RETURN)
    IF @TipoMovimiento = 'Tomar' AND NOT (@EsAgente = 1 AND @EstatusActual = 1
        AND NOT EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId = @TicketId AND EsActiva = 1)
        AND EXISTS(SELECT 1 FROM Usuarios WHERE Id = @UsuarioId AND AreaId = (SELECT AreaId FROM Ticket WHERE Id = @TicketId)))
        BEGIN SELECT 0; RETURN; END
    IF @TipoMovimiento = 'Resolver' AND NOT (@EsAgente = 1 AND @EstatusActual = 2
        AND EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId = @TicketId AND EsActiva = 1 AND UsuarioId = @UsuarioId)
        AND @Comentario IS NOT NULL AND LEN(@Comentario) BETWEEN 1 AND 300)
        BEGIN SELECT 0; RETURN; END
    IF @TipoMovimiento = 'Retomar' AND NOT (@EsAgente = 1 AND @EstatusActual = 4)
        BEGIN SELECT 0; RETURN; END
    IF @TipoMovimiento IN ('Cerrar','Rechazar') AND NOT (
        (SELECT CreadoPor FROM Ticket WHERE Id = @TicketId) = @Usuario AND @EstatusActual = 3
        AND (@TipoMovimiento = 'Cerrar' OR (@Comentario IS NOT NULL AND LEN(@Comentario) BETWEEN 1 AND 300)))
        BEGIN SELECT 0; RETURN; END
    IF @TipoMovimiento = 'Reasignar' AND NOT (@NuevoUsuarioId IS NOT NULL AND @EstatusActual IN (2, 4)
        AND EXISTS(SELECT 1 FROM Area WHERE Id = (SELECT AreaId FROM Ticket WHERE Id = @TicketId) AND UsuarioResponsableId = @UsuarioId))
        BEGIN SELECT 0; RETURN; END

    -- Estatus resultante + agente final
    SET @Resultado = CASE @TipoMovimiento WHEN 'Tomar' THEN 2 WHEN 'Resolver' THEN 3 WHEN 'Retomar' THEN 2
        WHEN 'Cerrar' THEN 5 WHEN 'Rechazar' THEN 4 WHEN 'Reasignar' THEN 2 END;
    SET @AgenteFinal = CASE WHEN @TipoMovimiento IN ('Cerrar','Rechazar') THEN NULL
        WHEN @TipoMovimiento = 'Reasignar' THEN @NuevoUsuarioId ELSE @UsuarioId END;

    UPDATE TicketAsignacion SET EsActiva = 0 WHERE TicketId = @TicketId AND EsActiva = 1;
    INSERT INTO TicketAsignacion (TicketId, UsuarioId, Comentario, EsActiva, TipoMovimiento, TicketEstatusId, CreadoPor, FechaCreacion, Estatus, EmpresaId)
    VALUES (@TicketId, @AgenteFinal, @Comentario, 1, @TipoMovimiento, @Resultado, @Usuario, GETDATE(), 1, @EmpresaId);
    UPDATE Ticket SET TicketEstatusId = @Resultado, ModificadoPor = @Usuario, FechaModificacion = GETDATE() WHERE Id = @TicketId;

    SELECT SCOPE_IDENTITY();
END
```

`TomarTicket`/`ReasignarTicket` existentes quedan **reemplazados por llamadas a `TransicionarTicket`** (los SPs viejos se dejan en BD sin usarse; DROP opcional). El contrato escalar (0 = fallo, >0 = id) se conserva.

### 2.2 `ObtenerUsuariosArea` (nuevo)

```sql
CREATE OR ALTER PROCEDURE [dbo].[ObtenerUsuariosArea]
(
    @AreaId BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT DISTINCT u.*, a.Nombre as AreaNombre
    FROM Usuarios u
    INNER JOIN Area a ON u.AreaId = a.Id
    INNER JOIN UsuarioRol ur ON ur.UsuarioId = u.Id AND ur.Estatus = 1
    INNER JOIN Rol r ON ur.RolId = r.Id AND r.PuedeAtenderTickets = 1 AND r.Estatus = 1
    WHERE u.AreaId = @AreaId AND u.Estatus = 1
      AND u.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
```

### 2.3 `ObtenerTickets` / `ObtenerTicketsPorArea` (modificar)

Añadir al SELECT (preservando el LEFT JOIN de agente existente) la columna del creador:
```sql
u.Id as CreadoPorId
```
Aplicar a las 5 variantes (`ObtenerTickets`, `ObtenerTicketsPorArea`, `ObtenerTicketsPorUsuario`, `ObtenerTicketsPorUrgencia`, `ObtenerTicketsPorEstatus`) por consistencia; mínimo requerido: `ObtenerTickets` + `ObtenerTicketsPorArea`.

### 2.4 `ObtenerTicketAsignaciones` (modificar)

Añadir `ta.TipoMovimiento`, `ta.TicketEstatusId`, y `te.Nombre as EstatusNombre`, `te.Color as EstatusColor` (JOIN a `TicketEstatus te ON ta.TicketEstatusId = te.Id`).

## 3. Entities / DTO

Modificar archivos existentes (sin nuevos .cs → **sin cambios en el csproj**):

| Archivo | Cambio |
|---|---|
| `Tickets/TicketAsignacion.cs` | `public string TipoMovimiento { get; set; }` + `public int? TicketEstatusId { get; set; }` |
| `Tickets/TicketAsignacionDTO.cs` | `public string EstatusNombre { get; set; }` + `public string EstatusColor { get; set; }` (para el historial) |
| `Tickets/TicketDTO.cs` | `public long? CreadoPorId { get; set; }` (ownership solicitante) |

`LlenarEntidad<T>` mapea columnas por nombre → los nombres coinciden.

## 4. WebApi Changes

### `DAL/DbWrapper.Ticket.cs`
- `TomarTicket`/`ReasignarTicket`: cambiar `ExecuteScalar("TomarTicket"...)` → `ExecuteScalar("TransicionarTicket"...)` con `TipoMovimiento='Tomar'/'Reasignar'` (+ `@NuevoUsuarioId` en Reasignar).
- Nuevos: `ResolverTicket`, `RechazarTicket`, `CerrarTicket`, `RetomarTicket` (todos `ExecuteScalar("TransicionarTicket"...)`).
- `ObtenerUsuariosArea(long areaId, string usuario)` → `GetObjects("ObtenerUsuariosArea"...)` → `List<UsuarioDTO>`.

### `Services/TicketService.cs`
Métodos nuevos con validación: `ResolverTicket(ticketId, usuario, comentario)`, `RechazarTicket(...)`, `CerrarTicket(ticketId, usuario)`, `RetomarTicket(ticketId, usuario)`, `ObtenerUsuariosArea(areaId, usuario)`. Validar comentario requerido + `<=300` en Resolver/Rechazar (retornar `IsSuccess=false` con mensaje).

### `Controllers/TicketController.cs` (WebApi)
```csharp
[Permiso("Tickets", "Editar")] [HttpPost, Route("Resolver")]  ResolverTicket([FromBody] TransicionTicketRequest r)
[Permiso("Tickets", "Leer")]    [HttpPost, Route("Cerrar")]    CerrarTicket([FromBody] TransicionTicketRequest r)
[Permiso("Tickets", "Leer")]    [HttpPost, Route("Rechazar")]  RechazarTicket([FromBody] TransicionTicketRequest r)
[Permiso("Tickets", "Editar")]  [HttpPost, Route("Retomar")]   RetomarTicket([FromBody] TransicionTicketRequest r)
[HttpGet, Route("UsuariosArea/{areaId:long}")]                 ObtenerUsuariosArea(long areaId) → ModelResponse<List<UsuarioDTO>>
```
`public class TransicionTicketRequest { public long TicketId; public string Comentario; public long? NuevoUsuarioId; }` (en el propio controller, igual que `TomarTicketRequest`). `Tomar`/`Reasignar` conservan sus request classes existentes.

## 5. MVC Changes

### `Controllers/TicketController.cs`
- `Index`: añadir campos `_usuarioService` (nuevo). Tras `ViewBag.EsAgente`:
```csharp
var usuarioActual = await _usuarioService.ObtenerUsuarioPorId(tokenCookie.UserID);
ViewBag.UsuarioActualId = tokenCookie.UserID;
ViewBag.UsuarioActualNombre = tokenCookie.UserName;
bool esResponsableArea = false;
if (usuarioActual?.AreaId != null) {
    var areaActual = await _areaService.ObtenerAreaPorId(usuarioActual.AreaId.Value);
    esResponsableArea = areaActual != null && areaActual.UsuarioResponsableId == tokenCookie.UserID;
}
ViewBag.EsResponsableArea = esResponsableArea;
```
- Nuevas acciones (mismo patrón `string` JSON que las existentes):
```csharp
[HttpPost][Permiso("Tickets","Editar")] ResolverTicket(long ticketId, string comentario)
[HttpPost][Permiso("Tickets","Leer")]    CerrarTicket(long ticketId)
[HttpPost][Permiso("Tickets","Leer")]    RechazarTicket(long ticketId, string comentario)
[HttpPost][Permiso("Tickets","Editar")]  RetomarTicket(long ticketId)
[HttpGet]                                ObtenerUsuariosArea(long areaId)
```
- Eliminar `CambiarEstatusTicket` (viola "todo cambio queda en historial", sin uso en UI). `GuardarOActualizarTicket` se conserva solo para CREAR (rama edición queda muerta desde UI). `ObtenerTicketAsignaciones` ya existe (historial).

### `DAL/HttpClientConnection.Ticket.cs` + `Services/TicketService.cs`
Espejo de los nuevos métodos: `ResolverTicket`, `RechazarTicket`, `CerrarTicket`, `RetomarTicket` (POST con `{ ticketId, comentario }`), `ObtenerUsuariosArea(long areaId)` (GET).

## 6. Frontend (Views / JS)

### 6.1 `Index.cshtml` (reescribir)
- Quitar el `frmTicket` inline (líneas 25–108) y el formulario de edición; dejar solo filtro de área + tabla + botón "Nuevo Ticket".
- Inyectar globals JS: `esAgenteGlobal`, `usuarioActualIdGlobal`, `esResponsableAreaGlobal` (serializados desde ViewBag).
- DataTable: columna "Agente" y badge de estatus se conservan. Añadir `createdRow`:
```js
createdRow: function (row, data) {
    if (data.AgenteId && data.AgenteId === usuarioActualIdGlobal) { $(row).addClass('ticket-mio'); }
}
```
- `<style>.ticket-mio { background-color: #e8f4ff !important; }</style>` (o en `TemplatePage.css`).
- Botones en Acciones (matriz abajo); "Editar" eliminado, "Eliminar" no se renderiza, "Ver" siempre.

### 6.2 Matriz de botones (JS, columna Acciones)
| Botón | Condición |
|---|---|
| Tomar | `esAgenteGlobal && row.TicketEstatusId === 1 && !row.AgenteId` |
| Ver | siempre |
| Resolver | `esAgenteGlobal && row.AgenteId === usuarioActualIdGlobal && row.TicketEstatusId === 2` |
| Retomar | `esAgenteGlobal && row.TicketEstatusId === 4` |
| Cerrar | `row.CreadoPorId === usuarioActualIdGlobal && row.TicketEstatusId === 3` |
| Rechazar | `row.CreadoPorId === usuarioActualIdGlobal && row.TicketEstatusId === 3` |
| Reasignar | `esResponsableAreaGlobal && (row.TicketEstatusId === 2 || row.TicketEstatusId === 4)` |

Comentario obligatorio (Resolver/Rechazar): SweetAlert2 con `input:'textarea'` + `inputValidator` (requerido, `<=300`). Retomar/Cerrar: confirm simple. Reasignar: abre modal con comentario opcional.

### 6.3 `_CapturarTicket.cshtml` (nuevo, modal)
Form `id="frmCapturaTicket"`: Área (`ddlArea`), Categoría (`ddlCategoria`), Subcategoría (`ddlSubcategoria`) en cascada (reutilizar `/Ticket/ObtenerCategoriasPorArea` y `/Ticket/ObtenerSubcategoriasPorCategoria`), Urgencia (ddl 1–4), Título (max 250), Descripción (textarea). **Sin** dropdown de estatus ni `Id`. Botón "Nuevo Ticket" abre (`$('#modalCapturarTicket').modal('show')`) con reset de campos + cascadas limpias. Submit valida (jquery.validate) y `PostMVC('/Ticket/GuardarOActualizarTicket', { ... , TicketEstatusId: 1 })`; éxito → cerrar+resetear modal + refrescar tabla.

### 6.4 `_ReasignarTicket.cshtml` (nuevo, modal)
Dropdown `ddlUsuarioReasignar` (se carga con `ObtenerUsuariosArea(row.AreaId)`), comentario opcional (`maxlength=300`), hidden `ticketId`. Submit → `PostMVC('/Ticket/ReasignarTicket', { ticketId, nuevoUsuarioId, comentario })`.

### 6.5 `_DetalleTicket.cshtml` (nuevo, modal — adición al proposal)
Detalle solo lectura (Título, Área, Categoría, Subcategoría, Urgencia, Estatus, Fecha creación, CreadoPor, Agente, Descripción) poblado desde `row` de la tabla (sin endpoint extra). Historial: `GetMVC('/Ticket/ObtenerTicketAsignaciones?ticketId=' + id)` → tabla (Fecha, TipoMovimiento, Agente, Comentario, Estatus resultante con badge).

> Bootstrap: el layout carga **Bootstrap 5.3** (aunque el prompt diga "4"): usar `data-bs-toggle="modal"` / `data-bs-dismiss="modal"` (no `data-dismiss`).

## 7. Build Considerations
- Sin nuevos `.cs` en `ServiceDeskDESIEntities` → **no se edita el csproj**. Solo se modifican archivos de entidad existentes.
- Métodos nuevos van en archivos `partial` existentes (`DbWrapper.Ticket.cs`, `HttpClientConnection.Ticket.cs`) y controllers/services existentes → sin cambios de proyecto.
- Parciales `.cshtml` bajo `Views/Ticket/` se incluyen solos (compilación runtime / AspnetCompileMerge en Release).
- Compilar `ServiceDeskDESI.sln` con MSBuild VS2022 → 0 errores.

## 8. Decision Log

| # | Decisión | Alternativas | Por qué |
|---|---|---|---|
| D1 | Un SP `TransicionarTicket` para las 6 transiciones | 6 SPs separados (Tomar/Resolver/Rechazar/Cerrar/Retomar/Reasignar) | Invariante "solo la última EsActiva=1" en un solo punto; validación de rol/ownership única; menos duplicación |
| D2 | `TipoMovimiento` NVARCHAR(20) + `TicketEstatusId` (resultante) en `TicketAsignacion` | Tabla `TicketHistorial` nueva; enum INT | Reusa `ObtenerTicketAsignaciones`; strings legibles en historial; `TicketEstatusId` evita ambigüedad con bit `Estatus` |
| D3 | Solicitante cierra asignación (UsuarioId=NULL en Cerrar/Rechazar) | Mantener agente en Cerrar/Rechazar | Tras Cerrar/Rechazar no hay agente "dueño"; libera el ticket para Retomar |
| D4 | `CreadoPorId` (JOIN `u.Id`) para "es propio" | Comparar `CreadoPor` (username) | Robusto ante renombres; consistente con `AgenteId` |
| D5 | `ObtenerUsuariosArea` SP nuevo (área-scoped) | Reusar helper MVC `ObtenerUsuariosQuePuedenAtenderLista` | El helper es privado, N+1 y no filtra por área; SP = 1 roundtrip + filtro área |
| D6 | Cerrar/Rechazar con `[Permiso("Tickets","Leer")]`; agente con `"Editar"` | `"Editar"` para todo | El solicitante suele no tener "Editar"; "Leer" lo tiene todo el que ve la página; el SP es la autorización real |
| D7 | `_DetalleTicket.cshtml` (3er parcial) para "Ver" | HTML inline en Index | Consistencia de modales; historial sustancial |
| D8 | Comentario máx. 300 validado en 3 capas | Solo cliente o solo server | Cliente (SweetAlert2/`maxlength`), Service (throw), SP (`LEN<=300`) |

## 9. Data Flow

```
UI (modal/botón) ──PostMVC──▶ MVC TicketController ──HttpClient──▶ WebApi TicketController
      │                                                              │
      │                                                              ▼
      │                                                     TicketService (valida)
      │                                                              │
      │                                                              ▼
      │                                              DbWrapper → SP TransicionarTicket
      │                                              (close activa → insert historial → update estatus)
      ◀────────────── ModelResponse JSON ◀───────────────────────────┘
```

## 10. Testing Strategy (strict_tdd=false; verificación por build + manual)

| Capa | Qué | Cómo |
|---|---|---|
| DB | Transiciones + invariante EsActiva | Ejecutar `migration.sql`; script manual por movimiento |
| WebApi | Endpoints + permisos | Swagger / Postman con token por rol |
| MVC | Botones por rol/estatus, modales | Prueba manual por rol (agente, responsable, solicitante) |
| Build | 0 errores | MSBuild VS2022 sobre `ServiceDeskDESI.sln` |

## 11. Rollback
- `UPDATE TicketEstatus SET Nombre='Reabierto' WHERE Id=4`.
- `ALTER TABLE TicketAsignacion DROP COLUMN TipoMovimiento, TicketEstatusId`.
- SPs aditivos: quitar endpoints/UI sin tocar datos.

## Open Questions (resueltas)
- [x] Solicitante tiene `Leer` sobre "Tickets" (la acción `Index` ya valida `PuedeLeer` y los solicitantes ven sus propios tickets) → D6 confirmado: `[Permiso("Tickets","Leer")]`.
- [x] Reasignar aplica a "En Progreso" (2) **y** "Rechazado" (4) — no a "Resuelto" (3). Corregido en SP (`@EstatusActual IN (2,4)`) y en la matriz de botones.

> Nota de endurecimiento (opcional): `Retomar` en el SP solo valida `@EsAgente=1 AND @EstatusActual=4`; se recomienda añadir también el chequeo de área (`Usuario.AreaId = Ticket.AreaId`) por consistencia con `Tomar` (defensa en profundidad). El frontend ya limita la lista a tickets del área del agente.

## Correcciones aplicadas durante la migración (desvían de este doc, son la fuente de verdad)

1. **D3 corregida**: `TicketAsignacion.UsuarioId` es `NOT NULL` en BD y `ObtenerTicketAsignaciones` usa `INNER JOIN Usuarios`. Por tanto, en Cerrar/Rechazar **NO** se inserta `UsuarioId=NULL` (rompería el INSERT y el histórico). En su lugar: el actor (solicitante) se registra como `UsuarioId` con `EsActiva = 0` (no queda agente activo). `@AgenteFinal = @NuevoUsuarioId` solo en Reasignar, si no `@UsuarioId`; `@EsActiva = 0` en Cerrar/Rechazar, `1` en el resto.
2. **`ObtenerTicketsPorArea` no tenía el LEFT JOIN de agente** (solo `ObtenerTickets` lo tenía). La migración añadió el LEFT JOIN de agente (`AgenteId`/`AgenteNombre`/`AgenteApellido`/`AgenteNombreUsuario`) **y** `CreadoPorId` a `ObtenerTicketsPorArea`, para que el filtro por área también exponga agente y creador.
3. El `TransicionarTicket` real (ver `migration.sql`) añade chequeo de área también en `Retomar` y en el agente destino de `Reasignar` (defensa en profundidad, no solo la nota opcional).
