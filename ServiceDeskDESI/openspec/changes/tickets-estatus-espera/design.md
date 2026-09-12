# Design: Estatus de espera por dependencia externa

## Technical Approach

Extender la máquina de estados de tickets (SP unificado `TransicionarTicket`) con 3 movimientos nuevos: `PendienteMateriales` (2→6), `EnEsperaTerceros` (2→7) y `Reanudar` (6/7→2). El agente asignado es el único que pausa y reanuda. La pausa exige comentario (motivo) de 1..300 y admite fecha estimada opcional, almacenada en `TicketAsignacion.FechaEstimada` (histórico). Desde 6/7 no se permiten Resolver/Rechazar/Cerrar/Reasignar. El frontend agrega el botón "Pausar" (modal con motivo + comentario + fecha) y el botón "Reanudar". El dashboard cuenta 6/7 como activos.

---

## 1. DB Migration

Archivo: `openspec/changes/tickets-estatus-espera/migration.sql` (idempotente).

```sql
-- 1. Catálogo: estatus 6 y 7 (identidad explícita)
IF NOT EXISTS (SELECT 1 FROM [dbo].[TicketEstatus] WHERE [Id] = 6)
BEGIN
    SET IDENTITY_INSERT [dbo].[TicketEstatus] ON;
    INSERT INTO [dbo].[TicketEstatus] ([Id],[Nombre],[Descripcion],[Color],[Orden],[CreadoPor],[FechaCreacion],[Estatus])
    VALUES (6, N'Pendiente de Materiales', N'...', N'#fd7e14', 6, N'sistema', GETDATE(), 1);
    SET IDENTITY_INSERT [dbo].[TicketEstatus] OFF;
END
-- idem Id=7 "En Espera de Terceros" #6f42c1, Orden 7

-- 2. Columna aditiva en TicketAsignacion
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TicketAsignacion]') AND name = 'FechaEstimada')
    ALTER TABLE [dbo].[TicketAsignacion] ADD [FechaEstimada] DATE NULL;
```

Notas: `FechaEstimada` es nullable (rollback = `DROP COLUMN`). Los estatus se insertan con `Id` explícito porque `TransicionarTicket` mapea a 6 y 7 en duro.

---

## 2. SP `TransicionarTicket` (modificado)

Se recrea preservando el cuerpo vigente (definido en `tickets-ciclo-vida/migration.sql`) y se añaden:

- Parámetro `@FechaEstimada DATE = NULL` (default ⇒ no rompe llamadas existentes).
- Validación de pausa: agente `EsAgente=1`, `@EstatusActual=2`, asignación activa del usuario, comentario `1..300`.
- Validación de reanudación: agente `EsAgente=1`, `@EstatusActual IN (6,7)`, asignación activa del usuario.
- Ramas del `CASE @Resultado`: `PendienteMateriales`→6, `EnEsperaTerceros`→7, `Reanudar`→2.
- `INSERT` del histórico incluye `FechaEstimada`; solo las pausas la conservan (si no, se fuerza `NULL`).

```sql
-- Pausa: agente dueño, desde En Progreso, comentario obligatorio
IF @TipoMovimiento IN ('PendienteMateriales','EnEsperaTerceros') AND NOT (
       @EsAgente = 1
       AND @EstatusActual = 2
       AND EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId=@TicketId AND EsActiva=1 AND Estatus=1 AND UsuarioId=@UsuarioId)
       AND @Comentario IS NOT NULL AND LEN(LTRIM(RTRIM(@Comentario))) BETWEEN 1 AND 300
   ) BEGIN SELECT 0; RETURN; END

-- Reanudar: agente dueño, desde 6/7
IF @TipoMovimiento = 'Reanudar' AND NOT (
       @EsAgente = 1
       AND @EstatusActual IN (6,7)
       AND EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId=@TicketId AND EsActiva=1 AND Estatus=1 AND UsuarioId=@UsuarioId)
   ) BEGIN SELECT 0; RETURN; END

SET @Resultado = CASE @TipoMovimiento
    WHEN 'Tomar' THEN 2 WHEN 'Resolver' THEN 3 WHEN 'Retomar' THEN 2
    WHEN 'Cerrar' THEN 5 WHEN 'Rechazar' THEN 4 WHEN 'Reasignar' THEN 2
    WHEN 'PendienteMateriales' THEN 6 WHEN 'EnEsperaTerceros' THEN 7 WHEN 'Reanudar' THEN 2 END;

IF @TipoMovimiento NOT IN ('PendienteMateriales','EnEsperaTerceros') SET @FechaEstimada = NULL;
```

`@AgenteFinal = @UsuarioId` y `@EsActiva = 1` para pausa/reanudación (el agente conserva la asignación). El contrato escalar (0 = fallo, >0 = id de asignación) se conserva.

> **GOTCHA**: `TipoMovimiento` es `NVARCHAR(20)`. `PendienteMateriales` = 19 y `EnEsperaTerceros` = 15 → caben. No acortar ni cambiar el tipo.

### 2.1 `ObtenerTickets` / `ObtenerTicketsPorArea` (modificar)

Añadir `ta.FechaEstimada` al `SELECT` (la asignación activa ya está en el `LEFT JOIN ta ... EsActiva=1`). Sin otros cambios.

### 2.2 `ObtenerTicketAsignaciones` (modificar)

Añadir `ta.FechaEstimada` al `SELECT`.

### 2.3 `ObtenerIndicadoresDashboard` (modificar)

`ActivosSemana` pasa de `TicketEstatusId IN (1,2)` a `IN (1,2,6,7)`. `Trabajando` permanece `t.TicketEstatusId = 2`.

---

## 3. Entities / DTO

Se modifican archivos existentes (sin nuevos `.cs` ⇒ **sin cambios en el csproj**):

| Archivo | Cambio |
|---|---|
| `Tickets/TicketAsignacion.cs` | `public DateTime? FechaEstimada { get; set; }` |
| `Tickets/TicketDTO.cs` | `public DateTime? FechaEstimada { get; set; }` |

`TicketAsignacionDTO : TicketAsignacion` hereda `FechaEstimada` (no requiere cambio). `LlenarEntidad<T>` mapea por nombre → los alias coinciden.

---

## 4. WebApi Changes

### `DAL/DbWrapper.Ticket.cs`

Dos métodos nuevos que llaman a `TransicionarTicket` (mismo patrón que `ResolverTicket`):

- `PausarTicket(long ticketId, string usuario, string comentario, DateTime? fechaEstimada, string tipoMovimiento)`
  → `@TicketId`, `@TipoMovimiento`, `@Comentario`, `@NuevoUsuarioId = DBNull`, `@FechaEstimada`, `@Usuario`.
- `ReanudarTicket(long ticketId, string usuario, string comentario)`
  → `@TipoMovimiento = 'Reanudar'`, `@Comentario`, `@NuevoUsuarioId = DBNull`, `@FechaEstimada = DBNull`, `@Usuario`.

### `Services/TicketService.cs` (WebApi)

- `PausarTicket(long ticketId, string usuario, string comentario, DateTime? fechaEstimada, string tipoMovimiento)`: valida `ticketId>0`, `usuario` no vacío, `tipoMovimiento IN ('PendienteMateriales','EnEsperaTerceros')`, comentario requerido `1..300` (`IsSuccess=false` + mensaje).
- `ReanudarTicket(long ticketId, string usuario, string comentario)`: valida `ticketId>0`, `usuario` no vacío.

### `Controllers/TicketController.cs` (WebApi)

```csharp
[Permiso("Tickets", "Editar")] [HttpPost, Route("Pausar")]
public ModelResponse PausarTicket([FromBody] PausarTicketRequest request)

[Permiso("Tickets", "Editar")] [HttpPost, Route("Reanudar")]
public ModelResponse ReanudarTicket([FromBody] TransicionTicketRequest request)
```

Las clases request (`TomarTicketRequest`, `ReasignarTicketRequest`, `TransicionTicketRequest`, `PausarTicketRequest`) MUST residir en `ServiceDeskDESIWebApi/Models/` (NO dentro del controller) y registrarse en `ServiceDeskDESIWebApi.csproj` (lista `<Compile Include>` explícita). `PausarTicketRequest` incluye `TicketId`, `TipoPausa` (`"Materiales"` | `"Terceros"`), `Comentario` y `FechaEstimada`.

El controller mapea `TipoPausa` → `TipoMovimiento`: `"Materiales"` → `"PendienteMateriales"`, `"Terceros"` → `"EnEsperaTerceros"`.

---

## 5. MVC Changes

### `Controllers/TicketController.cs`

Acciones espejo (mismo patrón `string` JSON que `ResolverTicket`):

```csharp
[HttpPost][Permiso("Tickets","Editar")] PausarTicket(long ticketId, string tipoPausa, string comentario, DateTime? fechaEstimada)
[HttpPost][Permiso("Tickets","Editar")] ReanudarTicket(long ticketId, string comentario)
```

### `DAL/HttpClientConnection.Ticket.cs` + `Services/TicketService.cs`

Espejo de los métodos HTTP: `PausarTicket(ticketId, tipoPausa, comentario, fechaEstimada)` (POST `{ TicketId, TipoPausa, Comentario, FechaEstimada }`) y `ReanudarTicket(ticketId, comentario)` (POST `{ TicketId, Comentario }`).

---

## 6. Frontend (Views / JS)

### 6.1 Matriz de botones (adición a la existente)

| Botón | Condición | Acción |
|---|---|---|
| Pausar | `esAgenteGlobal && row.AgenteId === usuarioActualIdGlobal && row.TicketEstatusId === 2` | abre `_PausarTicket` |
| Reanudar | `esAgenteGlobal && row.AgenteId === usuarioActualIdGlobal && (row.TicketEstatusId === 6 \|\| row.TicketEstatusId === 7)` | confirm + `PostMVC('/Ticket/ReanudarTicket', { ticketId, comentario })` |

### 6.2 Render del estatus

En la columna Estatus, si `row.TicketEstatusId === 6 || row.TicketEstatusId === 7` y `row.FechaEstimada`, añadir una línea secundaria `Est. respuesta: {fecha}` bajo el badge.

### 6.3 `_PausarTicket.cshtml` (nuevo, modal)

Form con: `ddlTipoPausa` (Pendiente de Materiales / En Espera de Terceros), `comentarioPausa` (textarea obligatorio, `maxlength=300`), `fechaEstimadaPausa` (`<input type="date">`, opcional), hidden `ticketIdPausar`. Submit → `PostMVC('/Ticket/PausarTicket', { ticketId, tipoPausa, comentario, fechaEstimada })`; éxito → cerrar + refrescar tabla. Validación cliente: motivo y comentario requeridos; comentario ≤300.

### 6.4 `_DetalleTicket.cshtml`

Añadir columna **"Fecha estimada"** a `tblHistorial` (data `FechaEstimada`, formato fecha o `---`).

> Bootstrap: el layout usa **Bootstrap 5.3** → usar `data-bs-toggle`/`data-bs-dismiss` y `bootstrap.Modal.getOrCreateInstance`.

---

## 7. Build Considerations

- Sin nuevos `.cs` → no se edita `ServiceDeskDESIEntities.csproj`.
- Los métodos nuevos van en archivos `partial` existentes (`DbWrapper.Ticket.cs`, `HttpClientConnection.Ticket.cs`) y controllers/services existentes.
- El parcial `.cshtml` nuevo (`_PausarTicket.cshtml`) se incluye solo (compilación runtime / AspnetCompileMerge).
- Compilar `ServiceDeskDESI.sln` con MSBuild VS2022 → 0 errores.

---

## 8. Decision Log

| # | Decisión | Alternativas | Por qué |
|---|---|---|---|
| D1 | Dos estatus (6 Materiales, 7 Terceros) | Un único "En Espera" | Semántica y reportería distintas: dependencia de insumo vs. dependencia de proveedor |
| D2 | Movimientos en el SP unificado | SPs separados por movimiento | Reutiliza la invariante "solo la última `EsActiva=1`" y la validación de rol |
| D3 | `FechaEstimada` en `TicketAsignacion` | Columna en `Ticket` | Queda en el histórico por pausa; no ensucia `Ticket` |
| D4 | Comentario obligatorio en pausa, opcional en reanudación | Obligatorio en ambos | La pausa necesita justificar el bloqueo; la reanudación es informativa |
| D5 | Sin Resolver/Rechazar/Cerrar/Reasignar desde 6/7 | Permitir cierre directo | El ticket está bloqueado; primero se reanuda |
| D6 | `ActivosSemana` incluye 6/7; `Trabajando` no | Incluir 6/7 en "Trabajando" | Pausado ≠ trabajando activamente, pero sigue abierto |
| D7 | Un endpoint `Pausar` con `tipoPausa` | Dos endpoints | Menos superficie; el tipo es un dato del request |
| D8 | Capacidad nueva `ticket-estatus-espera` | Modificar `ticket-ciclo-vida` | El delta previo no está archivado; capacidad autocontenida |

---

## 9. Data Flow

```
UI (botón Pausar/Reanudar) ──PostMVC──▶ MVC TicketController ──HttpClient──▶ WebApi TicketController
        │                                                                        │
        │                                                                        ▼
        │                                                               TicketService (valida)
        │                                                                        │
        │                                                                        ▼
        │                                                      DbWrapper → SP TransicionarTicket
        │                                                      (cierra activa → insert histórico + FechaEstimada → update estatus)
        ◀──────────────── ModelResponse JSON ◀─────────────────────────────────────┘
```

---

## 10. Testing Strategy (strict_tdd=false; verificación por build + manual)

| Capa | Qué | Cómo |
|---|---|---|
| DB | Transiciones + invariante `EsActiva` | Ejecutar `migration.sql`; script manual por movimiento |
| WebApi | Endpoints + permisos | Swagger/Postman con token de agente |
| MVC | Botones por rol/estatus, modal, fecha estimada | Prueba manual (agente) |
| Dashboard | `ActivosSemana` incluye 6/7 | Comparar indicador con/sin ticket pausado |
| Build | 0 errores | MSBuild VS2022 sobre `ServiceDeskDESI.sln` |

---

## 11. Rollback

- `DELETE FROM [dbo].[TicketEstatus] WHERE Id IN (6,7)` (solo si no hay tickets en 6/7).
- `ALTER TABLE [dbo].[TicketAsignacion] DROP COLUMN [FechaEstimada]`.
- Restaurar SPs previos (`rollback.sql`: `TransicionarTicket`, `ObtenerTickets`, `ObtenerTicketsPorArea`, `ObtenerTicketAsignaciones`, `ObtenerIndicadoresDashboard`).

---

## Open Questions (resueltas)

- [x] Nombres de los estatus → **6 "Pendiente de Materiales"**, **7 "En Espera de Terceros"** (confirmado por el usuario).
- [x] Salida desde 6/7 → solo **Reanudar** → "En Progreso"; sin Resolver/Rechazar/Cerrar/Reasignar (confirmado).
- [x] Fecha estimada → opcional en la pausa (confirmado).
- [x] Dashboard → 6/7 cuentan como activos (confirmado).
- [x] Comentario de reanudación → opcional (decisión de diseño, no contradicha por el usuario).
