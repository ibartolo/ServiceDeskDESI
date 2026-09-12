# ticket-estatus-espera Specification

## Purpose

Estatus de espera para tickets cuyo avance depende de factores externos al soporte interno (materiales/insumos o proveedores terceros). El ticket permanece abierto, con motivo obligatorio, fecha estimada opcional y registro en el histórico.

## Requirements

### Requirement: Catálogo de estatus de espera

El sistema MUST agregar al catálogo `TicketEstatus` los estatus **6 "Pendiente de Materiales"** y **7 "En Espera de Terceros"**, activos, con color y orden propios. El estatus 6 MUST representar la dependencia de una compra/insumo o de un mantenimiento de un tercero aún no ejecutado. El estatus 7 MUST representar la dependencia de la respuesta de un proveedor externo cuyo tiempo de espera no controla la empresa.

#### Scenario: Estatus disponibles

- GIVEN el catálogo de estatus
- WHEN se consulta `ObtenerTicketEstatus`
- THEN se incluyen "Pendiente de Materiales" (6) y "En Espera de Terceros" (7)

### Requirement: Pausa del agente por dependencia externa

Un usuario con rol `PuedeAtenderTickets` asignado al ticket MUST poder pausarlo desde "En Progreso" (2) hacia "Pendiente de Materiales" (6) o "En Espera de Terceros" (7). La pausa MUST requerir un comentario (motivo) de 1 a 300 caracteres y MUST permitir una fecha estimada de respuesta opcional.

#### Scenario: Pausa por materiales

- GIVEN un ticket "En Progreso" asignado al agente
- WHEN el agente pausa con motivo "Pendiente de Materiales" y un comentario válido
- THEN el ticket pasa a "Pendiente de Materiales" y el comentario queda en el histórico

#### Scenario: Pausa sin comentario

- GIVEN un ticket "En Progreso" del agente
- WHEN pausa sin comentario o con más de 300 caracteres
- THEN la transición se rechaza

#### Scenario: Pausa por terceros con fecha estimada

- GIVEN un ticket "En Progreso" del agente
- WHEN pausa con motivo "En Espera de Terceros", comentario y fecha estimada
- THEN el ticket pasa a "En Espera de Terceros" y la fecha estimada queda registrada

#### Scenario: Pausa sobre ticket ajeno o no asignado

- GIVEN un ticket "En Progreso" no asignado al usuario
- WHEN intenta pausarlo
- THEN la acción no está disponible y se deniega

### Requirement: Reanudación del agente

El agente asignado MUST poder reanudar un ticket en "Pendiente de Materiales" (6) o "En Espera de Terceros" (7), devolviéndolo a "En Progreso" (2). El comentario de reanudación MUST ser opcional.

#### Scenario: Reanudar ticket pausado

- GIVEN un ticket en "Pendiente de Materiales" o "En Espera de Terceros" asignado al agente
- WHEN el agente ejecuta "Reanudar"
- THEN el ticket pasa a "En Progreso"

### Requirement: Bloqueo de transiciones directas desde estatus de espera

Desde "Pendiente de Materiales" (6) o "En Espera de Terceros" (7) el sistema MUST NOT permitir Resolver, Rechazar, Cerrar ni Reasignar. La única salida MUST ser Reanudar hacia "En Progreso".

#### Scenario: Resolver bloqueado

- GIVEN un ticket en "Pendiente de Materiales"
- WHEN el agente intenta "Resolver"
- THEN la acción no está disponible y se deniega

#### Scenario: Cerrar/Rechazar bloqueados

- GIVEN un ticket en "En Espera de Terceros" cuyo creador es el usuario actual
- WHEN el solicitante intenta "Cerrar" o "Rechazar"
- THEN las acciones no están disponibles y se deniegan

### Requirement: Registro en el histórico

Toda pausa y reanudación MUST quedar registrada en `TicketAsignacion` con su `TipoMovimiento` (`PendienteMateriales`, `EnEsperaTerceros`, `Reanudar`), el estatus resultante y, en las pausas, la `FechaEstimada`. Solo la última fila MUST quedar `EsActiva = true`.

#### Scenario: Movimiento de pausa registrado

- GIVEN una pausa ejecutada
- WHEN se consulta el histórico
- THEN existe una fila con el `TipoMovimiento`, el estatus resultante y la fecha estimada (si se capturó)

### Requirement: Visibilidad de la fecha estimada

La UI MUST mostrar la fecha estimada de respuesta en el detalle/histórico del ticket y, cuando el ticket esté en estatus 6 o 7, en el listado.

#### Scenario: Fecha estimada visible

- GIVEN un ticket en estatus 6 o 7 con fecha estimada
- WHEN se muestra el listado o el detalle
- THEN se muestra la fecha estimada

### Requirement: Indicadores del dashboard

El indicador `ActivosSemana` MUST contar también los tickets en estatus 6 y 7. El indicador `Trabajando` MUST seguir contando únicamente los tickets en "En Progreso" (2).

#### Scenario: Pausados cuentan como activos

- GIVEN un ticket creado esta semana en "Pendiente de Materiales"
- WHEN se calculan los indicadores del dashboard
- THEN suma en `ActivosSemana` y no en `Trabajando`
