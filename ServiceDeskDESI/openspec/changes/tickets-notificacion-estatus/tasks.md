# Tasks: Notificación por correo en cada cambio de estatus de ticket

## Batch 1: WebApi (`ServiceDeskDESIWebApi/Services/TicketService.cs`)

- [x] 1.1 Agregar `using ServiceDeskDESIWebApi.Helpers;` (para `EmailHelper`). **Done when**: compila.
- [x] 1.2 Implementar `NotificarCambioEstatus(ticketId, usuario, tipoMovimiento, comentario, fechaEstimada)` (best-effort, `try/catch`, log) que carga el ticket + creador, rellena `Template_CambioEstatusTicket.html` y envía el correo. **Done when**: envía con todos los placeholders.
- [x] 1.3 Implementar helpers `ObtenerMensajeYNota`, `ObtenerPrioridadTexto`, `ObtenerPrioridadColor`. **Done when**: mensaje/nota por movimiento + prioridad.
- [x] 1.4 Invocar `NotificarCambioEstatus` tras transición exitosa en `TomarTicket`, `ReasignarTicket`, `ResolverTicket`, `RechazarTicket`, `CerrarTicket`, `RetomarTicket` (sin comentario), `PausarTicket` (tipoMovimiento + fechaEstimada) y `ReanudarTicket`. **Done when**: los 9 movimientos notifican.

## Batch 2: Build + verificación

- [x] 2.1 Compilar `ServiceDeskDESI.sln` (MSBuild VS2022, Debug). **Done when**: 0 errores.
- [x] 2.2 Verificación estática: las 8 llamadas existen y el método no propaga excepciones. **Done when**: escenarios de spec cubiertos.
