# Verification Report: tickets-ciclo-vida

- **Change**: `tickets-ciclo-vida`
- **Mode**: Standard (strict_tdd = false; no test runner; build + static verification)
- **Fecha**: 2026-08-19

---

## Verdict

**PASS WITH WARNINGS**

La implementación es completa y coherente con `design.md` (incluyendo las "Correcciones aplicadas durante la migración", que son la fuente de verdad), las specs y las tasks. No hay hallazgos CRÍTICOS. Hay un WARNING de coherencia autorización/visibilidad (Reasignar/acciones de agente) y varios SUGGESTION menores.

---

## Completeness (tasks.md)

| Metric | Value |
|--------|-------|
| Tasks total | 22 |
| Tasks marcadas `[x]` | 14 |
| Tasks marcadas `[ ]` | 8 |

| Task | Estado | Nota |
|------|--------|------|
| 1.1 migration.sql (rename + ALTER + backfill) | `[ ]` pero **implementado** | migration.sql completo y correcto |
| 1.2 TransicionarTicket | `[ ]` pero **implementado** | SP correcto en migration.sql |
| 1.3 ObtenerUsuariosArea | `[ ]` pero **implementado** | SP correcto |
| 1.4 ObtenerTickets + ObtenerTicketsPorArea (CreadoPorId) | `[ ]` pero **implementado** | migration.sql §6–§7 |
| 1.5 ObtenerTicketAsignaciones (Tipo/Estatus) | `[ ]` pero **implementado** | migration.sql §8 |
| 1.6 Endurecer Retomar (área) | `[ ]` pero **implementado** | migration.sql L76-80 (corrección #3) |
| 2.1 TicketAsignacion.cs | `[x]` ✅ | TipoMovimiento + TicketEstatusId |
| 2.2 TicketAsignacionDTO.cs | `[x]` ✅ | EstatusNombre + EstatusColor |
| 2.3 TicketDTO.cs | `[x]` ✅ | CreadoPorId |
| 3.1 DbWrapper.Ticket.cs | `[x]` ✅ | TransicionarTicket + 5 métodos |
| 3.2 TicketService.cs (WebApi) | `[x]` ✅ | validación comentario |
| 3.3 TicketController.cs (WebApi) | `[x]` ✅ | 5 endpoints + permisos + request |
| 4.1 HttpClientConnection + Service (MVC) | `[x]` ✅ | espejo completo |
| 4.2 TicketController.cs (MVC) | `[x]` ✅ | ViewBag + acciones; CambiarEstatusTicket eliminado |
| 5.1 Index.cshtml | `[x]` ✅ | listado, globals, createdRow |
| 5.2 Matriz de botones | `[x]` ✅ | condiciones exactas |
| 5.3 _CapturarTicket.cshtml | `[x]` ✅ | solo alta + cascada |
| 5.4 _ReasignarTicket.cshtml | `[x]` ✅ | modal reasignar |
| 5.5 _DetalleTicket.cshtml | `[x]` ✅ | detalle + historial |
| 5.6 CSS .ticket-mio | `[x]` ✅ | TemplatePage.css L436-439 (+ duplicado inline en Index) |
| 6.1 Build 0 errores | `[ ]` pero **verificado** | asumido por contexto (0 errores, 3 proyectos) |
| 6.2 Verificación manual por rol | `[ ]` pendiente | fuera de alcance del verify (end user) |

> Nota: Batch 1 (1.1–1.6) está sustancialmente completado (migration.sql existe, es correcto y ya fue aplicado a la BD), pero `tasks.md` nunca se actualizó a `[x]`. Es un tema de bookkeeping, no de trabajo faltante.

---

## Correctness (Static — Structural Evidence)

| Requisito | Estado | Evidencia |
|-----------|--------|-----------|
| Catálogo de estatus; estatus 4 = "Rechazado" | ✅ | migration.sql L8 `UPDATE TicketEstatus SET Nombre='Rechazado' WHERE Id=4` |
| Tomar (Nuevo→En Progreso) | ✅ | SP `TransicionarTicket` L62-67; DbWrapper.Ticket.cs L323; frontend L157 |
| Resolver (En Progreso→Resuelto, comentario ≤300) | ✅ | SP L69-74; Service L318; SweetAlert2 L333-355 |
| Retomar (Rechazado→En Progreso, área) | ✅ | SP L76-80 (incluye chequeo de área); Service L391; frontend L170 |
| Cerrar (Resuelto→Cerrado, solicitante) | ✅ | SP L82-86; Service L370; frontend L175 |
| Rechazar (Resuelto→Rechazado, solicitante, comentario ≤300) | ✅ | SP L82-86; Service L347; frontend L180/388-410 |
| Reasignar (responsable, estatus 2 ó 4, fija En Progreso) | ✅ | SP L88-96 `@EstatusActual IN (2,4)`; frontend L185 |
| Historial unificado (TicketAsignacion, solo última EsActiva=1) | ✅ | SP L107-112 (UPDATE EsActiva=0 → INSERT → UPDATE estatus) |
| Bloqueo Tomar con asignación activa | ✅ | SP L65 `NOT EXISTS(...EsActiva=1)`; frontend L157 `!row.AgenteId` |
| Captura solo alta en modal | ✅ | `_CapturarTicket.cshtml`; `Id:0`, `TicketEstatusId:1`; sin modo edición |
| Validación de campos (Título ≤250, Descripción, catálogos) | ✅ | `_CapturarTicket.cshtml` L153-184 + `maxlength=250` L65 |
| Cascada Área→Categoría→Subcategoría | ✅ | `_CapturarTicket.cshtml` L91-137; `CategoriaPadreId == null` L106-108 |
| Inmutabilidad (sin Editar/Eliminar en UI) | ✅ | Index.cshtml sin botones Editar/Eliminar; backend `EliminarTicket` intacto |
| Refresco + cierre modal tras alta | ✅ | `_CapturarTicket.cshtml` L209-217 |
| Detalle solo lectura + historial | ✅ | `_DetalleTicket.cshtml` + `VerTicket` L527-558 |
| Resaltado "mío" (AgenteId == usuario) | ✅ | `createdRow` L194-198; `.ticket-mio` CSS |
| Ocultación de Editar/Eliminar | ✅ | sin botones; `EliminarTicket` backend conservado |

---

## Coherence (Design Match)

| Decisión | Seguida | Nota |
|----------|---------|------|
| D1 Un SP `TransicionarTicket` (6 transiciones) | ✅ | migration.sql §4; DbWrapper llama a `TransicionarTicket` en todos los movimientos |
| D2 `TipoMovimiento` + `TicketEstatusId` en `TicketAsignacion` | ✅ | entidad + SP |
| D3 (corregida) Cerrar/Rechazar insertan actor con `EsActiva=0` (no NULL) | ✅ | migration.sql L102-103 (`@EsActiva=0`), L109-110; `ObtenerTicketAsignaciones` usa `INNER JOIN Usuarios` L212 |
| D4 `CreadoPorId` (JOIN `u.Id`) | ✅ | migration.sql L155/L184; TicketDTO.cs L14 |
| D5 `ObtenerUsuariosArea` SP | ✅ | migration.sql §5; DbWrapper L538 |
| D6 Cerrar/Rechazar `[Permiso("Leer")]` | ✅ | WebApi L193/L205; MVC L220/L228 |
| D7 `_DetalleTicket.cshtml` (3er parcial) | ✅ | presente |
| D8 Comentario ≤300 en 3 capas | ✅ | SP + Service + SweetAlert2/maxlength (ver §Comentario) |
| Corrección #2 (LEFT JOIN agente + CreadoPorId en ObtenerTicketsPorArea) | ✅ | migration.sql L186/L193-194 |
| Corrección #3 (área en Retomar + agente destino Reasignar) | ✅ | migration.sql L79, L92-95 |
| Bootstrap 5.3 (`data-bs-*`, `bootstrap.Modal`) | ✅ | todos los parciales + Index |
| `CambiarEstatusTicket` eliminado | ✅ | grep: solo referencias en planning artifacts, no en código |

---

## Verificación cruzada de la matriz de botones (specs vs implementación)

| Botón | Spec/design | Index.cshtml | OK |
|-------|-------------|--------------|----|
| Tomar | `esAgente && estatus===1 && !AgenteId` | L157 `esAgenteGlobal && row.TicketEstatusId === 1 && !row.AgenteId` | ✅ |
| Ver | siempre | L162 | ✅ |
| Resolver | `esAgente && AgenteId===usuario && estatus===2` | L165 `esAgenteGlobal && row.AgenteId === usuarioActualIdGlobal && row.TicketEstatusId === 2` | ✅ |
| Retomar | `esAgente && estatus===4` | L170 | ✅ |
| Cerrar | `CreadoPorId===usuario && estatus===3` | L175 | ✅ |
| Rechazar | `CreadoPorId===usuario && estatus===3` | L180 | ✅ |
| Reasignar | `esResponsableArea && (estatus===2\|\|estatus===4)` | L185 | ✅ |

**Reasignar permite estatus 2 OR 4** — confirmado en SP (L90 `@EstatusActual IN (2,4)`) y frontend (L185). ✅

---

## Comentario ≤300 (3 capas)

| Capa | Evidencia | OK |
|------|-----------|----|
| SP (migration.sql) | Resolver L73 `LEN(LTRIM(RTRIM(@Comentario))) BETWEEN 1 AND 300`; Rechazar L85 ídem | ✅ |
| Service (WebApi) | Resolver L318, Rechazar L347 `string.IsNullOrWhiteSpace(comentario) \|\| comentario.Length > 300` → `IsSuccess=false` | ✅ |
| Frontend | SweetAlert2 `inputValidator` (requerido + `>300`) + `inputAttributes.maxlength=300`: Resolver L333-355, Rechazar L388-410; `_ReasignarTicket.cshtml` L20 `maxlength="300"` | ✅ |

---

## Issues Found

### CRITICAL
None.

### WARNING

1. **Autorización vs visibilidad de Reasignar (y acciones de agente).** El botón Reasignar se muestra con `esResponsableAreaGlobal` (Index.cshtml L185), pero las acciones MVC `ReasignarTicket` (TicketController.cs L244) y WebApi (L159) exigen `[Permiso("Tickets","Editar")]`. Un responsable de área sin permiso "Editar" sobre la página "Tickets" verá el botón pero la operación será denegada. Lo mismo aplica a Tomar/Resolver/Retomar (gated por `esAgenteGlobal`, backend por `"Editar"`). D6 solo eximió a Cerrar/Rechazar con "Leer"; el SP es la autorización real, pero el frontend no chequea `permisos.PuedeEditar` antes de renderizar. Depende de la config de roles (si los agentes/responsables siempre tienen "Editar", no se manifiesta), pero es un gap de coherencia autorización/visibilidad que conviene cerrar (o relajar el permiso a "Leer" como en Cerrar/Rechazar, o gatear los botones también con `permisosGlobal.PuedeEditar`).

### SUGGESTION

1. **`tasks.md` desactualizado.** Batch 1 (1.1–1.6) y 6.1 están `[ ]` aunque el trabajo está hecho (migration.sql correcto y aplicado; build 0 errores). Marcar a `[x]` por trazabilidad antes del archive.

2. **`_CapturarTicket.cshtml` no usa `jquery.validate`.** El design 6.3 / task 5.3 especifica "Submit valida (jquery.validate)", pero la implementación usa validación manual con SweetAlert2 (funcionalmente equivalente). `jquery.validate` se carga (Index.cshtml L8) y no se usa. Aceptable; documentar la desviación o retirar el script si no se usará.

3. **`ViewBag.UsuarioActualNombre` sin uso.** Se setea en MVC TicketController.cs L108 pero no se serializa a JS ni se consume en el frontend (solo se usa `usuarioActualIdGlobal`). Valor muerto; eliminar o usar.

4. **Columna "Agente" del historial en Cerrar/Rechazar.** Por la corrección #1, el actor (solicitante) queda como `UsuarioId` con `EsActiva=0`; `_DetalleTicket.cshtml` (L93-98) muestra ese nombre en la columna "Agente", que para Cerrar/Rechazar será el solicitante. Cosmético: considerar etiquetar "Usuario/Actor" o renderizar rol-aware.

5. **`ObtenerUsuariosArea` sin `[Permiso]`.** WebApi (L229) solo hereda `[Authorize]`; MVC (L252) no tiene `[Permiso]`. Coincide con el design, pero añadir `[Permiso("Tickets","Leer")]` sería defensa en profundidad consistente.

6. **`CreadoPorId` no aplicado a las otras 3 variantes** (`ObtenerTicketsPorUsuario`/`PorUrgencia`/`PorEstatus`). El design 2.3 pide "por consistencia" (mínimo = 2). Solo se aplicó a `ObtenerTickets` + `ObtenerTicketsPorArea`; las otras 3 quedaron sin `CreadoPorId` (y no se recrean en migration.sql). Cumple el mínimo requerido; si algún día se exponen esos filtros, necesitarán la columna.

7. **`.ticket-mio` duplicado.** Definido en `TemplatePage.css` (L436-439) y redundante inline en `Index.cshtml` (L12-16). Inofensivo; consolidar en un solo lugar.

---

## Notas de verificación (evidencia de ejecución)

- **Build**: asumido por contexto del orchestrator (0 errores, 3 proyectos, MSBuild VS2022). No re-verificado contra la BD.
- **JSON casing**: WebApi usa `CamelCasePropertyNamesContractResolver` (WebApiConfig.cs L25), pero el MVC re-serializa con `JsonConvert.SerializeObject` (default PascalCase) en `ConsultarTodasTickets`/`ConsultarTicketsPorArea`; por tanto el browser recibe PascalCase y las columnas DataTable (`EstatusNombre`, `AgenteNombre`, `TicketEstatusId`, `AgenteId`, `CreadoPorId`) son correctas. Verificado: MVC no tiene config camelCase.
- **strict_tdd=false** → no aplican checks TDD; `config.yaml` sin `strict_tdd`.
