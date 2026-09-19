# Proposal: Foliador de Tickets (folio/consecutivo)

## Intent
Asignar a cada ticket un folio único, secuencial y por empresa (formato `T-00001`), generado de forma atómica durante el guardado y persistido en `Ticket.Folio`. El consecutivo vive en una nueva tabla `Foliador` (un foliador por empresa, `Nombre='Ticket'`).

## Scope

### In Scope
- Nueva tabla `Foliador` (EmpresaId, FechaActualizacion, Nombre, Descripcion, Consecutivo) + seed `Nombre='Ticket'`.
- Columna `Folio NVARCHAR(50)` en `Ticket` + propiedad `Folio` en `Ticket.cs`/`TicketDTO.cs`.
- SPs: `ConsultarFoliador`, `ActualizarFoliador` (atómico: incrementa y devuelve); modificar `GuardarOActualizarTicket` (param `@Folio`).
- WebApi: `DbWrapper.Foliador.cs` (nuevo) + `t.Folio` en `DbWrapper.Ticket.cs`; `FoliadorService` (Consultar público, ActualizarConsecutivo interno); `FoliadorController` (SOLO consultar por Nombre).
- Integrar cálculo de folio en `TicketService.GuardarTicketConEvidencias` (WebApi) dentro de la transacción existente.
- Threading de `Folio` por toda la cadena MVC→WebApi→SP y vista de captura.

### Out of Scope
- Backfill de folios para tickets existentes (decisión tomada: dejar históricos sin folio, `Folio` nullable).
- Exponer "actualizar foliador" por HTTP.
- Foliadores distintos a `Ticket`.

## Capabilities

### New
- `foliador-tickets`: generación, consulta y persistencia del folio por empresa.

### Modified
- None (openspec/specs/ está vacío).

## Approach
Seguir el patrón `Evidencia` (tabla + 2 SPs + DbWrapper parcial + service + controller mínimo). Incremento atómico en un solo SP con bloqueo (`UPDATE Foliador SET Consecutivo=Consecutivo+1 OUTPUT/SELECT`, `UPDLOCK`/`HOLDLOCK`) scopeado por `EmpresaId`, invocado desde `TicketService.GuardarTicketConEvidencias` dentro del `BeginTransaction/Commit/Rollback` ya existente. El formato `T-{Consecutivo:00000}` se computa SOLO en la capa de servicio WebApi; `Ticket.Folio` guarda el string formateado. El campo Folio del UI es display-only (no se serializa; el valor se re-deriva server-side).

## Affected Areas
| Area | Impacto | Descripción |
|------|---------|-------------|
| `openspec/changes/foliador-tickets/migration.sql` + `rollback.sql` | New | tabla Foliador, col Folio, 2 SPs, modificar GuardarOActualizarTicket |
| `ServiceDeskDESIEntities/Tickets/Ticket.cs`, `TicketDTO.cs` (+.csproj) | Modified | + `Folio` |
| `ServiceDeskDESIEntities/Catalogos/Foliador.cs` (+DTO, +.csproj) | New | entidad Foliador |
| WebApi `DAL/DbWrapper.Foliador.cs` | New | Consultar/Actualizar via SP |
| WebApi `DAL/DbWrapper.Ticket.cs` | Modified | + `t.Folio` en objeto anónimo |
| WebApi `Services/FoliadorService.cs` | New | Consultar (público) + ActualizarConsecutivo (interno) |
| WebApi `Services/TicketService.cs` | Modified | calcular folio en GuardarTicketConEvidencias |
| WebApi `Controllers/FoliadorController.cs` | New | solo `Consultar` |
| WebApi `Controllers/TicketController.cs` | Modified | `LeerTicketDesdeForm` + parse `Folio` |
| MVC `DAL/HttpClientConnection.Foliador.cs` + `Services/FoliadorService.cs` | New | cliente + servicio MVC |
| MVC `Services/TicketService.cs` + `DAL/HttpClientConnection.Ticket.cs` | Modified | + `Folio` al multipart |
| MVC `Controllers/TicketController.cs` | Modified | action `ConsultarFoliador` |
| MVC `Views/Ticket/_CapturarTicket.cshtml` | Modified | campo Folio disabled + populate |

## Key Decisions
- **Multi-tenant**: `EmpresaId` en Foliador; toda operación scopeada por empresa del usuario autenticado.
- **Data access**: ADO.NET + stored procedures (NO EF Core).
- **Atomicidad**: SP único incrementa+y-lee con bloqueo, dentro de la transacción existente.
- **Semántica**: `Consecutivo` INT inicia en 0, +1; formato visual `T-00001` solo en servicio; `Folio NVARCHAR(50)` almacena string.
- **Backfill**: NO se hace backfill; tickets históricos quedan con `Folio` NULL (solo nuevos generan folio). `Ticket.Folio` es nullable.

## Risks
| Riesgo | Prob | Mitigación |
|--------|------|-----------|
| Duplicados por concurrencia | Med | SP atómico UPDLOCK dentro de la transacción |
| Folio se pierde en form parser | Med | actualizar MVC multipart + `LeerTicketDesdeForm` en lockstep |
| Preview vs stored mismatch | Low | stored wins; UI advisory |
| csproj legacy no incluye nuevos .cs | Med | registrar manualmente en `ServiceDeskDESIEntities.csproj` |
| Reflection extra params a SPs | Low | `Folio` via objeto anónimo explícito, no entidad completa |

## Rollback Plan
`rollback.sql`: DROP columna `Ticket.Folio`, DROP tabla `Foliador`, DROP SPs nuevos, restaurar `GuardarOActualizarTicket` previo. Revertir archivos C# (quitar `Folio` y archivos Foliador).

## Dependencies
- Aplicar `migration.sql` manualmente contra DB hosted (`db_9c7990_servicedeskdesi`); no hay runner de migraciones.

## Success Criteria
- [ ] Cada ticket guardado recibe folio único, secuencial y por empresa (`T-00001`, ...).
- [ ] Sin duplicados bajo guardado concurrente.
- [ ] `Foliador` consultable vía API (Nombre='Ticket'); actualizar NO expuesto por HTTP.
- [ ] Folio visible en captura/detalle.

## Open Questions
- ~~¿Backfill de `Folio` para tickets existentes?~~ → Resuelto: no backfill (históricos NULL).
- ~~¿`Ticket.Folio` NOT NULL o nullable?~~ → Resuelto: nullable (Q1-a).
