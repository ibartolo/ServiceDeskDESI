# Tasks: Ciclo de vida de tickets

Orden: BD → Entities → WebApi → MVC → Frontend → Build/verificación.

## Batch 1: BD / migración (`migration.sql`)

- [x] 1.1 Crear `openspec/changes/tickets-ciclo-vida/migration.sql`: `UPDATE TicketEstatus SET Nombre='Rechazado' WHERE Id=4`; `ALTER TicketAsignacion ADD TipoMovimiento NVARCHAR(20) NULL, TicketEstatusId INT NULL` (idempotente vía `IF NOT EXISTS sys.columns`); backfill activas `TipoMovimiento='Tomar'`, `TicketEstatusId=t.TicketEstatusId`. **Done when**: re-ejecución no falla.
- [x] 1.2 `CREATE OR ALTER TransicionarTicket`: validaciones por movimiento (rol/ownership/estatus), cierra asignación activa previa, INSERT historial, UPDATE estatus. **Done when**: fallo→0, éxito→`SCOPE_IDENTITY()`; Reasignar respeta `@EstatusActual IN (2,4)`.
- [x] 1.3 `CREATE OR ALTER ObtenerUsuariosArea`: agentes del área con rol `PuedeAtenderTickets`, misma empresa. **Done when**: devuelve usuarios del área.
- [x] 1.4 Modificar `ObtenerTickets` + `ObtenerTicketsPorArea`: añadir `u.Id as CreadoPorId` preservando LEFT JOIN de agente. **Done when**: creador presente.
- [x] 1.5 Modificar `ObtenerTicketAsignaciones`: añadir `ta.TipoMovimiento`, `ta.TicketEstatusId`, `te.Nombre as EstatusNombre`, `te.Color as EstatusColor` (JOIN TicketEstatus). **Done when**: histórico con tipo + estatus.
- [x] 1.6 (baja prioridad) Endurecer `Retomar` en `TransicionarTicket`: añadir chequeo de área (`Usuario.AreaId = Ticket.AreaId`). **Done when**: defensa en profundidad añadida.

## Batch 2: Entities / DTO

- [x] 2.1 `Tickets/TicketAsignacion.cs`: +`public string TipoMovimiento` y `public int? TicketEstatusId`. **Done when**: mapea por nombre.
- [x] 2.2 `Tickets/TicketAsignacionDTO.cs`: +`EstatusNombre` y `EstatusColor`. **Done when**: presentes.
- [x] 2.3 `Tickets/TicketDTO.cs`: +`public long? CreadoPorId`. **Done when**: presente.

## Batch 3: WebApi

- [x] 3.1 `ServiceDeskDESIWebApi/DAL/DbWrapper.Ticket.cs`: Tomar/Reasignar → `ExecuteScalar("TransicionarTicket"...)`; nuevos `ResolverTicket`, `RechazarTicket`, `CerrarTicket`, `RetomarTicket` (ExecuteScalar) y `ObtenerUsuariosArea` (GetObjects→`List<UsuarioDTO>`). **Done when**: llaman `TransicionarTicket` con `TipoMovimiento` correcto.
- [x] 3.2 `ServiceDeskDESIWebApi/Services/TicketService.cs`: métodos nuevos; validar comentario requerido + ≤300 en Resolver/Rechazar (`IsSuccess=false` + mensaje). **Done when**: validación server.
- [x] 3.3 `ServiceDeskDESIWebApi/Controllers/TicketController.cs`: endpoints `Resolver`/`Retomar` `[Permiso("Tickets","Editar")]`, `Cerrar`/`Rechazar` `[Permiso("Tickets","Leer")]`, `UsuariosArea/{areaId}`; clase `TransicionTicketRequest`. **Done when**: rutas+permisos según design.

## Batch 4: MVC backend

- [x] 4.1 `ServiceDeskDESIMVC/DAL/HttpClientConnection.Ticket.cs` + `Services/TicketService.cs`: espejo Resolver/Rechazar/Cerrar/Retomar (POST `{ticketId,comentario}`) + `ObtenerUsuariosArea` (GET), patrón `RequestAsync` de Tomar/Reasignar. **Done when**: espejo completo.
- [x] 4.2 `ServiceDeskDESIMVC/Controllers/TicketController.cs`: `Index` obtiene `AreaId` vía `_usuarioService.ObtenerUsuarioPorId(tokenCookie.UserID)` + `_areaService.ObtenerAreaPorId` → ViewBag `UsuarioActualId`/`UsuarioActualNombre`/`EsResponsableArea`; acciones Resolver/Cerrar/Rechazar/Retomar/ObtenerUsuariosArea; eliminar `CambiarEstatusTicket`. **Done when**: ViewBag + acciones JSON.

## Batch 5: Frontend

- [x] 5.1 Reescribir `ServiceDeskDESIMVC/Views/Ticket/Index.cshtml`: quitar `frmTicket` inline (25–108) y edición; conservar filtro de área + DataTable; botón "Nuevo Ticket" + modales; globals `esAgenteGlobal`/`usuarioActualIdGlobal`/`esResponsableAreaGlobal`; `createdRow` resalta `.ticket-mio` si `data.AgenteId===usuarioActualIdGlobal`. **Done when**: solo listado, sin formulario inline.
- [x] 5.2 Matriz de botones JS: Tomar `esAgenteGlobal && estatus===1 && !AgenteId`; Ver siempre; Resolver `esAgenteGlobal && AgenteId===usuarioActualIdGlobal && estatus===2`; Retomar `esAgenteGlobal && estatus===4`; Cerrar/Rechazar `CreadoPorId===usuarioActualIdGlobal && estatus===3`; Reasignar `esResponsableAreaGlobal && (estatus===2||estatus===4)`. SweetAlert2 textarea obligatorio ≤300 en Resolver/Rechazar. **Done when**: visibilidad por rol+estatus.
- [x] 5.3 Crear `_CapturarTicket.cshtml`: cascada Área→Categoría→Subcategoría (reusa `ObtenerCategoriasPorArea`/`ObtenerSubcategoriasPorCategoria`), Urgencia, Título (max 250), Descripción; sin estatus/Id; submit `PostMVC('/Ticket/GuardarOActualizarTicket',{...,TicketEstatusId:1})`; éxito → cerrar+reset+refresh. **Done when**: solo alta.
- [x] 5.4 Crear `_ReasignarTicket.cshtml`: `ddlUsuarioReasignar` vía `ObtenerUsuariosArea(row.AreaId)`, comentario opcional maxlength 300, hidden ticketId → `PostMVC('/Ticket/ReasignarTicket',{ticketId,nuevoUsuarioId,comentario})`. **Done when**: reasigna.
- [x] 5.5 Crear `_DetalleTicket.cshtml`: detalle solo lectura desde `row` + historial vía `ObtenerTicketAsignaciones` (Fecha/TipoMovimiento/Agente/Comentario/Estatus). **Done when**: "Ver" muestra detalle+historial.
- [x] 5.6 CSS `.ticket-mio { background-color:#e8f4ff !important; }` en `TemplatePage.css`. **Done when**: resaltado visible.

## Batch 6: Build + verificación manual

- [x] 6.1 Compilar `ServiceDeskDESI.sln` (MSBuild VS2022, Debug). **Done when**: 0 errores.
- [ ] 6.2 Verificación manual por rol: agente (Tomar/Resolver/Retomar), solicitante (Cerrar/Rechazar con comentario), responsable (Reasignar), Ver+historial, estatus 4 = "Rechazado", resaltado "mío". **Done when**: escenarios de specs pasan.
