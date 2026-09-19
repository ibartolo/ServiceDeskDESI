# Proposal: Estatus de espera por dependencia externa

- **Change**: `tickets-estatus-espera`
- **Fase**: propose
- **Fecha**: 2026-09-11
- **Origen**: extensión del ciclo de vida de tickets (`tickets-ciclo-vida`). Necesidad de representar tickets abiertos cuyo avance depende de materiales o de proveedores externos, sin negar el servicio.

## Intent

Agregar 2 estatus al catálogo de tickets para representar pausas donde el tiempo de espera **no está bajo control del soporte interno**, manteniendo el ticket abierto y su histórico:

- **6 "Pendiente de Materiales"**: dependemos de una compra/insumo (ej. memoria RAM que tarda 1 semana) o de un mantenimiento de un tercero que aún no se ejecuta.
- **7 "En Espera de Terceros"**: el avance depende de que un proveedor externo responda (ej. soporte del hosting que no contesta); el tiempo de espera no lo controla la empresa.

El agente asignado pausa el ticket desde "En Progreso" con un **motivo obligatorio** (comentario) y una **fecha estimada opcional**; cuando llega el material o responde el tercero, ejecuta **"Reanudar"** y el ticket vuelve a "En Progreso".

## Scope

### In Scope

- Alta de estatus **6** y **7** en `TicketEstatus` (nombre, descripción, color, orden).
- Transiciones nuevas en el SP `TransicionarTicket`: `PendienteMateriales` (2→6), `EnEsperaTerceros` (2→7) y `Reanudar` (6/7→2).
- Columna `TicketAsignacion.FechaEstimada DATE NULL` para la fecha estimada de respuesta.
- Botones **"Pausar"** (con motivo Materiales/Terceros) y **"Reanudar"** en `Views/Ticket/Index.cshtml`; nuevo modal `_PausarTicket.cshtml`.
- Histórico/detalle muestran el motivo, el estatus y la fecha estimada.
- Dashboard: `ActivosSemana` cuenta también los estatus 6 y 7.
- Endpoints/servicios espejo en WebApi y MVC.

### Out of Scope

- Resolver / Rechazar / Cerrar directamente desde 6 o 7 (primero debe reanudarse).
- Reasignar desde 6 o 7 (se mantiene solo en 2 y 4).
- Notificaciones automáticas por vencimiento de la fecha estimada.
- Catálogo administrable de motivos/proveedores (el motivo es texto libre en el comentario).
- SLA/tiempos por proveedor.

## Capabilities

### New Capabilities

- `ticket-estatus-espera`: catálogo de estatus de espera por dependencia externa, transiciones de pausa/reanudación, bloqueo de transiciones directas, registro en histórico, visibilidad de la fecha estimada e impacto en indicadores.

### Modified Capabilities

- `ticket-ciclo-vida` (delta previo no archivado): se extiende con los movimientos `PendienteMateriales`, `EnEsperaTerceros` y `Reanudar` sobre el SP unificado `TransicionarTicket`.

## Approach

- **DB**: `INSERT` idempotente de estatus 6/7 con `SET IDENTITY_INSERT`; `ALTER TABLE TicketAsignacion ADD FechaEstimada DATE NULL`; `TransicionarTicket` con los 3 movimientos nuevos y `@FechaEstimada`; `ObtenerTickets`/`ObtenerTicketsPorArea` exponen `ta.FechaEstimada`; `ObtenerTicketAsignaciones` expone `ta.FechaEstimada`; `ObtenerIndicadoresDashboard` → `ActivosSemana IN (1,2,6,7)`.
- **Entities**: `TicketAsignacion` (base) y `TicketDTO` suman `DateTime? FechaEstimada` (el DTO de asignaciones hereda de la entidad → no requiere cambio).
- **WebApi**: `DbWrapper.Ticket.cs` agrega `PausarTicket`/`ReanudarTicket` (llaman a `TransicionarTicket`); `TicketService` valida (motivo obligatorio ≤300 en pausa); `TicketController` expone `POST Pausar`/`POST Reanudar` con `[Permiso("Tickets","Editar")]` y la clase `PausarTicketRequest`.
- **MVC**: espejo en `HttpClientConnection.Ticket.cs` + `Services/TicketService.cs`; acciones `PausarTicket`/`ReanudarTicket`; botones y render de fecha en `Index.cshtml`; modal `_PausarTicket.cshtml`; columna "Fecha estimada" en `_DetalleTicket.cshtml`.
- **Convenciones**: español; Bootstrap 5.3 (`data-bs-*`); jQuery + DataTables + SweetAlert2; contrato `ModelResponse<T>`; sin nuevos `.cs` → no se edita el `.csproj`.

## Affected Areas

| Área | Impacto | Descripción |
|------|---------|-------------|
| `TicketEstatus` (DB) | Mod (datos) | Alta de estatus 6 y 7 |
| `TicketAsignacion` (DB + entidad + DTO) | Mod | `FechaEstimada` |
| `TransicionarTicket` (DB) | Mod | Movimientos Pausar/Reanudar + `@FechaEstimada` |
| `ObtenerTickets` / `ObtenerTicketsPorArea` / `ObtenerTicketAsignaciones` (DB) | Mod | Exponer `FechaEstimada` |
| `ObtenerIndicadoresDashboard` (DB) | Mod | `ActivosSemana` incluye 6 y 7 |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Ticket.cs` | Mod | `PausarTicket` / `ReanudarTicket` |
| `ServiceDeskDESIWebApi/Services/TicketService.cs` | Mod | Métodos + validación |
| `ServiceDeskDESIWebApi/Controllers/TicketController.cs` | Mod | Endpoints + `PausarTicketRequest` |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.Ticket.cs` | Mod | Espejo HTTP |
| `ServiceDeskDESIMVC/Services/TicketService.cs` | Mod | Espejo |
| `ServiceDeskDESIMVC/Controllers/TicketController.cs` | Mod | Acciones JSON |
| `ServiceDeskDESIMVC/Views/Ticket/Index.cshtml` | Mod | Botones + render fecha estimada |
| `ServiceDeskDESIMVC/Views/Ticket/_PausarTicket.cshtml` | Nuevo | Modal de pausa |
| `ServiceDeskDESIMVC/Views/Ticket/_DetalleTicket.cshtml` | Mod | Columna fecha estimada |

## Risks

| Riesgo | Prob. | Mitigación |
|--------|-------|------------|
| Los IDs 6/7 no coincidan si el `IDENTITY` no está en 5 | Baja | `SET IDENTITY_INSERT` + `IF NOT EXISTS` por `Id` |
| `TipoMovimiento NVARCHAR(20)` insuficiente | Baja | `PendienteMateriales`=19 y `EnEsperaTerceros`=15 caben |
| El SP `TransicionarTicket` en BD diverge del repo | Media | `DROP`/`CREATE` preservando el cuerpo vigente y solo añadiendo validaciones/ramas |
| Romper llamadas existentes al añadir `@FechaEstimada` | Baja | Parámetro con default `= NULL` |
| "Trabajando" mal interpretado con pausados | Baja | Se mantiene = estatus 2; 6/7 no cuentan como "trabajando" |

## Rollback Plan

- `DELETE FROM TicketEstatus WHERE Id IN (6,7)` (solo si no hay tickets en esos estatus).
- `ALTER TABLE TicketAsignacion DROP COLUMN FechaEstimada`.
- Restaurar los SPs previos (`rollback.sql` incluye las versiones de `tickets-ciclo-vida` + dashboard).

## Dependencies

- Requiere que `tickets-ciclo-vida` esté aplicado (SP `TransicionarTicket`, columnas `TipoMovimiento`/`TicketEstatusId`, estatus 4 = "Rechazado"). Ejecutar la migración SQL antes de desplegar el código.

## Success Criteria

- [ ] Estatus 6 y 7 disponibles en el catálogo y en la UI con su color.
- [ ] El agente pausa (En Progreso → 6/7) con motivo obligatorio y fecha estimada opcional; y reanuda (6/7 → En Progreso).
- [ ] El histórico registra `TipoMovimiento`, motivo y fecha estimada.
- [ ] No se permite Resolver/Rechazar/Cerrar/Reasignar desde 6 o 7.
- [ ] `ActivosSemana` del dashboard incluye 6 y 7.
- [ ] `ServiceDeskDESI.sln` compila sin errores (0 errores).
