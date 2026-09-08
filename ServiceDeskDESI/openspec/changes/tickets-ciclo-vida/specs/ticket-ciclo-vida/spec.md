# ticket-ciclo-vida Specification

## Purpose

Transiciones de estatus del ticket basadas en rol, con comentario (máx. 300) registrado en un historial unificado sobre `TicketAsignacion`.

## Requirements

### Requirement: Catálogo de estatus

El sistema MUST usar los estatus: 1 Nuevo, 2 En Progreso, 3 Resuelto, 4 Rechazado, 5 Cerrado. El estatus 4 MUST mostrarse como "Rechazado" (renombrado desde "Reabierto").

#### Scenario: Renombre de estatus 4

- GIVEN un ticket con `TicketEstatusId` = 4
- WHEN se muestra su estatus en la UI
- THEN se muestra "Rechazado"

### Requirement: Transiciones del agente

Un usuario con rol `PuedeAtenderTickets` MUST poder: Tomar (Nuevo sin asignación activa → En Progreso); Resolver (En Progreso → Resuelto) con comentario obligatorio (máx. 300) registrado; Retomar (Rechazado → En Progreso).

#### Scenario: Tomar ticket nuevo

- GIVEN un ticket Nuevo sin agente asignado
- WHEN un agente ejecuta "Tomar"
- THEN el ticket pasa a En Progreso con el agente asignado

#### Scenario: Resolver con comentario

- GIVEN un ticket En Progreso del agente
- WHEN ejecuta "Resolver" con comentario válido
- THEN pasa a Resuelto y el comentario queda en el historial

#### Scenario: Resolver con comentario inválido

- GIVEN un ticket En Progreso
- WHEN el agente ejecuta "Resolver" sin comentario o con más de 300 caracteres
- THEN la transición se rechaza

#### Scenario: Retomar ticket rechazado

- GIVEN un ticket Rechazado
- WHEN un agente ejecuta "Retomar"
- THEN pasa a En Progreso

### Requirement: Transiciones del solicitante

El creador (`CreadoPor` = usuario actual) MUST poder, solo sobre sus propios tickets: Cerrar (Resuelto → Cerrado); Rechazar (Resuelto → Rechazado) con comentario obligatorio (máx. 300) registrado.

#### Scenario: Cerrar ticket resuelto

- GIVEN un ticket Resuelto cuyo creador es el usuario actual
- WHEN ejecuta "Cerrar"
- THEN pasa a Cerrado

#### Scenario: Rechazar ticket resuelto

- GIVEN un ticket Resuelto del solicitante
- WHEN ejecuta "Rechazar" con comentario válido
- THEN pasa a Rechazado y el comentario queda registrado

#### Scenario: Acción sobre ticket ajeno

- GIVEN un ticket cuyo `CreadoPor` no es el usuario actual
- WHEN el solicitante intenta Cerrar o Rechazar
- THEN la acción no está disponible y se deniega

### Requirement: Reasignación por responsable de área

El responsable del área (`Area.UsuarioResponsableId` = usuario actual) MUST poder reasignar el ticket mediante el modal `_ReasignarTicket.cshtml`, que lista los usuarios del área con rol `PuedeAtenderTickets`. La reasignación MUST fijar En Progreso, MUST crear una nueva asignación y MUST cerrar la asignación activa. El comentario es opcional.

#### Scenario: Reasignar ticket

- GIVEN el responsable del área y un ticket
- WHEN elige un usuario del área y confirma
- THEN el ticket pasa a En Progreso con nueva asignación activa y la anterior queda cerrada

### Requirement: Historial unificado

TODO cambio de estatus o asignación MUST quedar registrado en `TicketAsignacion` con `TipoMovimiento` (Tomar/Reasignar/Resolver/Rechazar/Cerrar/Retomar) y estatus resultante. Solo la última fila MUST quedar `EsActiva = true`.

#### Scenario: Movimiento registrado

- GIVEN una transición de estatus ejecutada
- WHEN se consulta el historial
- THEN existe una fila en `TicketAsignacion` con el `TipoMovimiento` y el estatus resultante correspondientes

### Requirement: Bloqueo de Tomar con asignación activa

Un ticket con asignación activa MUST NOT poder ser tomado por otro agente. La acción "Tomar" MUST ocultarse o bloquearse.

#### Scenario: Ticket ya tomado

- GIVEN un ticket con una asignación activa
- WHEN otro agente revisa el ticket
- THEN "Tomar" no está disponible
