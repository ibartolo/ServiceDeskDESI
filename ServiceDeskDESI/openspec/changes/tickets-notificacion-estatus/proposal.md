# Proposal: Notificación por correo en cada cambio de estatus de ticket

- **Change**: `tickets-notificacion-estatus`
- **Fase**: propose
- **Fecha**: 2026-09-11
- **Origen**: petición del negocio. Ya existe la plantilla `Template_CambioEstatusTicket.html`, pero **no se usa** en el código (hoy no se envía ningún correo al cambiar de estatus).

## Intent

Garantizar que **cada vez que un ticket cambia de estatus** —cualquiera que sea— se notifique por correo a **la persona que lo creó**, indicando el **nuevo estatus con toda su información** (folio, título, categoría, prioridad, fecha de creación, descripción, color del estatus, mensaje y nota), reutilizando la plantilla existente `Template_CambioEstatusTicket.html`.

## Scope

### In Scope

- Enviar correo al **creador** del ticket en las 9 transiciones: `Tomar`, `Resolver`, `Rechazar`, `Cerrar`, `Retomar`, `Reasignar`, `PendienteMateriales`, `EnEsperaTerceros`, `Reanudar`.
- Rellenar todos los placeholders del template (`{{NombreUsuario}}`, `{{MensajeEstatus}}`, `{{ColorEstatus}}`, `{{NumeroTicket}}`, `{{TituloTicket}}`, `{{Categoria}}`, `{{ColorPrioridad}}`, `{{Prioridad}}`, `{{FechaCreacion}}`, `{{Estatus}}`, `{{DescripcionTicket}}`, `{{NotaAdicional}}`, `{{UrlTicket}}`).
- El mensaje y la nota varían según el movimiento; en pausas se incluye la fecha estimada (si se capturó) y el motivo.
- **Best-effort**: un fallo al enviar el correo MUST NOT romper ni revertir la transición (se registra en log).

### Out of Scope

- Notificar a otros destinatarios (agente asignado, responsable de área, administrador).
- Preferencias de notificación por usuario (opt-in/opt-out).
- Editar la plantilla desde la UI.
- Reintentos/cola de correos.

## Capabilities

### New Capabilities

- `ticket-notificacion-estatus`: notificación por correo al creador en cada cambio de estatus, con la información completa del ticket y el nuevo estatus.

### Modified Capabilities

- None.

## Approach

- **Sin cambios de BD**: se reutiliza `ObtenerTicketPorId` (folio, título, categoría, urgencia, fecha, descripción, estatus, creador) y `ObtenerUsuarioPorNombreUsuario` (nombre + `Correo` del creador).
- **WebApi**: método privado `NotificarCambioEstatus(ticketId, usuario, tipoMovimiento, comentario, fechaEstimada)` en `TicketService`, invocado tras **cada transición exitosa** de los 8 métodos (`TomarTicket`, `ReasignarTicket`, `ResolverTicket`, `RechazarTicket`, `CerrarTicket`, `RetomarTicket`, `PausarTicket`, `ReanudarTicket`).
- Se lee `~/Template/Template_CambioEstatusTicket.html`, se reemplazan los placeholders y se envía con `EmailHelper.EnvioEmaiil` al correo del creador.
- `{{UrlTicket}}` = `AppSettings["BaseUri"]` + `Ticket/Index`.

## Affected Areas

| Área | Impacto | Descripción |
|------|---------|-------------|
| `ServiceDeskDESIWebApi/Services/TicketService.cs` | Mod | `+using Helpers`; `NotificarCambioEstatus` + helpers de prioridad/mensaje; llamada en cada transición |
| `ServiceDeskDESIWebApi/Template/Template_CambioEstatusTicket.html` | Reutiliza | Plantilla ya existente (sin cambios) |

## Risks

| Riesgo | Prob. | Mitigación |
|--------|-------|------------|
| SMTP caído/correo inválido rompe la transición | Media | `try/catch` dentro de `NotificarCambioEstatus` (best-effort) + log |
| Creador sin correo | Baja | Se omite el envío y se registra warning |
| Duplicidad de correos | Baja | Se notifica una vez por transición exitosa |

## Rollback Plan

- Revertir `TicketService.cs` (quitar las llamadas y el método). No hay cambios de BD.

## Dependencies

- Config SMTP existente (`smtpClient`, `port`, `userEmail`, `passEmail`, `BaseUri`) en `ServiceDeskDESIWebApi/Web.config`.

## Success Criteria

- [ ] Cada cambio de estatus (los 9 movimientos) envía un correo al creador.
- [ ] El correo muestra el nuevo estatus con su color y toda la información del ticket.
- [ ] Un fallo de correo no altera el resultado de la transición.
- [ ] `ServiceDeskDESI.sln` compila sin errores.
