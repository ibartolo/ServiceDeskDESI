# Verification Report — `fk-escalares` (E2)

- **Change**: `fk-escalares`
- **Versión spec**: N/A (refactor sin spec formal; fuente de verdad = `proposal.md`)
- **Modo**: Standard (no `strict_tdd` en `openspec/config.yaml`; sin test runner)
- **Fecha**: 2026-08-18
- **Ejecutor**: sdd-verify

---

## Resumen ejecutivo

La conversión de FKs de navegación a escalares `*Id` está **completa y correcta** a nivel estático y de compilación. Las 8 entidades usan escalares con la nullabilidad exacta del proposal, los 7 DTOs existen y `UsuarioDTO` cubre **ambos** conjuntos de alias de empresa (`EmpresaNombre` y `EmpresaNombreComercial` + trial), el `DbWrapper` quedó libre de bloques de mapeo manual (`new Area(){…}`/`new TicketEstatus(){…}`), `ObtenerParametrosSQL` ya no extrae `.Id` de nav, los SPs `GuardarOActualizarTicket`/`GuardarOActualizarActivo` usan `@AreaId`/`@CategoriaId`/`@SubcategoriaId`/`@TipoActivoId`/`@MarcaId`/`@ModeloId`, y el MVC no conserva ninguna referencia a nav. La solución **compila con 0 errores**.

Quedan **sin verificar en runtime** (requieren smoke test manual con BD viva) las tareas 6.3 y 6.4, que corresponden a los criterios de éxito #5 (listados muestran nombres/colores) y #6 (login + bloqueo de trial vencido).

---

## Completeness (tareas)

| Métrica | Valor |
|---|---|
| Tareas totales | 32 |
| Completadas `[x]` | 30 |
| Incompletas `[ ]` | 2 |

Tareas incompletas:
- `6.3` Smoke test manual por catálogo (tickets, usuarios, activos, categorías, personas, responsables).
- `6.4` Login + bloqueo de trial vencido (`EsPeriodoPrueba`/`FechaVigenciaFin`).

> Ambas son verificaciones manuales de runtime, no automatizables en este paso (requieren instancia + BD). No bloquean la compilación pero sí los criterios de éxito #5/#6 del proposal.

---

## Build & Tests (ejecución real)

**Build**: ✅ Passed — `MSBuild.exe ServiceDeskDESI.sln /t:Build /p:Configuration=Debug` → `EXITCODE=0`, 0 errores.

```
ServiceDeskDESIEntities -> ...\ServiceDeskDESIEntities\bin\Debug\ServiceDeskDESIEntities.dll
ServiceDeskDESIMVC      -> ...\ServiceDeskDESIMVC\bin\ServiceDeskDESIMVC.dll
ServiceDeskDESIWebApi   -> ...\ServiceDeskDESIWebApi\bin\ServiceDeskDESIWebApi.dll
```

**Tests**: ➖ No disponibles (no hay test runner en la solución; sin `test_command` en config).

**Coverage**: ➖ No disponible.

---

## Correctness (estático — entidades, DTOs, DAL, script, MVC)

### 1. Entidades (8/8 correctas)

| Entidad | Escalares observados | Nullabilidad | Estado |
|---|---|---|---|
| `Usuario` (`Autenticacion/Usuario.cs:19,22,23`) | `SucursalId`, `AreaId`, `EmpresaId` | `long?` ×3 | ✅ |
| `Persona` (`Catalogos/Persona.cs:17`) | `PuestoId` | `long` | ✅ |
| `Categoria` (`Catalogos/Categoria.cs:13,14`) | `CategoriaPadreId`, `AreaId` | `long?` / `long` | ✅ |
| `CategoriaResponsable` (`Catalogos/CategoriaResponsable.cs:8,9`) | `CategoriaId`, `UsuarioId` | `long` ×2 | ✅ |
| `Modelo` (`Catalogos/Modelo.cs:13`) | `MarcaId` | `long?` | ✅ |
| `Activo` (`Catalogos/Activo.cs:13,15,16`) | `TipoActivoId`, `MarcaId`, `ModeloId` | `long?` ×3 | ✅ |
| `Ticket` (`Tickets/Ticket.cs:12,13,14,18`) | `AreaId`, `CategoriaId`, `SubcategoriaId`, `TicketEstatusId` | `long`×2 / `long?` / `int` | ✅ |
| `UsuarioPagina` (`Catalogos/UsuarioPagina.cs:13,14`) | `UsuarioId`, `PaginaId` | `long?` ×2 | ✅ |

No queda ninguna propiedad de navegación FK en las 8 entidades. La nullabilidad coincide exactamente con la tabla del proposal.

### 2. DTOs (7/7 correctos)

| DTO | Hereda | Campos flat | Estado |
|---|---|---|---|
| `TicketDTO` | `Ticket` | `AreaNombre`, `CategoriaNombre`, `SubcategoriaNombre`, `EstatusNombre`, `EstatusColor` | ✅ |
| `UsuarioDTO` | `Usuario` | `SucursalNombre`, `AreaNombre`, **`EmpresaNombre`**, **`EmpresaNombreComercial`**, `EmpresaRazonSocial/RFC/Responsable/Direccion/Ciudad/Estado/CodigoPostal/Telefono/CorreoContacto`, `FechaVigenciaInicio`, `FechaVigenciaFin`, `EsPeriodoPrueba` | ✅ |
| `ActivoDTO` | `Activo` | `TipoActivoNombre`, `MarcaNombre`, `ModeloNombre` | ✅ |
| `CategoriaDTO` | `Categoria` | `AreaNombre`, `CategoriaPadreNombre` | ✅ |
| `CategoriaResponsableDTO` | `CategoriaResponsable` | `CategoriaNombre`, `AreaNombre`, `NombreUsuario`, `Nombre`, `Apellido`, `Correo` | ✅ |
| `ModeloDTO` | `Modelo` | `MarcaNombre`, `MarcaDescripcion` | ✅ |
| `PersonaDTO` | `Persona` | `PuestoNombre`, `PuestoDescripcion` | ✅ |

`UsuarioDTO` cubre **ambos** conjuntos de alias confirmados en los SPs:
- `ObtenerUsuarios`/`ObtenerUsuarioPorId` devuelven `e.NombreComercial as EmpresaNombre` (`basededatosservicedesk.txt:5063,5088,5114,5139`) → campo `EmpresaNombre`.
- `AutenticarUsuario` devuelve `e.NombreComercial as EmpresaNombreComercial`, `FechaVigenciaInicio`, `FechaVigenciaFin`, `EsPeriodoPrueba` (`basededatosservicedesk.txt:1131-1144`) → campos `EmpresaNombreComercial` + trial.

### 3. DbWrapper (correcto)

- `DbWrapper.cs:63-77` — `ObtenerParametrosSQL` genera `@{p.Name}` con `p.GetValue(o)`; **eliminada** la rama que extraía `.Id` de nav. ✅
- `DbWrapper.Ticket.cs` — todas las lecturas usan `LlenarEntidad<TicketDTO>`; sin bloques `new Area(){…}`/`new TicketEstatus(){…}`; `GuardarOActualizarTicket` usa `AreaId`/`CategoriaId`/`SubcategoriaId`/`TicketEstatusId`. ✅
- `DbWrapper.Autenticacion.cs` — lecturas con `LlenarEntidad<UsuarioDTO>`; trial check en `AutenticarUsuario` (`:379`) usa `usuario.EsPeriodoPrueba == true` y `usuario.FechaVigenciaFin`; escritura usa `SucursalId`/`AreaId`/`EmpresaId`. ✅
- `DbWrapper.Activo.cs`, `Categoria.cs`, `CategoriaResponsable.cs`, `Modelo.cs`, `Persona.cs`, `UsuarioPagina.cs` — lecturas con `LlenarEntidad<*DTO>` y escritura con escalares `*Id`. ✅

### 4. Script BD (correcto)

- `GuardarOActualizarTicket` — firma y cuerpo usan `@AreaId`/`@CategoriaId`/`@SubcategoriaId` (`basededatosservicedesk.txt:3074-3180` y `changes/fk-escalares/migration.sql:163-264`). ✅
- `GuardarOActualizarActivo` — firma usa `@TipoActivoId`/`@MarcaId`/`@ModeloId` (`basededatosservicedesk.txt:2023-2039` y `migration.sql:21-38`). ✅

### 5. MVC (sin residuos de nav)

- Controllers: `TicketController` (`TicketEstatusId`, `CategoriaPadreId`), `CatalogsController` (`CategoriaPadreId == null`, `AreaId`), `UserController` (`usuario.EmpresaId = tokenCookie.EmpresaID`), `HomeController` (`usuarioAutenticado.EmpresaId ?? 0`). Sin referencias a nav. ✅
- Vistas/JS: DataTables usan campos flat (`AreaNombre`, `EstatusNombre`, `EstatusColor`, `TipoActivoNombre`, `MarcaNombre`, `ModeloNombre`, `CategoriaPadreNombre`, `SucursalNombre`, `PuestoNombre`, …). Guardado JS usa escalares (`Ticket/Index.cshtml:450-456`: `AreaId`, `CategoriaId`, `SubcategoriaId`, `TicketEstatusId`). ✅

---

## Grep de residuos (tarea 6.2)

Patrones ejecutados sobre la solución:
1. `\.(Area|Categoria|Subcategoria|TicketEstatus|Sucursal|Empresa|Puesto|Marca|TipoActivo|Modelo)\.` → **0 matches reales en código**. Los únicos hits son falsos positivos: docs `openspec/*.md`, nombres de archivo en `.csproj` (`DbWrapper.Area.cs`, …), y `Model.Categoria.*` en `CategoriaResponsable.cshtml` (propiedad legítima del ViewModel `CategoriaResponsableViewModel.Categoria` de tipo `CategoriaDTO`, **no** una nav FK).
2. `new (Area|Categoria|Subcategoria|TicketEstatus|Sucursal|Empresa|Puesto|Marca|TipoActivo|Modelo)\(\)\{` → **0 matches en código** (solo en docs `.md`).
3. Complementarios (para cubrir variantes que el patrón de la tarea no captura):
   - `data\s*:\s*['"](…|Modelo)\.` → 0 matches.
   - `(Area|Categoria|…|Modelo)\s*:\s*\{` (objetos de guardado JS) → 0 matches.
   - `\b(Area|…|Modelo)\s*\?\.` (nav con `?.`) → 0 matches en código.

---

## Issues Found

### CRITICAL (rompe compilación o contrato)
**None.** Compilación 0 errores; no quedan referencias a nav ni bloques de mapeo manual; parámetros de escritura alineados entre DAL y SPs.

### WARNING (riesgo funcional / pendiente de verificación runtime)
1. **Tareas 6.3 y 6.4 pendientes (`tasks.md:52-53`).** Los criterios de éxito #5 ("listados muestran nombres/colores igual que antes") y #6 ("login y bloqueo de trial vencido") del proposal no han sido verificados en runtime. El análisis estático confirma que los alias de SP coinciden con los campos DTO (ver sección Correctness), por lo que el riesgo es bajo, pero **requiere smoke test manual** antes de archivar. No es defecto de código.

### SUGGESTION (mejora menor, no bloqueante)
1. **Usings huérfanos en entidades** tras eliminar nav: `Usuario.cs:1` (`using ServiceDeskDESIEntities.Catalogos;`), `CategoriaResponsable.cs:1` (`using ServiceDeskDESIEntities.Autenticacion;`), `UsuarioPagina.cs:1-2` (`using ...Autenticacion;` + `using ...Seguridad;`). Limpieza cosmética.
2. **Archivo muerto** `ServiceDeskDESIWebApi/ServiceDeskDESIWebApi - copia.csproj` — copia del csproj no referenciada por la solución. Fuera de alcance, candidato a borrado.
3. **`ObtenerUsuarioPaginaPorId`** (`DbWrapper.UsuarioPagina.cs:72`) sigue enviando `@Id` con `SqlDbType.Int` aunque la columna es `bigint` (`BaseObject.Id` es `long`). Pre-existente y fuera del alcance de este cambio (no introducido por `fk-escalares`), pero latente.

---

## Verdict

**PASS WITH WARNINGS**

El cambio `fk-escalares` está implementado correctamente y compila sin errores: entidades con escalares `*Id` de nullabilidad exacta, 7 DTOs de lectura (incluido `UsuarioDTO` cubriendo ambos conjuntos de alias), `DbWrapper` sin mapeo manual, SPs con parámetros normalizados y MVC sin residuos de nav. Queda pendiente únicamente el smoke test manual (6.3/6.4) para confirmar los criterios de éxito de runtime #5 y #6.
