# Spec: Sesión y Expiración

## Requirement: Expiración de sesión sin código muerto

`BaseController` **MUST NOT** contener un `Redirect(...)` cuyo resultado no se asigna ni se devuelve. La validación de sesión y la redirección a login **MUST** residir en el filtro global `AuthenticationFilter`, que valida `Token.ExpirationDate` vía `SessionHelper.EixstSession()`.

#### Scenario: sesión expirada
- **Given** un usuario con el token de sesión expirado
- **When** intenta acceder a una acción protegida del MVC
- **Then** el filtro global lo redirige a `Home/Autentication` sin ejecutar la acción.

## Requirement: PermissionsController instanciable sin DI

`PermissionsController` **MUST** tener un constructor sin parámetros que instancie `PermisosService` manualmente, de modo que el `DefaultControllerFactory` de MVC pueda crearlo sin un contenedor de DI.

#### Scenario: invocar una acción de PermissionsController
- **Given** una petición a `PermissionsController.ConsultarPermisosUsuario`
- **When** MVC crea el controller
- **Then** no se lanza `InvalidOperationException` por falta de constructor sin parámetros.
