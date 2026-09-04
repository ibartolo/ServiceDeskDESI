# mis-activos Specification

## Purpose

Vista y endpoint "Mis Activos" que muestran al usuario básico sus activos vigentes y por aceptar, derivando `PersonaId` de forma autenticada sin exponer el catálogo de Personas.

## Requirements

### Requirement: MA-001 — Menú configurable vía RolPaginaAccion

El sistema MUST mostrar la página "Mis Activos" como menú único, visible según `RolPaginaAccion` (configurable por admin), sin depender de `UsuarioPagina`.

#### Scenario: Rol con permiso

- GIVEN un rol con `RolPaginaAccion` de "Mis Activos" con lectura
- WHEN un usuario con ese rol inicia sesión
- THEN ve el menú "Mis Activos" en el sidebar

#### Scenario: Rol sin permiso

- GIVEN un rol sin permiso sobre "Mis Activos"
- WHEN un usuario con ese rol inicia sesión
- THEN no ve el menú

### Requirement: MA-002 — Endpoint autenticado GET MisActivos

El sistema MUST exponer un endpoint autenticado `GET MisActivos` que derive el `PersonaId` desde `Usuarios.PersonaId` del usuario logueado, y MUST NOT exigir `[Permiso("Personas")]`.

#### Scenario: Usuario básico

- GIVEN un usuario autenticado con `Usuarios.PersonaId` no nulo
- WHEN invoca `GET MisActivos`
- THEN devuelve los activos de su `PersonaId` sin requerir permiso "Personas"

#### Scenario: Usuario sin persona vinculada

- GIVEN un usuario autenticado sin `Usuarios.PersonaId`
- WHEN invoca `GET MisActivos`
- THEN no devuelve activos (lista vacía) sin error de permiso

### Requirement: MA-003 — Activos vigentes y por aceptar

El sistema MUST mostrar los activos vigentes (aceptados) y los "por aceptar" (Status 1) del usuario.

#### Scenario: Por aceptar

- GIVEN un usuario con asignaciones donde `FechaConfirmacion IS NULL` y `FechaFin IS NULL`
- WHEN abre "Mis Activos"
- THEN las ve listadas como "por aceptar"

#### Scenario: Vigentes

- GIVEN un usuario con asignaciones aceptadas (`FechaConfirmacion IS NOT NULL`)
- WHEN abre "Mis Activos"
- THEN las ve listadas como vigentes

### Requirement: MA-004 — Aceptación sin reautenticación

El sistema MUST permitir aceptar una asignación desde "Mis Activos" sin volver a pedir credenciales (el usuario ya está autenticado).

#### Scenario: Aceptación directa

- GIVEN un usuario autenticado en "Mis Activos" con una asignación por aceptar
- WHEN presiona "Aceptar"
- THEN la asignación se marca Aceptado sin pedir credenciales de nuevo
