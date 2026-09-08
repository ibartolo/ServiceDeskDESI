# foliador-tickets Specification

## Purpose

Folio único, secuencial y por empresa (`T-00001`) por ticket, generado atómicamente al guardar y persistido en `Ticket.Folio`; el consecutivo vive en `Foliador` (`Nombre='Ticket'`).

## Requirements

### Requirement: FOL-001 — Tabla Foliador y seed por empresa

El sistema MUST crear `Foliador` (`EmpresaId`, `FechaActualizacion`, `Nombre`, `Descripcion`, `Consecutivo`) con seed `Nombre='Ticket'` por empresa; `Consecutivo` MUST iniciar en `0`. Operaciones MUST scopearse por `EmpresaId`.

#### Scenario: Seed por empresa

- GIVEN una empresa sin fila en `Foliador`
- WHEN se aplica el seed
- THEN existe una fila `Nombre='Ticket'` con `Consecutivo = 0`

#### Scenario: Aislamiento entre empresas

- GIVEN las empresas A y B con su fila `Foliador`
- WHEN se incrementa el foliador de A
- THEN el `Consecutivo` de B no cambia

### Requirement: FOL-002 — Consultar foliador por nombre

El sistema MUST exponer `ConsultarFoliador`, que dado `Nombre` devuelve el `Consecutivo` actual de la empresa.

#### Scenario: Consulta exitosa

- GIVEN una fila de la empresa A con `Consecutivo = 5`
- WHEN se consulta por `Nombre='Ticket'`
- THEN se devuelve `Consecutivo = 5`

#### Scenario: Consulta sin fila

- GIVEN una empresa sin fila para el nombre consultado
- WHEN se consulta por ese `Nombre`
- THEN se devuelve un resultado vacío sin error

### Requirement: FOL-003 — Incremento atómico del consecutivo (interno)

El sistema MUST proveer `ActualizarFoliador`, que incrementa `Consecutivo` en `1` atómicamente (SP con `UPDATE ... SET Consecutivo = Consecutivo + 1` bajo `UPDLOCK`) scopeado por `EmpresaId` y devuelve el nuevo valor. Es interno al servicio.

#### Scenario: Incremento secuencial

- GIVEN una fila con `Consecutivo = 0`
- WHEN se ejecuta `ActualizarFoliador`
- THEN `Consecutivo` queda en `1` y devuelve `1`

#### Scenario: Incrementos concurrentes

- GIVEN dos solicitudes concurrentes sobre el mismo foliador
- WHEN ambas ejecutan `ActualizarFoliador`
- THEN cada una recibe un valor distinto (sin duplicados)

### Requirement: FOL-004 — Generación de folio en el guardado

Al guardar, el sistema MUST, dentro de la transacción existente, incrementar el foliador, construir `T-{Consecutivo:00000}` (5 dígitos, p.ej. `T-01000`) y persistirlo en `Ticket.Folio`. Incremento e inserción MUST ser atómicos.

#### Scenario: Primer folio

- GIVEN una empresa con `Consecutivo = 0`
- WHEN se guarda su primer ticket
- THEN persiste con `Folio = 'T-00001'` y `Consecutivo` queda en `1`

#### Scenario: Rollback por fallo

- GIVEN un incremento ya ejecutado
- WHEN la inserción del ticket falla y se revierte
- THEN el incremento se revierte (sin hueco en el consecutivo)

### Requirement: FOL-005 — Vista previa del folio en la captura

Al abrir la captura, el sistema MUST devolver el folio `current+1` formateado para mostrarlo en un campo deshabilitado de solo lectura. El folio almacenado es autoritativo.

#### Scenario: Vista previa

- GIVEN un foliador con `Consecutivo = 4`
- WHEN se abre la captura de un ticket nuevo
- THEN la UI muestra `T-00005` en un campo deshabilitado

#### Scenario: Vista previa desactualizada

- GIVEN la vista previa muestra `T-00005`
- WHEN otro usuario guarda antes y luego el usuario actual guarda
- THEN el folio persistido es el asignado al guardar

### Requirement: FOL-006 — Folio nullable y sin backfill

El sistema MUST agregar `Ticket.Folio NVARCHAR(50)` nullable. Históricos MUST quedar con `Folio = NULL` (sin backfill); solo los nuevos generan folio.

#### Scenario: Históricos sin folio

- GIVEN tickets creados antes de este cambio
- WHEN se aplica la migración
- THEN su `Folio` permanece `NULL`

#### Scenario: Lectura de folio nulo

- GIVEN un ticket con `Folio = NULL`
- WHEN se muestra su detalle
- THEN no se muestra folio (vacío) sin error

### Requirement: FOL-007 — Exposición limitada de la API

El sistema MUST exponer por HTTP únicamente `ConsultarFoliador` (por `Nombre`) mediante `FoliadorController`. `ActualizarFoliador` MUST NOT exponerse por HTTP.

#### Scenario: Consulta por HTTP

- GIVEN un cliente autenticado
- WHEN invoca `GET Foliador` con `Nombre='Ticket'`
- THEN recibe el `Consecutivo` actual de su empresa

#### Scenario: Incremento no expuesto

- GIVEN un cliente autenticado
- WHEN intenta incrementar el foliador por HTTP
- THEN no existe endpoint y la solicitud es rechazada (404)
