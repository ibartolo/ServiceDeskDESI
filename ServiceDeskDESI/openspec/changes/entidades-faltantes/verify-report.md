# Verify Report: `entidades-faltantes` (E4/E10/D17)

- **Change**: `entidades-faltantes`
- **Fecha**: 2026-08-18
- **Mode**: Standard (sin spec/design formales; fuente de verdad = `proposal.md`)
- **Fuente de verdad**: `proposal.md`, `tasks.md`

## Resumen ejecutivo

Implementación **correcta y completa** en lo que a código y compilación respecta. Las 3 entidades de dominio (`RolPaginaAccion`, `UsuarioRol`, `TokenRecuperacion`) existen, heredan `BaseObject` y tienen los campos/tipos exactos del proposal. Los 2 DTOs de lectura heredan correctamente sus entidades base. Las 3 lecturas tipadas (`ObtenerPermisosPorRol`, `ObtenerPermisosPorUsuario`, `ObtenerTokenRecuperacion`) usan `LlenarEntidad<T>` sin `dynamic`/anónimos, y `RestablecerContrasenia` usa cast tipado `(TokenRecuperacionDTO)`. `Compania` solo recibió un comentario documental; su CRUD (Controller/Service/DbWrapper) permanece intacto. La solución compila con **0 errores** (Entities + MVC + WebApi).

Único punto abierto: la tarea **6.3 (smoke test manual)** no es ejecutable en esta fase de verificación estática (requiere entorno con BD), por lo que queda pendiente como WARNING.

---

## 1. Completeness (tasks.md)

| Métrica | Valor |
|---|---|
| Tareas totales | 17 |
| Tareas completas `[x]` | 16 |
| Tareas incompletas `[ ]` | 1 |

**Incompleta**: `6.3` — Smoke test manual (permisos por rol, permisos por usuario, restablecer contraseña, Compania). Requiere entorno con BD + UI; fuera del alcance de la verificación estática.

---

## 2. Correctness (evidencia estructural)

### 2.1 Entidades (`ServiceDeskDESIEntities/Seguridad/`)

| Entidad | Herencia | Campos | Estado |
|---|---|---|---|
| `RolPaginaAccion.cs` | `: BaseObject` ✅ | `RolId long`, `PaginaId long`, `PuedeLeer/Crear/Editar/Eliminar/Exportar bool` ✅ | ✅ Correcto |
| `UsuarioRol.cs` | `: BaseObject` ✅ | `UsuarioId long`, `RolId long` ✅ | ✅ Correcto |
| `TokenRecuperacion.cs` | `: BaseObject` ✅ | `UsuarioId long`, `Token string`, `FechaExpiracion DateTime`, `Usado bool` ✅ | ✅ Correcto |

`BaseObject` aporta `Id`, `CreadoPor`, `FechaCreacion`, `ModificadoPor`, `FechaModificacion DateTime?`, `Estatus` (verificado en `BaseObject.cs`).

### 2.2 DTOs

| DTO | Herencia | Campos extra | Estado |
|---|---|---|---|
| `RolPaginaAccionDTO.cs` | `: RolPaginaAccion` ✅ | `PaginaNombre string`, `Direccion string` ✅ | ✅ Correcto |
| `TokenRecuperacionDTO.cs` | `: TokenRecuperacion` ✅ | `Nombre`, `Apellido`, `Correo`, `NombreUsuario` (string) ✅ | ✅ Correcto |

`PermisosViewModel` reutilizado como DTO de lectura de permisos por usuario (shape: `PaginaId`, `PaginaNombre`, `Direccion`, 5 flags bool) — coincide con el anónimo anterior.

### 2.3 csproj (old-style `<Compile Include>`)

Los 5 archivos nuevos están registrados en `ServiceDeskDESIEntities.csproj`:
- `Seguridad\RolPaginaAccion.cs` (línea 73)
- `Seguridad\RolPaginaAccionDTO.cs` (línea 74)
- `Seguridad\TokenRecuperacion.cs` (línea 77)
- `Seguridad\TokenRecuperacionDTO.cs` (línea 78)
- `Seguridad\UsuarioRol.cs` (línea 79)

✅ Correcto (5/5).

### 2.4 DbWrapper — lecturas tipadas

| Método | Archivo:línea | Implementación | `dynamic`/anónimo |
|---|---|---|---|
| `ObtenerPermisosPorRol` | `DbWrapper.Permisos.cs:251` | `LlenarEntidad<RolPaginaAccionDTO>` ✅ | ❌ ninguno |
| `ObtenerPermisosPorUsuario` | `DbWrapper.Permisos.cs:179` | `LlenarEntidad<PermisosViewModel>` ✅ | ❌ ninguno |
| `ObtenerTokenRecuperacion` | `DbWrapper.Autenticacion.cs:443` | `LlenarEntidad<TokenRecuperacionDTO>` ✅ | ❌ ninguno |

✅ Correcto. `LlenarEntidad<T>` genérico confirmado en `DbWrapper.cs:28`.

### 2.5 Service — `RestablecerContrasenia`

`AutenticacionService.cs:404`:
```csharp
var tokenInfo = (TokenRecuperacionDTO)tokenResponse.Response;
```
Cast tipado, sin `dynamic`. ✅

Consumidores de `ObtenerTokenRecuperacion` verificados: `AutenticacionService.ObtenerTokenRecuperacion` (reenvía `ModelResponse`) y `AutenticationController:139` (reenvía `ModelResponse`) — sin acceso dinámico. ✅

### 2.6 `Compania` — solo documentación

- `Catalogos/Compania.cs`: se añadió comentario documental (líneas 9–12) aclarando que es catálogo simple (4 campos), distinto de `Empresa` (tenant). La clase `Compania : BaseObject` permanece intacta. ✅
- `CompaniaController.cs`: CRUD completo (List/GetById/Guardar/Eliminar) intacto. ✅
- `CompaniaService.cs`: validaciones y reenvíos intactos. ✅
- `DbWrapper.Compania.cs`: 4 SPs (`ObtenerCompanias`, `ObtenerCompaniaPorId`, `GuardarOActualizarCompania`, `EliminarCompania`) intactos. ✅

No se eliminó entidad/CRUD/SP/Controller.

---

## 3. Build & Compilación (ejecución real)

**Comando**:
```
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
  ServiceDeskDESI.sln /t:Build /p:Configuration=Debug
```

**Resultado**: `EXITCODE=0` — 0 errores.

```
ServiceDeskDESIEntities -> ...\ServiceDeskDESIEntities\bin\Debug\ServiceDeskDESIEntities.dll
ServiceDeskDESIMVC      -> ...\ServiceDeskDESIMVC\bin\ServiceDeskDESIMVC.dll
ServiceDeskDESIWebApi   -> ...\ServiceDeskDESIWebApi\bin\ServiceDeskDESIWebApi.dll
```

Los 3 proyectos (Entities + MVC + WebApi) compilan correctamente.

---

## 4. Grep de residuos (`dynamic`)

- `DbWrapper.Permisos.cs`: **0** ocurrencias de `dynamic`/objeto anónimo. ✅
- `ObtenerTokenRecuperacion` (`DbWrapper.Autenticacion.cs` + `AutenticacionService.cs` + `AutenticationController.cs`): **0** `dynamic`. ✅
- `ServiceDeskDESIEntities` (todo el proyecto): **0** `dynamic`. ✅
- Todo `ServiceDeskDESIWebApi`: **1** ocurrencia — `PermisosService.cs:140` (`(IEnumerable<dynamic>)rolesResponse.Response`), en `GuardarPermisosRol`, asociada a `ObtenerRolesPorUsuario` (listado de roles). **FUERA de alcance** (no es lectura de permisos ni de token).

---

## 5. Issues Found

### CRITICAL (debe corregirse antes de archivar)
Ninguno.

### WARNING (debería corregirse)
- **`tasks.md:6.3`** — Smoke test manual pendiente (permisos por rol, permisos por usuario/menú MVC, restablecer contraseña end-to-end, Compania operativo). Requiere entorno con BD; no ejecutable en verificación estática. Se recomienda ejecutarlo antes del archive.

### SUGGESTION (mejora, no bloqueante)
- **`ServiceDeskDESIWebApi/Services/PermisosService.cs:140`** — `(IEnumerable<dynamic>)rolesResponse.Response` en `GuardarPermisosRol` (listado de roles vía `ObtenerRolesPorUsuario`). Es el `dynamic` conocido y **fuera de alcance** de este cambio, pero tipar `ObtenerRolesPorUsuario` a una entidad/DTO (`Rol`) eliminaría el último `dynamic` del proyecto.

---

## 6. Verdict

**PASS WITH WARNINGS** — `status: pass`

La implementación de E4/E10/D17 está completa y correcta a nivel de código y compilación. Único pendiente: smoke test manual (6.3) y un `dynamic` residual fuera de alcance (listado de roles).
