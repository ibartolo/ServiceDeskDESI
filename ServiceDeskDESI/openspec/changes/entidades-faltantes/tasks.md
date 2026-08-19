# Tasks: Entidades de dominio faltantes (RBAC y recuperación) + resolución `Compania` vs `Empresa`

> Cambio `entidades-faltantes` — refactor sin spec/design formales. Fuente de verdad: `proposal.md`.
> Nullabilidad verificada en `openspec/basededatosservicedesk.txt`: las 3 tablas (`RolPaginaAccion`, `UsuarioRol`, `TokenRecuperacion`) tienen `CreadoPor`/`FechaCreacion`/`Estatus` NOT NULL y `ModificadoPor`/`FechaModificacion` NULL → `BaseObject` encaja sin nullables.
> ⚠️ `ServiceDeskDESIEntities.csproj` es old-style con `<Compile Include>` explícito: cada archivo nuevo debe registrarse ahí.

## Bloque 1: Entidades nuevas (`ServiceDeskDESIEntities/Seguridad/`)

- [x] 1.1 Crear `ServiceDeskDESIEntities/Seguridad/RolPaginaAccion.cs`: `RolPaginaAccion : BaseObject` con `RolId long`, `PaginaId long`, `PuedeLeer/Crear/Editar/Eliminar/Exportar bool`.
- [x] 1.2 Crear `ServiceDeskDESIEntities/Seguridad/UsuarioRol.cs`: `UsuarioRol : BaseObject` con `UsuarioId long`, `RolId long`.
- [x] 1.3 Crear `ServiceDeskDESIEntities/Seguridad/TokenRecuperacion.cs`: `TokenRecuperacion : BaseObject` con `UsuarioId long`, `Token string`, `FechaExpiracion DateTime`, `Usado bool`.
- [x] 1.4 Registrar los 3 archivos en `ServiceDeskDESIEntities/ServiceDeskDESIEntities.csproj` (3 entradas `<Compile Include="Seguridad\...">`).

## Bloque 2: DTOs de lectura (patrón DTO-hereda-entidad)

- [x] 2.1 Crear `ServiceDeskDESIEntities/Seguridad/RolPaginaAccionDTO.cs`: `RolPaginaAccionDTO : RolPaginaAccion` + `PaginaNombre string`, `Direccion string`.
- [x] 2.2 Crear `ServiceDeskDESIEntities/Seguridad/TokenRecuperacionDTO.cs`: `TokenRecuperacionDTO : TokenRecuperacion` + `Nombre/Apellido/Correo/NombreUsuario string`.
- [x] 2.3 Registrar los 2 DTOs en `ServiceDeskDESIEntities.csproj`.
- [x] 2.4 Compilar `ServiceDeskDESIEntities` (0 errores) — cierra Bloques 1+2.

## Bloque 3: DbWrapper — tipar lecturas

- [x] 3.1 `ServiceDeskDESIWebApi/DAL/DbWrapper.Permisos.cs` `ObtenerPermisosPorRol` (~línea 264): reemplazar `Func<IDataReader, dynamic>` anónimo por `new Func<IDataReader, RolPaginaAccionDTO>(r => LlenarEntidad<RolPaginaAccionDTO>(r))`.
- [x] 3.2 `DbWrapper.Permisos.cs` `ObtenerPermisosPorUsuario` (~línea 179): reemplazar anónimo por `new Func<IDataReader, PermisosViewModel>(r => LlenarEntidad<PermisosViewModel>(r))` (reutilizar `PermisosViewModel` existente, shape idéntico).
- [x] 3.3 `ServiceDeskDESIWebApi/DAL/DbWrapper.Autenticacion.cs` `ObtenerTokenRecuperacion` (~línea 443): reemplazar `Func<IDataReader, dynamic>` por `new Func<IDataReader, TokenRecuperacionDTO>(r => LlenarEntidad<TokenRecuperacionDTO>(r))`.

## Bloque 4: Service — sin `dynamic`

- [x] 4.1 `ServiceDeskDESIWebApi/Services/AutenticacionService.cs` `RestablecerContrasenia` (~línea 404): sustituir `dynamic tokenInfo = tokenResponse.Response;` por `var tokenInfo = (TokenRecuperacionDTO)tokenResponse.Response;`.
- [x] 4.2 Confirmar que no queda otro consumidor de `ObtenerTokenRecuperacion` con acceso dinámico (`AutenticacionService.ObtenerTokenRecuperacion` y `AutenticationController` solo reenvían `ModelResponse`; sin cambio).
- [x] 4.3 Compilar `ServiceDeskDESIWebApi` (0 errores) — cierra Bloques 3+4.

## Bloque 5: Compania — solo documentación

- [x] 5.1 Añadir comentario en `ServiceDeskDESIEntities/Catalogos/Compania.cs` aclarando que es un catálogo simple (4 campos) distinto de `Empresa` (tenant), NO residuo. No eliminar código ni tocar CRUD/SPs/Controller.

## Bloque 6: Verificación

- [x] 6.1 Compilar `ServiceDeskDESI.sln` completo (Entities + WebApi + MVC), 0 errores.
- [x] 6.2 Grep para confirmar 0 `dynamic`/objetos anónimos en `ObtenerPermisosPorRol`, `ObtenerPermisosPorUsuario` y `ObtenerTokenRecuperacion`.
- [ ] 6.3 Smoke test manual: permisos por rol (`Security/ObtenerPermisosPorRol`), permisos por usuario (menú/botones MVC), restablecer contraseña end-to-end, y `Compania` (listado/edición/borrado) operativo.
