# Autorización Specification

## Purpose

Define el enforcement server-side de permisos basados en `RolPaginaAccion` en el WebApi y el MVC, y el modelo "seguro por defecto" del MVC (filtro global + allowlist). La fuente de verdad es `RolPaginaAccion`; `UsuarioPagina` se usa solo para render de menús.

## Requirements

### Requirement: Fuente de verdad de permisos

El sistema MUST resolver la autorización contra `RolPaginaAccion` server-side por request. `UsuarioPagina` MUST NOT usarse como referencia de seguridad.

#### Scenario: Permiso vía RolPaginaAccion

- GIVEN un usuario cuyo rol tiene la acción en `RolPaginaAccion`
- WHEN ejecuta una acción de escritura
- THEN la acción se permite

#### Scenario: Permiso solo en UsuarioPagina

- GIVEN un usuario con registro en `UsuarioPagina` pero sin la acción en `RolPaginaAccion`
- WHEN ejecuta una acción de escritura
- THEN la acción se deniega

### Requirement: Enforcement de permisos en WebApi

Cada acción de escritura del WebApi MUST validar el permiso contra `RolPaginaAccion` (vía `ValidarPermisoUsuario`) antes de ejecutarse. Un usuario autenticado sin permiso MUST recibir denegación.

#### Scenario: Usuario con permiso

- GIVEN un usuario autenticado con el permiso requerido
- WHEN invoca una acción de escritura
- THEN la acción se ejecuta

#### Scenario: Usuario sin permiso

- GIVEN un usuario autenticado sin el permiso requerido
- WHEN invoca una acción de escritura
- THEN se devuelve 403 y la acción no se ejecuta

#### Scenario: Sin token

- GIVEN una solicitud sin token válido
- WHEN invoca una acción de escritura
- THEN se devuelve 401

### Requirement: Enforcement de permisos en MVC

Las acciones de escritura del MVC MUST validar permisos contra `RolPaginaAccion` server-side. Ocultar botones (`PuedeCrear/Editar/Eliminar`) MUST NOT ser la única protección.

#### Scenario: Escritura con permiso

- GIVEN un usuario de sesión con el permiso requerido
- WHEN ejecuta una acción de escritura en MVC
- THEN la acción se ejecuta

#### Scenario: Escritura sin permiso

- GIVEN un usuario de sesión sin el permiso requerido
- WHEN ejecuta una acción de escritura en MVC
- THEN la acción se deniega

### Requirement: Filtro global con allowlist en MVC

El MVC MUST estar protegido por defecto mediante un filtro de autorización global. Las acciones públicas MUST declararse explícitamente en una allowlist. Una acción no listada MUST requerir autenticación y permisos.

#### Scenario: Acción en allowlist

- GIVEN una acción pública listada en la allowlist
- WHEN un usuario no autenticado la invoca
- THEN se ejecuta sin autenticación

#### Scenario: Acción no listada sin sesión

- GIVEN una acción no listada en la allowlist
- WHEN un usuario sin sesión la invoca
- THEN se deniega (redirige a login)

#### Scenario: Acción no listada con sesión sin permiso

- GIVEN una acción de escritura no listada y un usuario con sesión sin permiso
- WHEN la invoca
- THEN se deniega por falta de permiso

### Requirement: Eliminar anonimato accidental en WebApi

El WebApi MUST NOT exponer endpoints mutantes anónimos fuera del conjunto de autenticación permitido. Los `[AllowAnonymous]` de clase en `AutenticationController` y los sueltos en `EmpresaController`, `RelacionController` y `UsuarioPaginaController` MUST ser eliminados.

#### Scenario: Endpoint mutante sin token

- GIVEN un endpoint mutante que antes tenía `[AllowAnonymous]`
- WHEN se invoca sin token
- THEN se devuelve 401
