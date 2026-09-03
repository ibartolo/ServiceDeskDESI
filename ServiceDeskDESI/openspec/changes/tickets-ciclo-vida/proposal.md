# Proposal: Ciclo de vida de tickets

- **Change**: `tickets-ciclo-vida`
- **Fase**: propose
- **Fecha**: 2026-08-19
- **Origen**: evolución del flujo de asignación de tickets ya implementado (Tomar/Reasignar/Estatus).

## Intent

Convertir el módulo de tickets en un ciclo de vida controlado e inmutable: captura en modal (solo alta), tickets inmutables tras crearlos, detalle de solo lectura con histórico, y transiciones de estatus basadas en rol con comentario (máx. 300) registrado en cada cambio. Renombrar el estatus 4 "Reabierto" → "Rechazado".

## Scope

### In Scope
- Modal Bootstrap `_CapturarTicket.cshtml` + botón "Nuevo Ticket"; el formulario actual solo CREA (se elimina el modo edición).
- Inmutabilidad: quitar botón "Editar"; añadir "Ver" (solo lectura + histórico); ocultar "Eliminar" en UI (backend intacto).
- Resaltado visual "míos vs otros" en la tabla (`AgenteId == usuario actual`) conservando el badge de estatus.
- Renombrar estatus 4 → "Rechazado" (SQL).
- Botones por rol/estatus: Tomar (ya existe), Resolver (En Progreso → Resuelto, con comentario), Retomar (Rechazado → En Progreso), Cerrar (Resuelto → Cerrado), Rechazar (Resuelto → Rechazado, con comentario), Reasignar (modal `_ReasignarTicket.cshtml`).
- Historial unificado: TODO cambio de estatus/asignación queda registrado en `TicketAsignacion`.

### Out of Scope
- Auto-cierre por inactividad (p. ej. 2 días hábiles sin respuesta del solicitante).
- Notificación por email al responsable del área.
- Eliminación real de tickets (solo se oculta el botón en UI).

## Capabilities

### New Capabilities
- `ticket-captura`: captura en modal (solo alta) e inmutabilidad del ticket.
- `ticket-ciclo-vida`: transiciones de estatus por rol + registro de histórico.
- `ticket-detalle`: vista "Ver" (detalle + histórico) y resaltado visual "míos vs otros".

### Modified Capabilities
- None (`openspec/specs/` está vacío; no hay specs existentes).

## Approach

- **Decisión de historial (unificada)**: reutilizar `TicketAsignacion` como registro único de movimientos en lugar de crear `TicketHistorial`. Añadir columnas `TipoMovimiento` (Tomar/Reasignar/Resolver/Rechazar/Cerrar/Retomar) y estatus resultante; solo la última fila queda `EsActiva = true`. Evita tabla nueva y reaprovecha `ObtenerTicketAsignaciones`.
- **Backend (WebApi)**: nuevos SPs `ResolverTicket`, `RechazarTicket`, `CerrarTicket`, `RetomarTicket`, `ObtenerUsuariosArea`; extender `ReasignarTicket` para fijar En Progreso. Actualizar `DbWrapper.Ticket.cs`, `TicketService`, y el `TicketController` del WebApi.
- **Frontend (MVC)**: parciales `_CapturarTicket.cshtml` y `_ReasignarTicket.cshtml` (modal); reescribir `Index.cshtml` (columnas, botones de acción, resaltado); actualizar `HttpClientConnection.Ticket.cs` y `TicketService` MVC.
- **DB**: `UPDATE TicketEstatus SET Nombre='Rechazado' WHERE Id=4` + ALTER aditivo de `TicketAsignacion`.
- **Convenciones**: entidades/dto nuevos deben registrarse en `ServiceDeskDESIEntities.csproj` (old-style); contrato `ModelResponse<T>`; idioma español; Bootstrap 4 + jQuery + DataTables + SweetAlert2.

## Affected Areas

| Área | Impacto | Descripción |
|------|---------|-------------|
| `TicketAsignacion` (DB + entidad + DTO) | Mod | `TipoMovimiento` + estatus resultante |
| `TicketEstatus` (DB) | Mod | Renombrar id 4 → "Rechazado" |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Ticket.cs` | Mod | Nuevos SPs |
| `ServiceDeskDESIWebApi/Services/TicketService.cs` | Mod | Métodos de servicio |
| `ServiceDeskDESIWebApi/Controllers/TicketController.cs` | Mod | Endpoints |
| `ServiceDeskDESIMVC/Views/Ticket/Index.cshtml` | Mod | Modal, botones, resaltado |
| `ServiceDeskDESIMVC/Views/Ticket/_CapturarTicket.cshtml` | Nuevo | Captura en modal |
| `ServiceDeskDESIMVC/Views/Ticket/_ReasignarTicket.cshtml` | Nuevo | Reasignar en modal |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.Ticket.cs` + Services | Mod | Cliente HTTP |
| `ServiceDeskDESIEntities` (.csproj + entities) | Mod | Registrar entidades/DTOs nuevos |

## Risks

| Riesgo | Prob. | Mitigación |
|--------|-------|------------|
| Renombrar estatus 4 rompe referencias/UI que muestran "Reabierto" | Baja | El id se mantiene; auditar UI/SPs que comparen por nombre |
| Ambigüedad `Estatus` (soft-delete bit) vs `TicketEstatusId` en `TicketAsignacion` | Media | Modelar `TipoMovimiento` + estatus resultante explícitos |
| Usuarios acostumbrados a editar tickets | Media | "Ver" + indicar cerrar y crear nuevo |
| Verificar ownership del solicitante (`CreadoPor` = UserName) | Media | Reusar `ObtenerTickets` que ya expone `CreadoPor` |

## Rollback Plan

- SPs nuevos son aditivos: revertir = quitar endpoints/UI sin tocar datos.
- Renombrar estatus reversible: `UPDATE TicketEstatus SET Nombre='Reabierto' WHERE Id=4`.
- ALTER de `TicketAsignacion` es aditivo (columna nullable nueva) → `DROP COLUMN` si se revierte.

## Dependencies

- Ninguna externa. Requiere ejecutar la migración SQL en la BD antes de desplegar.

## Success Criteria

- [ ] El formulario solo crea (modal); "Editar" eliminado; "Ver" muestra detalle + histórico.
- [ ] Transiciones por rol funcionan y todo cambio queda en histórico con comentario (máx. 300).
- [ ] Estatus 4 se muestra "Rechazado" en toda la UI.
- [ ] `ServiceDeskDESI.sln` compila sin errores (0 errores).
