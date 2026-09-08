# mantenimiento-activo Specification

## Purpose

Registro y consulta (histórico) de mantenimientos por activo mediante un modal. El usuario captura únicamente el comentario; la fecha se asigna automáticamente (`GETDATE()`) y es VISIBLE en el modal en un input deshabilitado (solo lectura). La tabla `Mantenimiento` es multi-tenant (`EmpresaId`) con soft-delete (`Estatus`).

## Requirements

### Requirement: MTA-001 — Registro de mantenimiento

El sistema MUST crear la tabla `Mantenimiento` (patrón `PersonaActivo`) y el SP `GuardarMantenimiento`, que MUST persistir `Comentario`, `Fecha = GETDATE()`, `CreadoPor`, `FechaCreacion`, `EmpresaId` y `Estatus = 1` para el `ActivoId` indicado.

#### Scenario: Registro exitoso

- GIVEN un activo (`Id = 50`) y un usuario que captura el comentario "Cambio de disco SSD"
- WHEN se guarda el mantenimiento
- THEN se inserta una fila con `Comentario = 'Cambio de disco SSD'`, `Fecha` = fecha/hora actual del sistema, `Estatus = 1` y el `EmpresaId` de la sesión

### Requirement: MTA-002 — Fecha visible en input deshabilitado

El modal MUST mostrar el campo `Fecha` en un input deshabilitado (solo lectura) con la fecha actual del sistema, de modo que el usuario vea la fecha que quedará registrada; el usuario MUST NOT capturar la fecha.

#### Scenario: Fecha visible y no editable

- GIVEN el modal de mantenimiento abierto
- WHEN se muestra el campo `Fecha`
- THEN aparece la fecha actual en un input `disabled` (solo lectura) y no se puede modificar

### Requirement: MTA-003 — Historial ordenado por fecha descendente

El SP `ObtenerMantenimientosPorActivo` MUST devolver los mantenimientos del activo con `Estatus = 1` y `Fecha IS NOT NULL`, ordenados por `Fecha DESC`.

#### Scenario: Historial ordenado

- GIVEN un activo con mantenimientos del día 1, día 3 y día 2
- WHEN se consulta su historial
- THEN se listan en orden descendente de fecha (día 3, día 2, día 1)

#### Scenario: Registros sin fecha excluidos

- GIVEN un mantenimiento con `Fecha IS NULL`
- WHEN se consulta el historial
- THEN ese registro no se lista

### Requirement: MTA-004 — Multi-tenant por EmpresaId

La tabla `Mantenimiento` MUST incluir `EmpresaId` y los SPs MUST scopear la consulta/guardado por la empresa de la sesión.

#### Scenario: Aislamiento entre empresas

- GIVEN los activos de las empresas A y B
- WHEN se consultan los mantenimientos del activo de A
- THEN no se devuelven mantenimientos de activos de B

### Requirement: MTA-005 — Soft-delete

`Mantenimiento` MUST usar `Estatus` (default `1`) como soft-delete; los registros con `Estatus = 0` MUST quedar excluidos del historial.

#### Scenario: Mantenimiento eliminado lógicamente

- GIVEN un mantenimiento marcado `Estatus = 0`
- WHEN se consulta el historial del activo
- THEN ese mantenimiento no se muestra

### Requirement: MTA-006 — Modal de captura + histórico

`Active.cshtml` MUST exponer un botón "Mantenimientos" por fila que abra el modal `modalMantenimientoActivo`, con captura del comentario y listado del histórico.

#### Scenario: Apertura del modal

- GIVEN la tabla de Activos
- WHEN se presiona "Mantenimientos" en una fila
- THEN se abre el modal con el campo Fecha (deshabilitado), el campo de comentario y el historial cargado
