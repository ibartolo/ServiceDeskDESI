# campos-activo Specification

## Purpose

Nuevo campo `SerieLocal` (texto libre, no único) en Activos y conversión del campo `Notas` existente a `<textarea>` con `maxlength = 250`. No se crea un campo `Comentarios` (se reutiliza `Notas`, ya `NVARCHAR(250)` en BD).

## Requirements

### Requirement: CAM-001 — Campo SerieLocal capturable y persistido

El sistema MUST agregar `SerieLocal NVARCHAR(100) NULL` a la tabla `Activo`, exponerlo en el formulario `Active.cshtml` y persistirlo vía el SP `GuardarOActualizarActivo` (lectura automática por `a.*`).

#### Scenario: Captura de SerieLocal

- GIVEN el formulario de Activo
- WHEN el usuario captura `SerieLocal = 'LAP-PR-001'` y guarda
- THEN el activo se persiste con `SerieLocal = 'LAP-PR-001'` y se muestra al consultarlo

#### Scenario: SerieLocal vacío

- GIVEN el formulario de Activo sin capturar `SerieLocal`
- WHEN se guarda
- THEN `SerieLocal` queda `NULL` sin error

### Requirement: CAM-002 — SerieLocal no único

El sistema MUST NOT imponer restricción de unicidad sobre `SerieLocal`.

#### Scenario: SerieLocal repetido permitido

- GIVEN dos activos de la misma empresa con `SerieLocal = 'LAP-PR-001'`
- WHEN se guardan ambos
- THEN el guardado procede (no hay error de unicidad)

### Requirement: CAM-003 — Notas como textarea con maxlength 250

El sistema MUST renderizar `Notas` como `<textarea>` (reemplazando `@Html.TextBoxFor`) y MUST aplicar `maxlength = 250` en la validación (`jquery.validate`).

#### Scenario: Notas multilínea

- GIVEN el formulario de Activo
- WHEN el usuario captura una nota de varias líneas (hasta 250 caracteres)
- THEN se guarda completa y se muestra como textarea

#### Scenario: Notas exceden 250

- GIVEN el formulario de Activo
- WHEN el usuario intenta capturar más de 250 caracteres en `Notas`
- THEN la validación impide exceder 250 caracteres

### Requirement: CAM-004 — Sin campo Comentarios

El sistema MUST NOT crear un campo `Comentarios`; las notas MUST reutilizar `Notas` (`NVARCHAR(250)`).

#### Scenario: No existe Comentarios

- GIVEN la migración de este cambio
- WHEN se inspecciona la tabla `Activo`
- THEN no existe columna `Comentarios` y `Notas` sigue siendo `NVARCHAR(250)`
