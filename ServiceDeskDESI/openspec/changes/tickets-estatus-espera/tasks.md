# Tasks: Estatus de espera por dependencia externa

Orden: BD → Entities → WebApi → MVC → Frontend → Build/verificación.

## Batch 1: BD / migración (`migration.sql`)

- [x] 1.1 Insertar estatus 6 "Pendiente de Materiales" y 7 "En Espera de Terceros" en `TicketEstatus` (idempotente, `SET IDENTITY_INSERT`, color + orden). **Done when**: re-ejecución no falla y `ObtenerTicketEstatus` los devuelve.
- [x] 1.2 `ALTER TABLE TicketAsignacion ADD FechaEstimada DATE NULL` (idempotente vía `sys.columns`). **Done when**: columna existe y re-ejecución no falla.
- [x] 1.3 `DROP`/`CREATE TransicionarTicket` con `@FechaEstimada DATE = NULL` + movimientos `PendienteMateriales` (2→6), `EnEsperaTerceros` (2→7), `Reanudar` (6/7→2); preservar validaciones existentes. **Done when**: fallo→0, éxito→`SCOPE_IDENTITY()`.
- [x] 1.4 Modificar `ObtenerTickets` y `ObtenerTicketsPorArea`: añadir `ta.FechaEstimada` preservando el resto del cuerpo. **Done when**: la lista expone `FechaEstimada`.
- [x] 1.5 Modificar `ObtenerTicketAsignaciones`: añadir `ta.FechaEstimada`. **Done when**: el histórico expone `FechaEstimada`.
- [x] 1.6 Modificar `ObtenerIndicadoresDashboard`: `ActivosSemana` → `TicketEstatusId IN (1,2,6,7)`; `Trabajando` sin cambio. **Done when**: los pausados suman en activos.

## Batch 2: Entities / DTO

- [x] 2.1 `Tickets/TicketAsignacion.cs`: +`public DateTime? FechaEstimada`. **Done when**: mapea por nombre (DTO hereda).
- [x] 2.2 `Tickets/TicketDTO.cs`: +`public DateTime? FechaEstimada`. **Done when**: presente.

## Batch 3: WebApi

- [x] 3.1 `ServiceDeskDESIWebApi/DAL/DbWrapper.Ticket.cs`: `PausarTicket` y `ReanudarTicket` (`ExecuteScalar("TransicionarTicket"...)`). **Done when**: llaman al SP con el `TipoMovimiento` correcto.
- [x] 3.2 `ServiceDeskDESIWebApi/Services/TicketService.cs`: `PausarTicket` (validar tipo + comentario `1..300`) y `ReanudarTicket`. **Done when**: validación server-side.
- [x] 3.3 `ServiceDeskDESIWebApi/Controllers/TicketController.cs`: endpoints `Pausar`/`Reanudar` `[Permiso("Tickets","Editar")]` + clase `PausarTicketRequest` (TicketId, TipoPausa, Comentario, FechaEstimada) y mapeo TipoPausa→TipoMovimiento. **Done when**: rutas + permisos según design.
- [x] 3.4 Mover las clases request (`TomarTicketRequest`, `ReasignarTicketRequest`, `TransicionTicketRequest`, `PausarTicketRequest`) fuera del controller a `ServiceDeskDESIWebApi/Models/` y registrarlas en `ServiceDeskDESIWebApi.csproj`. **Done when**: el controller solo contiene la clase `TicketController`.

## Batch 4: MVC backend

- [x] 4.1 `ServiceDeskDESIMVC/DAL/HttpClientConnection.Ticket.cs`: `PausarTicket`/`ReanudarTicket` (POST espejo). **Done when**: llaman `api/Ticket/Pausar` y `api/Ticket/Reanudar`.
- [x] 4.2 `ServiceDeskDESIMVC/Services/TicketService.cs`: espejo de 4.1. **Done when**: métodos disponibles.
- [x] 4.3 `ServiceDeskDESIMVC/Controllers/TicketController.cs`: acciones `PausarTicket(ticketId, tipoPausa, comentario, fechaEstimada)` y `ReanudarTicket(ticketId, comentario)`. **Done when**: devuelven JSON `ModelResponse`.

## Batch 5: Frontend

- [x] 5.1 Crear `ServiceDeskDESIMVC/Views/Ticket/_PausarTicket.cshtml`: modal con motivo, comentario obligatorio (`maxlength=300`) y fecha estimada opcional; submit `PostMVC('/Ticket/PausarTicket', {...})`. **Done when**: pausa con motivo.
- [x] 5.2 `ServiceDeskDESIMVC/Views/Ticket/Index.cshtml`: incluir parcial; botón "Pausar" (estatus 2, agente dueño) y "Reanudar" (estatus 6/7, agente dueño); render de `FechaEstimada` en la columna Estatus para 6/7. **Done when**: botones visibles por rol/estatus.
- [x] 5.3 `ServiceDeskDESIMVC/Views/Ticket/_DetalleTicket.cshtml`: columna "Fecha estimada" en `tblHistorial`. **Done when**: el histórico muestra la fecha.
- [x] 5.4 Guardar `_PausarTicket.cshtml` como UTF-8 **con BOM** (igual que los demás `.cshtml`) para que Razor muestre los acentos correctamente. **Done when**: el archivo tiene BOM y los acentos se renderizan bien.
- [x] 5.5 Actualizar el manual de ayuda `Views/Home/Ayuda.cshtml`: ciclo de vida con los estatus 6/7, sección "Pausar y reanudar un ticket", tabla de roles, FAQ y glosario, y `data-tags` para el buscador. **Done when**: el manual describe el pausado por materiales/terceros.

## Batch 6: Build + verificación

- [x] 6.1 Compilar `ServiceDeskDESI.sln` (MSBuild VS2022, Debug). **Done when**: 0 errores.
- [x] 6.2 Verificación estática de reglas: pausa (2→6/7) con comentario, reanudación (6/7→2), bloqueo de Resolver/Rechazar/Cerrar/Reasignar desde 6/7, dashboard incluye 6/7. **Done when**: escenarios de spec cubiertos.
