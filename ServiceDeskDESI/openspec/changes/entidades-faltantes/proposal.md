# Proposal: Entidades de dominio faltantes (RBAC y recuperación) + resolución `Compania` vs `Empresa`

- **Change**: `entidades-faltantes`
- **Fase**: propose
- **Fecha**: 2026-08-18
- **Origen**: `security-remediation` Fase 3 — hallazgo MEDIO "FKs sin entidad (`RolPaginaAccion`, `UsuarioRol`, `TokenRecuperacion`) + `Compania` vs `Empresa`" (refs **E4, E10, D17**)

## Intent

Cerrar **E4/E10/D17**: tres tablas del dominio de seguridad (`RolPaginaAccion`, `UsuarioRol`, `TokenRecuperacion`) no tienen entidad de dominio y se manejan con DTOs sueltos, parámetros escalares y un objeto `dynamic`/anónimo (`DbWrapper.Autenticacion.cs:443`). Se crean las 3 entidades y se tipan las lecturas. Además se resuelve la ambigüedad `Compania` vs `Empresa`.

## Estado

- **E4** (3 tablas sin entidad): **este cambio**.
- **E10/D17** (`Compania` vs `Empresa`): **este cambio** — resultado: no eliminar, documentar.

## Scope

### In Scope
- Crear 3 entidades de dominio (heredan `BaseObject`; sin desajuste de nullabilidad).
- Tipar las lecturas de permisos (`ObtenerPermisosPorRol`, `ObtenerPermisosPorUsuario`) y de token (`ObtenerTokenRecuperacion`), eliminando `dynamic`/objetos anónimos.
- Resolver `Compania` vs `Empresa` (documentación; sin eliminar).

### Out of Scope
- `ModelResponse.Response` `object` + `IsSuccess=true` por defecto (E8).
- CSRF / verbos HTTP / `[FromBody]` (WebApi) dentro de MVC (M13/M14).
- Nullabilidad de `BaseObject` (`bool? Estatus`, `DateTime? FechaCreacion`) — hallazgo E5.
- Migrar las **escrituras** de `RolPaginaAccion`/`UsuarioRol` a aceptar la entidad (se mantienen escalares por bajo riesgo).
- Relacionar/mergear `Compania` con `Empresa` (decisión de negocio, diferida).

## Approach

### 1. Nuevas entidades

Las 3 tablas tienen auditoría `CreadoPor/FechaCreacion/Estatus NOT NULL` (a diferencia de E5), por lo que `BaseObject` encaja sin nullables:

| Entidad | Namespace | Campos propios (según script BD) |
|---|---|---|
| `RolPaginaAccion : BaseObject` | `Seguridad` | `RolId long`, `PaginaId long`, `PuedeLeer/Crear/Editar/Eliminar/Exportar bool` |
| `UsuarioRol : BaseObject` | `Seguridad` | `UsuarioId long`, `RolId long` |
| `TokenRecuperacion : BaseObject` | `Seguridad` | `UsuarioId long`, `Token string`, `FechaExpiracion DateTime`, `Usado bool` |

`BaseObject` aporta `Id`, `CreadoPor`, `FechaCreacion`, `ModificadoPor`, `FechaModificacion (DateTime?)`, `Estatus`.

### 2. Lecturas tipadas (patrón DTO-hereda-entidad de `fk-escalares`)

| Método | Hoy | Después |
|---|---|---|
| `ObtenerPermisosPorRol` | anónimo (`rpa.*` + `PaginaNombre`+`Direccion`) | `LlenarEntidad<RolPaginaAccionDTO>`; DTO hereda `RolPaginaAccion` + `PaginaNombre`/`Direccion` |
| `ObtenerPermisosPorUsuario` | anónimo (agregado por página) | `PermisosViewModel` existente (mismo shape exacto) |
| `ObtenerTokenRecuperacion` | `dynamic` (`t.*` + `Nombre/Apellido/Correo/NombreUsuario`) | `LlenarEntidad<TokenRecuperacionDTO>`; DTO hereda `TokenRecuperacion` + 4 campos de usuario; `RestablecerContrasenia` deja de usar `dynamic tokenInfo` |

### 3. DTOs de request (se conservan)

`PermisoRequest`, `ValidarPermisoRequest`, `GuardarPermisosRequest`, `GuardarPermisosMasivoRequest` siguen como payloads de request. `PermisosViewModel` se reutiliza como DTO de lectura (ya existe, shape idéntico al anónimo actual).

### 4. `Compania` vs `Empresa` — no eliminar

`Compania` **no es residuo**: tiene CRUD completo — tabla + 4 SPs (`ObtenerCompanias`, `ObtenerCompaniaPorId`, `GuardarOActualizarCompania`, `EliminarCompania`); WebApi `CompaniaController`/`CompaniaService`/`DbWrapper.Compania.cs`; MVC `CatalogsController.Company` + `Views/Catalogs/Company.cshtml` + `HttpClientConnection.Compania.cs` + `HomeController.NewCompany`. `Empresa` (tenant, vigencia/trial) y `Compania` (catálogo simple de 4 campos) son conceptos distintos que solo comparten `RFC`/`Direccion`. Se **documenta** la coexistencia; sin cambio de código. Merge/relación queda diferido.

## Capabilities

### New Capabilities
- `entidades-permisos-recuperacion`: entidades de dominio `RolPaginaAccion`, `UsuarioRol`, `TokenRecuperacion` y contrato tipado de las lecturas de permisos y de validación de token de recuperación.

### Modified Capabilities
- None (`openspec/specs/` está vacío).

## Affected Areas

| Área | Impacto | Descripción |
|---|---|---|
| `ServiceDeskDESIEntities/Seguridad/` | Nuevo | `RolPaginaAccion.cs`, `UsuarioRol.cs`, `TokenRecuperacion.cs` + DTOs de lectura (`RolPaginaAccionDTO`, `TokenRecuperacionDTO`) |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Permisos.cs` | Modificado | Anónimo → `LlenarEntidad<RolPaginaAccionDTO>` / `PermisosViewModel` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Autenticacion.cs` | Modificado | `dynamic` → `TokenRecuperacionDTO` |
| `ServiceDeskDESIWebApi/Services/AutenticacionService.cs` | Modificado | `RestablecerContrasenia` sin `dynamic` |
| `ServiceDeskDESIEntities/Catalogos/Compania.cs` | Sin cambios | Documentación (comentario) — no eliminar |

## Riesgos

| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Cambio de tipo del `Response` de `ObtenerPermisosPorRol`/`ObtenerTokenRecuperacion` rompe MVC | Media | Mismos nombres de campo que el anónimo actual (solo cambia el tipo CLR); smoke test de permisos y de restablecer contraseña |
| `TokenRecuperacionDTO` omite campos usados por `RestablecerContrasenia` (`Id`, `UsuarioId`, `NombreUsuario`) | Baja | DTO incluye `t.*` + los 4 campos de usuario; verificación de reset end-to-end |
| Desajuste de nullabilidad en las 3 entidades | Baja | Las 3 tablas son `NOT NULL` en auditoría; `FechaModificacion DateTime?` ya coincide con `BaseObject` |
| Regresión en UI de `Compania` si se elimina por error | N/A | No se toca; solo documentación |

## Rollback Plan

Sin migración de BD ni cambios de esquema. Rollback por commit: revertir Entities + DAL + Service. Los métodos conservan firmas y columnas devueltas (solo cambia el tipo CLR del `Response`), por lo que el JSON emitido es idéntico. `Compania` no se modifica.

## Success Criteria

- [ ] `ServiceDeskDESI.sln` compila sin errores (Entities, WebApi, MVC).
- [ ] Existen `RolPaginaAccion`, `UsuarioRol`, `TokenRecuperacion` (heredan `BaseObject`) con campos 1:1 al script BD.
- [ ] Ningún `dynamic`/objeto anónimo permanece en `ObtenerPermisosPorRol`, `ObtenerPermisosPorUsuario` ni `ObtenerTokenRecuperacion`.
- [ ] Los permisos por rol/usuario y el restablecimiento de contraseña funcionan de punta a punta (smoke test manual).
- [ ] `Compania` permanece operativo (listado/edición/borrado) y documentado como entidad distinta de `Empresa`.
