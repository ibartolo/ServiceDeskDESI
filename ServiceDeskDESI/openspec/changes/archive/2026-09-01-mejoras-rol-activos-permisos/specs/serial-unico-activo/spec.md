# serial-unico-activo Specification

## Purpose

Unicidad del campo `Serial` de Activo por empresa entre activos vigentes (`Estatus = 1`) con serial no nulo. Los seriales nulos quedan permitidos y no únicos; la eliminación lógica (`Estatus = 0`) libera el serial. Un índice único filtrado y el SP `GuardarOActualizarActivo` (retorno `-2`) garantizan la regla con mensaje amigable.

## Requirements

### Requirement: SUA-001 — Unicidad por empresa entre activos vigentes

El sistema MUST crear el índice único filtrado `UX_Activo_EmpresaSerial` sobre `(EmpresaId, Serial)` con filtro `WHERE Serial IS NOT NULL AND Estatus = 1`, de modo que no existan dos activos vigentes de la misma empresa con el mismo `Serial`.

#### Scenario: Serial duplicado en la misma empresa

- GIVEN la empresa A (`EmpresaId = 1`) con un activo vigente de `Serial = 'SN-001'`
- WHEN se intenta guardar otro activo vigente con `Serial = 'SN-001'` en la misma empresa
- THEN el guardado se rechaza (no hay dos filas vigentes con el mismo par EmpresaId/Serial)

#### Scenario: Mismo serial en otra empresa permitido

- GIVEN la empresa A con activo vigente `Serial = 'SN-001'`
- WHEN la empresa B (`EmpresaId = 2`) guarda un activo con `Serial = 'SN-001'`
- THEN el guardado procede (la unicidad es por empresa)

### Requirement: SUA-002 — Serial nulo permitido

El sistema MUST permitir seriales nulos; un `Serial` nulo MUST NOT quedar sujeto a la regla de unicidad.

#### Scenario: Varios activos sin serial

- GIVEN dos activos vigentes de la misma empresa con `Serial = NULL`
- WHEN se guardan ambos
- THEN el guardado procede sin error de unicidad

### Requirement: SUA-003 — Soft-delete libera el serial

El sistema MUST excluir de la unicidad los activos eliminados lógicamente (`Estatus = 0`), de modo que su `Serial` quede reutilizable.

#### Scenario: Serial reutilizado tras soft-delete

- GIVEN la empresa A con un activo de `Serial = 'SN-001'` eliminado lógicamente (`Estatus = 0`)
- WHEN se guarda un activo nuevo con `Serial = 'SN-001'` en la misma empresa
- THEN el guardado procede (el serial fue liberado)

### Requirement: SUA-004 — Validación en SP con retorno -2

El SP `GuardarOActualizarActivo` MUST validar el duplicado (activos vigentes, misma `EmpresaId`, excluyendo el `Id` actual) y MUST devolver `-2` antes de fallar por el índice.

#### Scenario: Duplicado detectado por el SP

- GIVEN una edición que reutiliza un `Serial` ya existente en otro activo vigente de la misma empresa
- WHEN se ejecuta `GuardarOActualizarActivo`
- THEN devuelve `-2` y no persiste el cambio

### Requirement: SUA-005 — Mensaje amigable en la UI

El sistema MUST mapear el código `-2` al mensaje "Ya existe un activo con ese No. de Serie" y mostrarlo en `Active.cshtml`.

#### Scenario: Mensaje de duplicado

- GIVEN un guardado que devuelve `-2`
- WHEN se procesa la respuesta en `Active.cshtml`
- THEN se muestra "Ya existe un activo con ese No. de Serie" (vía Swal)
