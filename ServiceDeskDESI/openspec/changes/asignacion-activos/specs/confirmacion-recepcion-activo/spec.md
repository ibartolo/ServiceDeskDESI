# confirmacion-recepcion-activo Specification

## Purpose

Confirmar de forma anónima la recepción de un activo mediante un token GUID sin caducidad, marcando `FechaConfirmacion` de forma idempotente; la confirmación es obligatoria para dar por cerrada la asignación.

## Requirements

### Requirement: CRA-001 — Confirmación anónima por token

El sistema MUST exponer un endpoint anónimo que valide el `TokenConfirmacion` (GUID) y, si es válido, fije `FechaConfirmacion`. La confirmación MUST ser idempotente.

#### Scenario: Confirmación exitosa

- GIVEN una asignación pendiente con token válido
- WHEN se invoca el endpoint con ese token
- THEN `FechaConfirmacion` queda establecida

#### Scenario: Confirmación repetida (idempotencia)

- GIVEN una asignación ya confirmada
- WHEN se invoca de nuevo el endpoint con el mismo token
- THEN no hay cambio de estado y se reporta "ya confirmado" sin error

### Requirement: CRA-002 — Confirmación obligatoria para cerrar

El sistema MUST tratar la asignación como pendiente mientras `FechaConfirmacion IS NULL`. La asignación MUST NOT considerarse cerrada hasta la confirmación.

#### Scenario: Asignación pendiente

- GIVEN una asignación recién creada
- WHEN no se ha confirmado
- THEN `FechaConfirmacion IS NULL` y el estado es "pendiente"

#### Scenario: Asignación cerrada

- GIVEN una asignación con `FechaConfirmacion` establecida
- WHEN se consulta
- THEN se considera confirmada/cerrada

### Requirement: CRA-003 — Token sin caducidad

El `TokenConfirmacion` MUST NOT expirar; la confirmación MUST ser válida sin importar el tiempo transcurrido.

#### Scenario: Confirmación tardía

- GIVEN un token generado hace N días
- WHEN el usuario confirma
- THEN la confirmación es aceptada

### Requirement: CRA-004 — Token inválido o desconocido

El sistema MUST devolver error claro y MUST NOT cambiar estado ante un token inválido o desconocido.

#### Scenario: Token desconocido

- GIVEN un GUID que no corresponde a ninguna asignación
- WHEN se invoca el endpoint
- THEN se devuelve error claro y no cambia `FechaConfirmacion`

#### Scenario: Token malformado

- GIVEN un token que no es un GUID válido
- WHEN se invoca el endpoint
- THEN se devuelve error claro sin cambio de estado

### Requirement: CRA-005 — El administrador no puede confirmar

El sistema MUST NOT permitir que el administrador confirme en nombre del usuario; el único flujo de confirmación MUST ser el enlace del correo.

#### Scenario: Sin endpoint administrativo

- GIVEN un administrador autenticado
- WHEN intenta confirmar una asignación manualmente
- THEN no existe endpoint de confirmación para admin; la confirmación solo ocurre vía el enlace anónimo
