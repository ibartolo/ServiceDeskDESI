# Proposal: Sesión y Expiración Forzada (M1, M3, M8)

- **Change**: `sesion-expiracion`
- **Fase**: propose
- **Fecha**: 2026-08-18
- **Origen**: `security-remediation` Fase 1 — hallazgo CRÍTICO/URGENTE "Sesión/expiración no forzada" (refs **M1, M3, M8**)

## Intent

Garantizar que la sesión del MVC se valide y expire de forma real: que ninguna acción protegida se ejecute sin sesión válida, que la expiración redirija de verdad a login, y que `PermissionsController` sea instanciable sin contenedor de DI.

## Estado tras `autorizacion-e2e`

- **M1 (UserController sin `[Autenticated]` y acciones sueltas de `HomeController`)**: ✅ CERRADO. El filtro global `AuthenticationFilter` (`App_Start/FilterConfig.cs`) impone "seguro por defecto": toda acción fuera de la allowlist exige `SessionHelper.EixstSession()` (que valida `Token.ExpirationDate`) y redirige a `Home/Autentication`. Los atributos `AutenticatedAttribute`/`NoAutenticatedAttribute` fueron retirados (`Helpers/FiltersHelper.cs`).
- **M3 (BaseController muerto)**: ❌ ABIERTO. El constructor de `BaseController` todavía contiene un chequeo de expiración cuyo `Redirect("~/Home/Autentication")` no se asigna ni se devuelve (código muerto; solo ejecuta `CloseSession()`).
- **M8 (PermissionsController roto por DI)**: ❌ ABIERTO. `PermissionsController(PermisosService)` solo tiene constructor con inyección y no existe contenedor DI → `DefaultControllerFactory` lanza `InvalidOperationException` si se invoca. Sus acciones no son referenciadas por ninguna vista/JS, pero el constructor sigue roto.

## Scope

### In Scope
- **M3**: eliminar el bloque muerto de expiración/redirect del constructor de `BaseController` (la redirección real ya la hace el filtro global).
- **M8**: dar a `PermissionsController` un constructor sin parámetros que instancie `PermisosService` manualmente (patrón del resto de controllers).

### Out of Scope
- Eliminar `PermissionsController` por completo (código muerto → Fase 3, M12/M12bis).
- El comportamiento de redirect de peticiones AJAX expiradas (filtro global existente).

## Success Criteria
- [ ] `BaseController` no contiene código muerto de redirect; la expiración se resuelve únicamente en el filtro global.
- [ ] `PermissionsController` es instanciable por el `DefaultControllerFactory` de MVC (constructor sin parámetros).
- [ ] `ServiceDeskDESI.sln` compila sin errores.
