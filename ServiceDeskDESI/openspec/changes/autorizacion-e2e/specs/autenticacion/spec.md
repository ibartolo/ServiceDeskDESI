# Autenticación Specification

## Purpose

Define la emisión y validación de tokens OAuth Bearer del WebApi: identidad y roles reales, validación real de clientes, HTTPS obligatorio y CORS restrictivo. Los roles se emiten como claims informativos; la autorización efectiva se resuelve server-side (ver especificación `autorizacion`).

## Requirements

### Requirement: Emisión de token con identidad y roles reales

El sistema MUST emitir el token con la identidad real del usuario (`ClaimTypes.Name` y claim `usuarioId`). El sistema MUST incluir los roles reales del usuario (vía `ObtenerRolesPorUsuario`) como claims. El sistema MUST NOT emitir un rol hardcodeado `role="user"`.

#### Scenario: Autenticación exitosa

- GIVEN un usuario válido con roles `Admin` y `Operador` en la tabla `Rol`
- WHEN se autentica con credenciales correctas
- THEN el token contiene `Name` y `usuarioId` reales
- AND contiene claims de rol `Admin` y `Operador`
- AND no contiene `role="user"` hardcodeado

#### Scenario: Usuario sin roles

- GIVEN un usuario válido sin roles asignados
- WHEN se autentica correctamente
- THEN el token se emite sin claims de rol (lista vacía)
- AND la emisión no falla por ausencia de roles

### Requirement: Validación real de clientes OAuth

El sistema MUST validar `client_id` y `client_secret` antes de emitir el token. `ValidateClientAuthentication` MUST NOT validar a ciegas. Un cliente desconocido o con secreto inválido MUST ser rechazado.

#### Scenario: Cliente válido

- GIVEN un cliente registrado con `client_id` y `client_secret` correctos
- WHEN solicita un token
- THEN se invoca `context.Validated()` y el flujo continúa

#### Scenario: Cliente desconocido

- GIVEN un `client_id` no registrado
- WHEN solicita un token
- THEN la solicitud se rechaza (`context.Rejected()`)

#### Scenario: Secreto inválido

- GIVEN un `client_id` registrado con `client_secret` incorrecto
- WHEN solicita un token
- THEN la solicitud se rechaza

### Requirement: HTTPS obligatorio

En configuración de release, el sistema MUST rechazar la emisión de tokens sobre HTTP plano.

#### Scenario: Solicitud por HTTPS

- GIVEN un cliente autenticándose sobre HTTPS
- WHEN solicita un token
- THEN el token se emite

#### Scenario: Solicitud por HTTP en release

- GIVEN un cliente autenticándose sobre HTTP en configuración release
- WHEN solicita un token
- THEN la solicitud se rechaza (HTTP inseguro no permitido)

### Requirement: CORS restrictivo

El sistema MUST permitir CORS solo para orígenes autorizados. `Access-Control-Allow-Origin` MUST NOT ser `*`.

#### Scenario: Origen autorizado

- GIVEN una solicitud desde un origen permitido
- WHEN llama a un endpoint del WebApi
- THEN la respuesta incluye ese origen específico en `Access-Control-Allow-Origin`

#### Scenario: Origen no autorizado

- GIVEN una solicitud desde un origen no permitido
- WHEN llama a un endpoint del WebApi
- THEN la respuesta no incluye `Access-Control-Allow-Origin` para ese origen

### Requirement: Endpoints de autenticación anónimos acotados

Solo un conjunto explícito de acciones MUST permanecer anónimo: `autenticar`, `ValidarRecetearContrasenia`, `validarToken`, `restablecerContrasenia` y el registro de empresa pre-login. El resto de `AutenticationController` MUST requerir token.

#### Scenario: Login sin token

- GIVEN un usuario sin token
- WHEN invoca `autenticar`
- THEN la acción se ejecuta sin autenticación

#### Scenario: Acción de usuarios sin token

- GIVEN un usuario sin token
- WHEN invoca una acción de lectura/escritura de usuarios no permitida
- THEN la acción se rechaza por falta de token
