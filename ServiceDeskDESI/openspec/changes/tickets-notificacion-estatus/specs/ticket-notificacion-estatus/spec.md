# ticket-notificacion-estatus Specification

## Purpose

Notificar por correo al **creador** del ticket cada vez que el ticket cambia de estatus, con la información completa del ticket y el nuevo estatus, usando la plantilla `Template_CambioEstatusTicket.html`.

## Requirements

### Requirement: Notificación en todo cambio de estatus

El sistema MUST enviar un correo al **creador** del ticket (`Ticket.CreadoPor`) cada vez que un ticket cambia de estatus, para **cualquiera** de los movimientos: `Tomar`, `Resolver`, `Rechazar`, `Cerrar`, `Retomar`, `Reasignar`, `PendienteMateriales`, `EnEsperaTerceros` y `Reanudar`.

#### Scenario: Cambio de estatus notifica al creador

- GIVEN un ticket cuyo creador es el usuario A
- WHEN se ejecuta cualquier transición de estatus sobre el ticket
- THEN se envía un correo a la dirección de A

#### Scenario: Todos los movimientos notifican

- GIVEN un ticket
- WHEN se ejecuta cualquiera de los 9 movimientos
- THEN se dispara una notificación (no solo en algunos movimientos)

### Requirement: Contenido del correo

El correo MUST reutilizar la plantilla `Template_CambioEstatusTicket.html` y rellenar: nombre del creador, mensaje del estatus, color del estatus, número/folio, título, categoría, prioridad (texto y color), fecha de creación, estatus actual, descripción y nota adicional (motivo/comentario), además del enlace al ticket.

#### Scenario: Correo con el nuevo estatus

- GIVEN un ticket con folio, título, categoría, urgencia y descripción
- WHEN cambia de estatus
- THEN el correo muestra el **nuevo estatus** con su color y todos los datos del ticket

#### Scenario: Pausa con fecha estimada

- GIVEN un ticket que pasa a "Pendiente de Materiales" con comentario y fecha estimada
- WHEN se notifica
- THEN el correo incluye el motivo y la fecha estimada de respuesta

### Requirement: Notificación best-effort

Un fallo al enviar el correo (SMTP caído, correo inválido, etc.) MUST NOT alterar el resultado de la transición ni revertirla. El error MUST registrarse en el log.

#### Scenario: SMTP falla

- GIVEN un cambio de estatus válido
- WHEN el envío del correo falla
- THEN la transición se completa igualmente y el error queda en el log

#### Scenario: Creador sin correo

- GIVEN un ticket cuyo creador no tiene correo registrado
- WHEN cambia de estatus
- THEN la transición se completa y no se envía correo (se registra un aviso)
