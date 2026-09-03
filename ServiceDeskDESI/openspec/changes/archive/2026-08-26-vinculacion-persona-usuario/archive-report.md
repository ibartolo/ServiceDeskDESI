# Archive Report — vinculacion-persona-usuario

- **Change**: `vinculacion-persona-usuario`
- **Archived on**: 2026-08-26
- **Archive location**: `openspec/changes/archive/2026-08-26-vinculacion-persona-usuario/`
- **Artifact store**: openspec (file-based) + engram (`sdd/vinculacion-persona-usuario/archive-report`)
- **Verdict**: **PASS WITH WARNINGS** (única WARNING ya corregida; build real 0 errores)

---

## What was delivered

Relación 1:1 Persona↔Usuario (deducible por `Usuarios.PersonaId`, sin flag), sincronización desde el catálogo de Personas, rediseño del flujo de confirmación de asignación de activos en **aceptación autenticada de 2 estados** (reemplaza el confirm anónimo del change `asignacion-activos`), desvinculación autenticada y la vista "Mis Activos" para el usuario básico.

- **DB (`migration.sql`)**: `Usuarios.PersonaId BIGINT NULL` + FK `FK_Usuarios_Persona` + índice único filtrado `UX_Usuarios_PersonaId` (`WHERE PersonaId IS NOT NULL`); reescritura de `AsignarActivoPersona` (rama `-2` antes de `-1`) y de `ConfirmarRecepcionActivo` (autenticado, retornos `0/1/2/3`); enriquecimiento de `ObtenerActivosPorPersona` y `ObtenerPersonas`; SPs nuevos `VincularPersonaUsuario`, `DesvincularPersonaUsuario`, `ObtenerPersonaIdPorUsuario`, `ObtenerAsignacionPorToken`, `ObtenerPersonaActivoPorId`, `DesvincularActivoPersonaConfirmacion`; `Pagina` "Mis Activos" + `RolPaginaAccion` para los roles `Nombre='Usuario'` (roles 3 y 31).
- **Entities**: `Usuario.PersonaId long?`; `PersonaDTO.UsuarioId/NombreUsuarioVinculado`; `PersonaActivoDTO` enriquecido; nuevo `AsignacionActivoDetalleDTO` (+ `<Compile Include>`).
- **WebApi**: DAL `DbWrapper.PersonaActivo` (rama `-2`, aceptación/desvinculación autenticada, `ObtenerAsignacionPorToken`, `ObtenerPersonaIdPorUsuario`) y `DbWrapper.Persona` (`Vincular/DesvincularPersonaUsuario` con `-3`); services con correo dual + compensación, `ObtenerMisActivos`, `ConfirmarRecepcion` autenticado, `DesvincularConfirmacion`, `IniciarDesvinculacion`; template `Template_DesvinculacionActivo.html` (+ `<Content Include>`); provisioning "Mis Activos" al rol "Usuario" en `EmpresaService`.
- **MVC**: sync Persona↔Usuario en `Persona.cshtml` (botón SVG + modal + doble warning + campos bloqueados); página anónima `VerAsignacion.cshtml` + modal login; vista `MisActivos.cshtml`; `FilterConfig` (`PublicActions` + `VerAsignacion`, sin `ConfirmarRecepcion`).

## Spec sync location

No existía main spec previo para ninguno de los 4 dominios (ni este change ni `asignacion-activos` se habían sincronizado a `openspec/specs/`). Resultado:

| Delta (origen) | Main spec (destino) | Acción |
|---|---|---|
| `specs/vinculacion-persona-usuario/spec.md` (full nuevo) | `openspec/specs/vinculacion-persona-usuario/spec.md` | Copiado íntegro (6 reqs VPU-001..006) |
| `specs/mis-activos/spec.md` (full nuevo) | `openspec/specs/mis-activos/spec.md` | Copiado íntegro (4 reqs MA-001..004) |
| `specs/confirmacion-recepcion-activo/spec.md` (delta MODIFIED) | `openspec/specs/confirmacion-recepcion-activo/spec.md` | **Resuelto a full spec final** (9 reqs CRA-001..009) |
| `specs/notificacion-asignacion-activo/spec.md` (delta MODIFIED) | `openspec/specs/notificacion-asignacion-activo/spec.md` | **Resuelto a full spec final** (7 reqs NAA-001..007) |

> Los 2 deltas MODIFIED se resolvieron contra los full specs originales en el change no-archivado `asignacion-activos` (ADDED → append; MODIFIED → replace; se preservaron los requirements no tocados: **CRA-003, CRA-004** y **NAA-002..005**). Las anotaciones delta "(Previously: …)" se retiraron; el main spec queda limpio como fuente de verdad y SUPERA el flujo anónimo de `asignacion-activos`.

## DB migration status

**APLICADA y verificada (hosted DB `db_9c7990_servicedeskdesi` @ `SQL5105.site4now.net`) — T47.** `migration.sql` ejecutada manualmente vía `sqlcmd -C` (sin runner de migraciones). Verificado: `Usuarios.PersonaId` en `sys.columns`, FK `FK_Usuarios_Persona`, índice `UX_Usuarios_PersonaId`, 6 SPs nuevos, `Pagina` "Mis Activos" + `RolPaginaAccion` (roles 3 y 31). Migración **aditiva e idempotente** (guardas `sys.columns`/`sys.foreign_keys`/`sys.indexes` + DROP/CREATE). Rollback disponible en `rollback.sql` (orden inverso).

**Fix QUOTED_IDENTIFIER (índice filtrado)**: el `CREATE UNIQUE INDEX … WHERE PersonaId IS NOT NULL` (índice filtrado) exige `SET QUOTED_IDENTIFIER ON` en la sesión. La migración ya incluye `SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON;` (líneas 10-11) para evitar el error de creación del índice filtrado en la BD hosted.

## Follow-ups (pendientes — NO bloqueantes)

- **T48–T52 (smoke manual, Lote 11)** — diferidos al usuario: sync Persona↔Usuario + 2 correos (T48); aceptación liga→modal login→Status 2→redirect (T49); desvinculación (T50); Mis Activos menú/lista vacía (T51); permisos `-2`/`3`/sin `[Permiso("Personas")]` (T52).
- **`asignacion-activos`** — change SUPERSEDIDO por éste (flujo anónimo `ConfirmarRecepcion` reemplazado). Sigue sin archivar y se archivará por separado (ya no es fuente de verdad de `confirmacion-recepcion-activo`/`notificacion-asignacion-activo`).

### Sugerencias menores (no bloqueantes, del verify-report)

1. Vista huérfana `Views/Home/ConfirmarRecepcion.cshtml` (y su `<Content Include>` en `ServiceDeskDESIMVC.csproj:226`) — eliminar por limpieza.
2. `VincularPersonaUsuario` no valida si el Usuario objetivo ya está vinculado a OTRA persona (el índice único garantiza 1:1 por persona, no por usuario) — guard opcional.
3. `DesvincularActivoPersona` (desvinculación inmediata) sigue expuesto aunque la UI ya no lo invoca — se conserva como primitivo de compensación (design Assumption 1).

## Warning resuelta

El `verify-report.md` reportaba **1 WARNING** (NAA-001: correo al admin renderizaba `<a href="">` vacío). **RESUELTA post-verify** en `ServiceDeskDESIWebApi/Services/PersonaActivoService.cs:474-478`: `ResolverTemplateAsignacion(...)` ahora elimina por `Regex.Replace` el bloque `<a href="{{UrlConfirmacion}}">…</a>` cuando `urlConfirmacion` es vacío (correo admin), cumpliendo que el admin MUST NOT recibir liga.

## Verdict

**ARCHIVADO — PASS WITH WARNINGS (WARNING corregida; sin issues bloqueantes).**

47/52 tareas completas (T1–T47), build real con **0 errores en los 3 proyectos**, 28/28 escenarios cubiertos estáticamente (VPU/MA/CRA/NAA), 12/12 decisiones de diseño D1–D12 seguidas, migración aplicada y verificada en BD hosted. Los 5 pendientes (T48–T52) son smoke manual de comportamiento, diferidos al usuario y no bloqueantes con `strict_tdd=false`. Cambio **#4/4** (cierre del feature set extendido): `foliador-tickets` (#1), `personal-administracion` (#2), `asignacion-activos` (#3) + `vinculacion-persona-usuario` (este).
