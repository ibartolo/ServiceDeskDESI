# Verification Report: personal-administracion

- **Change**: `personal-administracion`
- **Versión spec**: menu-etiquetas (MEN-001..005)
- **Modo**: Standard (strict_tdd = false — sin tests automatizados; verificación estática + build + evidencia BD)
- **Fecha**: 2026-08-26
- **Resultado (veredicto)**: **PASS WITH WARNINGS**

---

## Resumen ejecutivo

La implementación de código y SQL (T1–T6) está completa y es correcta. La migración es idempotente y fue aplicada a la BD hosted (evidencia: `NombreVisible` backfilleado en 19 filas; `Usuarios` Id 4 → 'Administración', `Personas` Id 20 → 'Personal'). El build compila 0 errores en los 3 proyectos. La verificación estática confirma MEN-001..005 y D1..D6, con una desviación **documentada y segura**: los `UPDATE` usan `WHERE Nombre = ...` en lugar de `WHERE Id = ...`. Quedan pendientes los smoke tests manuales (T8–T10) y la actualización del dump (T11, opcional), más un desfase de bookkeeping en `tasks.md` (T7 sin marcar `[x]`). Ningún hallazgo es defecto de código.

---

## 1. Completeness (vs tasks.md)

| Métrica | Valor |
|---|---|
| Tareas totales | 11 |
| Tareas completas `[x]` | 6 (T1–T6) |
| Tareas incompletas `[ ]` | 5 (T7–T11) |

| Tarea | Estado en tasks.md | Estado real | Nota |
|---|---|---|---|
| T1 migration.sql | `[x]` | ✅ Hecho | Evidencia en `migration.sql` |
| T2 rollback.sql | `[x]` | ✅ Hecho | Evidencia en `rollback.sql` |
| T3 Pagina.cs | `[x]` | ✅ Hecho | `Pagina.cs:8` |
| T4/T5 MenusUser.cshtml | `[x]` | ✅ Hecho | `MenusUser.cshtml:23,34,46` |
| T6 Build | `[x]` | ✅ Hecho | Re-ejecutado: EXIT_CODE=0 |
| T7 Aplicar migración BD hosted | `[ ]` | ✅ Aplicado (según orquestador) | ⚠️ tasks.md no marcado `[x]` |
| T8 Smoke sidebar | `[ ]` | ⏳ Pendiente | Manual E2E |
| T9 Regresión permisos | `[ ]` | ⏳ Pendiente | Manual E2E |
| T10 Chooser Permisos.cshtml | `[ ]` | ⏳ Pendiente | Verificado estático, no runtime |
| T11 Actualizar dump | `[ ]` | ⏳ Pendiente (opcional) | Drift detectado |

**Hallazgo WARNING**: `tasks.md` muestra T7 como `[ ]`, pero el orquestador reporta que la migración ya fue aplicada a la BD hosted y verificada (backfill 19 filas; Id 4 → 'Administración', Id 20 → 'Personal'). Desfase de bookkeeping: T7 debería marcarse `[x]`.

---

## 2. Build (ejecución real)

```
MSBuild.exe ServiceDeskDESI.sln /t:Build /p:Configuration=Debug
  ServiceDeskDESIEntities -> ...\bin\Debug\ServiceDeskDESIEntities.dll
  ServiceDeskDESIMVC      -> ...\bin\Debug\ServiceDeskDESIMVC.dll
  ServiceDeskDESIWebApi   -> ...\bin\Debug\ServiceDeskDESIWebApi.dll
EXIT_CODE=0
```

✅ **0 errores**. Sin warnings nuevos (los warnings CS0168/CS1998 son pre-existentes, no de este cambio). Confirmado que la propiedad `NombreVisible` fluye vía `SELECT p.*` + `LlenarEntidad<T>` sin tocar DAL/SPs.

---

## 3. Correctness (estático — evidencia estructural)

| Requisito | Estado | Evidencia |
|---|---|---|
| **MEN-001** separación etiqueta vs llave | ✅ Implementado | `Pagina.cs:7-8` (`Nombre` + `NombreVisible` string nullable); `migration.sql:9-10` (`ADD [NombreVisible] NVARCHAR(250) NULL`); `Nombre` queda intacto como llave (SP `ObtenerPaginaPorNombre` en `basededatosservicedesk.txt:4345` → `WHERE Nombre = @Nombre` sin cambio) |
| **MEN-002** render con fallback | ✅ Implementado | `MenusUser.cshtml:23,34,46` → `@(menu.NombreVisible ?? menu.Nombre)` / `@(sub.NombreVisible ?? sub.Nombre)` (3 sitios exactos) |
| **MEN-003** renombre de 2 ítems | ✅ Implementado | `migration.sql:18` (`SET NombreVisible='Personal' WHERE Nombre='Personas'`) y `:20` (`... 'Administración' WHERE Nombre='Usuarios'`); resto cubierto por backfill `:14` (`= Nombre`) |
| **MEN-004** migración idempotente + rollback | ✅ Implementado | `migration.sql:9` (`IF COL_LENGTH('dbo.Pagina','NombreVisible') IS NULL`) + backfill `:14` + 2 UPDATE `:18,:20`; `rollback.sql:7,9` orden inverso conservador (DROP comentado `:13-14`) |
| **MEN-005** no-regresión de permisos | ✅ Implementado | Ver evidencia abajo |

### MEN-005 — evidencia de no-regresión (grep exhaustivo de `NombreVisible`)

`NombreVisible` aparece **únicamente** en:
- `Pagina.cs:8` (POCO) — nuevo.
- `MenusUser.cshtml:23,34,46` — render (nuevo).
- Documentación (spec/design/proposal/explore/tasks/migration/rollback).

**NO aparece** en ningún SP, atributo `[Permiso]`, servicio, controller ni DAL. Confirmado intacto:
- `[Permiso("Usuarios")]`: `UserController.cs:239,278`; `AutenticationController.cs:56,69,96,163` — sin cambio.
- `[Permiso("Personas")]`: `CatalogsController.cs:782,789,813,821`; `PersonaController.cs:55,69`; `PersonaActivoController.cs:29,41,53,65` — sin cambio.
- Comparaciones `PaginaNombre == "Personas"` (`PersonaService.cs:50`) y `"Usuarios"` (`UsuarioService.cs:65`) — sin cambio.
- SP `ObtenerPaginaPorNombre` (`basededatosservicedesk.txt:4337-4346`): `WHERE Nombre = @Nombre AND Estatus = 1` — sin cambio; `DbWrapper.Paginas.cs:41-74` sigue pasando `@Nombre`.
- SP `ObtenerPaginasPorUsuario` (`basededatosservicedesk.txt:4372-4402`): `SELECT DISTINCT p.*` — propaga la columna nueva sin edición.
- `LlenarEntidad<T>` (`DbWrapper.cs:28-61`): mapeo case-insensitive por nombre de columna ↔ propiedad (`reader.GetName(j).ToUpper().Equals(item.Name.ToUpper())`) — `NombreVisible` (BD) ↔ `NombreVisible` (POCO) resuelven sin tocar DAL.
- Chooser `Permisos.cshtml`: sigue usando `pagina.Nombre` (`Permisos.cshtml:416,427,437,458,480,517,541`); **no** usa `NombreVisible`. (MEN-005 escenario "Chooser de permisos intacto" ✅)

---

## 4. Spec Compliance Matrix

> strict_tdd = false ⇒ no hay tests automatizados. La validación de comportamiento es **estática + build + evidencia de datos BD**. Se marca "COMPLIANT (static)" cuando la evidencia estructural y de datos respalda el escenario; "PENDING (smoke)" cuando requiere E2E manual.

| Requisito | Escenario | Estado | Evidencia |
|---|---|---|---|
| MEN-001 | Etiqueta explícita ('Personas'→'Personal') | ✅ COMPLIANT (static) | `migration.sql:18` + fallback `MenusUser.cshtml:23` |
| MEN-001 | Fallback por NULL ('Áreas' → `Nombre`) | ✅ COMPLIANT (static) | `?? Nombre` en `MenusUser.cshtml:23,34,46`; backfill `migration.sql:14` garantiza no-NULL salvo nuevas filas |
| MEN-002 | Menú muestra etiqueta visible | ✅ COMPLIANT (static) | `MenusUser.cshtml:23,34,46` |
| MEN-002 | Render no afecta permisos | ✅ COMPLIANT (static) | grep: `NombreVisible` ausente en permisos; SPs/comparaciones usan `Nombre` |
| MEN-003 | Renombre en pantalla Id 20/4 | ✅ COMPLIANT (static + DB) | `migration.sql:18,20`; orquestador confirma datos BD: Id4→'Administración', Id20→'Personal' |
| MEN-003 | Resto de ítems sin cambios | ✅ COMPLIANT (static + DB) | backfill `migration.sql:14` (19 filas = `Nombre`) |
| MEN-004 | Aplicación inicial | ✅ COMPLIANT (static + DB) | `COL_LENGTH` guard + ADD + backfill + 2 UPDATE; migración aplicada |
| MEN-004 | Re-ejecución idempotente | ✅ COMPLIANT (static) | `IF COL_LENGTH(...) IS NULL` + backfill `WHERE NombreVisible IS NULL` (re-ejecutable sin duplicar) |
| MEN-004 | Rollback | ✅ COMPLIANT (static) | `rollback.sql:7,9` revierte los 2 valores en orden inverso |
| MEN-005 | Acceso tras el renombre | ⚠️ PENDING (smoke T9) | Estático ✅ (SP/comparaciones por `Nombre` intactos); runtime no ejecutado |
| MEN-005 | Chooser de permisos intacto | ✅ COMPLIANT (static) | `Permisos.cshtml` usa `pagina.Nombre` (sin `NombreVisible`) |

**Resumen**: 11/11 escenarios con evidencia estática/DB; 1 escenario (acceso runtime T9) pendiente de smoke manual por falta de tests.

---

## 5. Coherence (vs design D1–D6)

| Decisión | ¿Seguida? | Nota |
|---|---|---|
| D1 columna nueva + fallback `?? Nombre` | ✅ Sí | `migration.sql:9-10` + `MenusUser.cshtml:23,34,46` |
| D2 nullable + backfill `= Nombre` (no NOT NULL) | ✅ Sí | `migration.sql:10,14` |
| D3 `nvarchar(250) NULL`, sin índice | ✅ Sí | `NVARCHAR(250) NULL`; no se crea índice |
| D4 propiedad POCO, sin DTO, sin cambio csproj | ✅ Sí | `Pagina.cs:8`; no existe `PaginaDTO`; `.csproj` sin cambios |
| D5 solo `MenusUser.cshtml` | ✅ Sí | `Permisos.cshtml` y títulos intactos |
| D6 migración idempotente + rollback inverso | ✅ Sí (con desviación) | Idempotente y rollback correctos, **pero** los UPDATE usan `WHERE Nombre` en vez de `WHERE Id` |

**Desviación documentada (D6/T1/T2)**: `design.md` (contratos SQL líneas 50-54, 59-60) y `tasks.md` (T1/T2) describen `UPDATE ... WHERE Id = 4/20`. La implementación real usa `WHERE Nombre = 'Usuarios'/'Personas'` (`migration.sql:18,20`; `rollback.sql:7,9`). Es **funcionalmente equivalente** y más robusto a cambios de Id (explore.md confirma Id 4 = 'Usuarios', Id 20 = 'Personas'). La desviación está registrada en el apply-progress (memoria #163). **Acción sugerida**: actualizar `design.md` y `tasks.md` para reflejar el criterio real (`WHERE Nombre`) y evitar ambigüedad.

---

## 6. Issues Found

### CRITICAL
- Ninguno.

### WARNING
1. **tasks.md desincronizado**: T7 aparece `[ ]` pero la migración ya fue aplicada y verificada en la BD hosted. Debe marcarse `[x]` (bookkeeping).
2. **Desviación Nombre vs Id no reflejada en docs**: `design.md` y `tasks.md` aún describen `WHERE Id = 4/20`; la implementación usa `WHERE Nombre`. Documentada y segura, pero los artefactos deberían alinearse.
3. **Smoke tests T8–T10 no ejecutados**: la validación de comportamiento en runtime (render real del sidebar, ausencia de 403 en `/User/Users` y `/Catalogs/Persona`) está pendiente. La evidencia estática + datos BD es sólida, pero no hay prueba E2E. Sin tests automatizados (strict_tdd=false), esto depende de validación manual.

### SUGGESTION
1. **T11 (drift del dump)**: `basededatosservicedesk.txt` `CREATE TABLE [dbo].[Pagina]` (líneas 273-291) aún NO incluye `NombreVisible`. Actualizarlo evita drift entre el dump y la BD real.
2. **Rollback completo**: el `DROP COLUMN` está comentado (`rollback.sql:13-14`, pregunta abierta del design). El default conservador es válido, pero el path de "reversión total" queda manual.

---

## 7. Veredicto

**PASS WITH WARNINGS**

El cambio está funcionalmente completo y correcto: código y SQL implementados (T1–T6), migración idempotente aplicada a BD, build limpio (0 errores), y verificación estática que confirma MEN-001..005 y D1..D6 sin regresión de permisos. Los pendientes son documentación (T7 sin marcar, dump T11, alinear la desviación Nombre-vs-Id) y validación E2E manual (T8–T10) — ninguno es defecto de código ni bloqueante para archivar, siempre que el orquestador confirme la aceptación del estado actual.
