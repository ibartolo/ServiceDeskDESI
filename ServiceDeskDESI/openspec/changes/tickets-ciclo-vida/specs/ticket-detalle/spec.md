# ticket-detalle Specification

## Purpose

Vista de detalle de solo lectura ("Ver") con el historial completo, resaltado visual "míos vs otros" en la tabla y ocultación de las acciones de edición/eliminación.

## Requirements

### Requirement: Vista de detalle de solo lectura

El sistema MUST ofrecer una acción "Ver" que muestre el detalle del ticket y su historial completo de asignaciones en modo solo lectura.

#### Scenario: Ver detalle e historial

- GIVEN un ticket con historial de movimientos
- WHEN el usuario ejecuta "Ver"
- THEN se muestra el detalle del ticket y el historial completo de asignaciones

### Requirement: Resaltado visual míos vs otros

En la tabla de tickets, un ticket con `AgenteId` igual al usuario actual MUST mostrarse visualmente distinto ("míos") de los demás. El badge de color del estatus MUST conservarse.

#### Scenario: Ticket mío

- GIVEN un ticket cuyo `AgenteId` es el usuario actual
- WHEN se renderiza la tabla
- THEN el ticket se resalta como "mío" y conserva su badge de estatus

#### Scenario: Ticket de otro agente

- GIVEN un ticket con `AgenteId` distinto del usuario actual
- WHEN se renderiza la tabla
- THEN el ticket se muestra sin el resaltado "mío"

### Requirement: Ocultación de Editar y Eliminar

La acción "Editar" MUST eliminarse. La acción "Eliminar" MUST ocultarse solo en la UI, sin eliminar la funcionalidad backend.

#### Scenario: Acciones disponibles

- GIVEN un ticket listado
- WHEN se muestran las acciones
- THEN "Editar" no existe y "Eliminar" no se muestra en la UI
