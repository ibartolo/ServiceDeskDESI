# Spec: Multi-tenancy (Contención)

Capacidad nueva para el aislamiento de datos entre empresas.

## Requirement: Filtro de tenant en SPs de lectura

Los stored procedures de lectura que hoy carecen de filtro **MUST** restringir los resultados a la empresa del usuario autenticado, resolviendo la pertenencia desde `@Usuario` (derivado del token).

#### Scenario: listar asignaciones usuario-página solo de la propia empresa
- **Given** un usuario autenticado de la empresa A
- **When** invoca `ObtenerUsuarioPagina`
- **Then** el SP solo devuelve filas de `UsuarioPagina` cuyos usuarios pertenecen a la empresa A.

#### Scenario: obtener asignación usuario-página por Id de otra empresa
- **Given** un usuario autenticado de la empresa A
- **When** invoca `ObtenerUsuarioPaginaPorId` con un `Id` de la empresa B
- **Then** el SP no devuelve la fila.

## Requirement: Resolución de usuario por nombre acotada al tenant

`ObtenerUsuarioPorNombreUsuario` **MUST** filtrar por la empresa del usuario autenticado (parámetro `@Usuario`).

#### Scenario: búsqueda de usuario de otra empresa
- **Given** un usuario autenticado de la empresa A
- **When** invoca `ObtenerUsuarioPorNombreUsuario` con un nombre de usuario de la empresa B
- **Then** el SP no devuelve ese usuario.

## Requirement: IDOR cerrado en EliminarTicket

`EliminarTicket` **MUST** validar que el ticket pertenezca a la empresa del usuario autenticado antes de marcarlo como eliminado.

#### Scenario: eliminar ticket de otra empresa
- **Given** un usuario autenticado de la empresa A
- **When** invoca `EliminarTicket` con el `Id` de un ticket de la empresa B
- **Then** el SP no modifica el ticket y devuelve `0`.

## Requirement: Directorio de empresas no expuesto

Los endpoints `GET api/Empresas/List` y `POST api/Empresas/RFC` **MUST NOT** existir.

#### Scenario: listar empresas
- **Given** un cliente HTTP autenticado
- **When** invoca `GET api/Empresas/List`
- **Then** recibe `404 Not Found`.

## Requirement: Unicidad de empresa server-side sin exponer el directorio

El registro de empresa **MUST** validar unicidad de RFC, correo de contacto, nombre comercial y razón social mediante consultas puntuales server-side (no cargando todas las empresas).

#### Scenario: alta con correo duplicado
- **Given** ya existe una empresa con un correo de contacto
- **When** se registra una nueva empresa con el mismo correo
- **Then** el registro se rechaza con mensaje de unicidad.

## Requirement: Claim de tenant en el token

El token OAuth emitido en login **MUST** incluir el claim `empresaId` con el identificador de la empresa del usuario autenticado.

#### Scenario: login exitoso
- **Given** un usuario con empresa asignada
- **When** se emite el token
- **Then** el token contiene un claim `empresaId` igual al `EmpresaId` del usuario.
