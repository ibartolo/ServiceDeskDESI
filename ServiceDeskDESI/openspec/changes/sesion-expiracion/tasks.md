# Tasks: sesion-expiracion

## 1. BaseController (M3)
- [x] 1.1 Quitar el bloque muerto `if (tokenCookie?.Token?.ExpirationDate <= DateTime.Now) { CloseSession(); Redirect(...); }` del constructor.

## 2. PermissionsController (M8)
- [x] 2.1 Reemplazar el constructor con inyección por uno sin parámetros que haga `new PermisosService(httpClientConnection)`.

## 3. Verificación
- [x] 3.1 Compilar `ServiceDeskDESI.sln` (0 errores).
- [ ] 3.2 Smoke: sesión expirada redirige a login; `PermissionsController` no crashea al invocarse.
