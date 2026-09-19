# Archive Report — foliador-tickets

**Change**: `foliador-tickets`
**Archived on**: 2026-08-24
**Archive location**: `openspec/changes/archive/2026-08-24-foliador-tickets/`
**Artifact store**: openspec (file-based)

---

## What was delivered

Asignación de un folio único, secuencial y por empresa (`T-00001`) a cada ticket nuevo, generado de forma atómica durante el guardado y persistido en `Ticket.Folio`. El consecutivo vive en una nueva tabla `Foliador` (una fila `Nombre='Ticket'` por empresa).

- **DB**: tabla `Foliador` (PK `(EmpresaId, Nombre)`, FK→Empresa, `Consecutivo` default 0) + seed por empresa; columna `Ticket.Folio NVARCHAR(50) NULL`; SPs `ConsultarFoliador` (público) y `ActualizarFoliador` (interno, `UPDATE ... WITH (UPDLOCK, HOLDLOCK) ... OUTPUT INSERTED.Consecutivo`); `GuardarOActualizarTicket` modificado con `@Folio` (INSERT inserta folio; UPDATE no lo toca).
- **Entities**: `Catalogos/Foliador.cs`, `Catalogos/FoliadorDTO.cs`, y `Ticket.Folio` en `Ticket.cs` (hereda a `TicketDTO`).
- **WebApi**: `DAL/DbWrapper.Foliador.cs` (nuevo), `t.Folio` en `DbWrapper.Ticket.cs`, `Services/FoliadorService.cs` (con `static FormatearFolio`), folio generado dentro de la transacción en `Services/TicketService.cs` (instancia `DbWrapper` compartida), `Controllers/FoliadorController.cs` (solo `Consultar`).
- **MVC**: `DAL/HttpClientConnection.Foliador.cs`, `Services/FoliadorService.cs`, `+Folio` al multipart en `Services/TicketService.cs`, `ConsultarFoliador` en `Controllers/TicketController.cs`, campo `#Folio` en `_CapturarTicket.cshtml`.
- **Folio visible**: columna `Folio` en la lista (`Index.cshtml`) y en el detalle (`_DetalleTicket.cshtml`).

## Spec sync location

- Delta spec `openspec/changes/foliador-tickets/specs/foliador-tickets/spec.md` → **main spec** `openspec/specs/foliador-tickets/spec.md` (creado; no existía main spec previo, por lo que el delta se copió íntegro).
- 7 requirements (FOL-001 … FOL-007), 13 escenarios, sin cambios de estructura. No hubo merge sobre spec existente.

## DB migration status

**Aplicada (hosted DB `db_9c7990_servicedeskdesi`) — ya ejecutada.** `migration.sql` (tabla `Foliador` + seed, `Ticket.Folio`, 3 SPs incluyendo la modificación de `GuardarOActualizarTicket`) fue aplicada manualmente (sin runner de migraciones). Rollback disponible en `rollback.sql` (orden inverso). Migración idempotente (`IF OBJECT_ID(...) IS NULL`, `DROP PROCEDURE` + `CREATE`, seed `WHERE NOT EXISTS`).

## Follow-ups (resueltos)

El `verify-report.md` original reportaba **PASS WITH WARNINGS** con una única WARNING: un camino de creación sin folio en el endpoint legacy `TicketService.GuardarOActualizarTicket` (WebApi) / `api/Ticket/Guardar`. Dicha WARNING quedó **RESUELTA** por el follow-up post-verify:

- **T23** — `TicketService.GuardarOActualizarTicket` (WebApi) ahora genera el folio atómicamente al crear (`ticket.Id <= 0`) dentro de `BeginTransaction`/`CommitTransaction`/`RollbackTransaction`; al actualizar conserva el folio existente.
- **T24** — `Views/Ticket/Index.cshtml`: columna `Folio` en `<thead>` y en `InicializarDataTable()`.
- **T25** — `Views/Ticket/_DetalleTicket.cshtml` + `VerTicket(id)`: muestra el folio en el detalle.

Esto cubre también las SUGGESTION #3 del verify-report (folio visible en lista/detalle) y elimina el único WARNING. El build final pasa con **0 errores** en los 3 proyectos.

## Sugerencias menores restantes (no bloqueantes)

- `DbWrapper.ActualizarFoliador` es `public`; podría ser `internal` (defensa en profundidad) — no expuesto por HTTP.
- Archivo huérfano `ServiceDeskDESIWebApi/ServiceDeskDESIWebApi - copia.csproj` (pre-existente, no referenciado por el `.sln`) — recomendar borrado para evitar drift.

## Verdict

**ARCHIVADO — PASS (sin warnings abiertas).** 25/25 tareas completas (T1–T25), 7/7 requirements satisfechos (estático), 8/8 decisiones de diseño seguidas, todos los `.cs` nuevos registrados en los `.csproj` legacy, build limpio en los 3 proyectos.
