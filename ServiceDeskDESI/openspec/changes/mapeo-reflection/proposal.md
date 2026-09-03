# Proposal: Mapeo por reflection — endurecer + TicketEstatus.Id (W10, E3)

- **Change**: `mapeo-reflection`
- **Fase**: propose
- **Fecha**: 2026-08-18
- **Origen**: `security-remediation` Fase 2 — hallazgo ALTO "Mapeo por reflection frágil + FKs como navegación vs *Id + TicketEstatus.Id int/long" (refs **W10, E2, E3**)

## Intent

Cerrar la parte concreta y de bajo riesgo del hallazgo: el fallo en runtime del catálogo de estatus (E3) y la fragilidad del mapeo por reflection (W10). La refactorización estructural de FKs (E2) se trata como un cambio separado.

## Estado

- **E3** (`TicketEstatus.Id` `int` vs `long`): el SP `ObtenerTicketEstatus` hace `SELECT *` y `DbWrapper.Ticket.cs` lo mapea con `LlenarEntidad<TicketEstatus>`; el reflection recibía un `Int32` para una propiedad `Int64` → `ArgumentException` → el catálogo de estatus siempre fallaba. **Corregido** endureciendo `LlenarEntidad<T>`.
- **W10** (reflection frágil, sin widening numérico): **Corregido** añadiendo conversión de tipos (`Convert.ChangeType` + unwrap de nullables) a `LlenarEntidad<T>` y a `MapearPorpiedades<T>`.
- **E2** (FKs como navegación vs columnas `*Id`): **NO incluido aquí** (refactor estructural grande → cambio dedicado, ver "Out of Scope").

## Scope

### In Scope
- Endurecer `LlenarEntidad<T>` y `MapearPorpiedades<T>` en `ServiceDeskDESIWebApi/DAL/DbWrapper.cs` para convertir tipos de forma segura (int↔long, nullables, enums) en vez de castear directo.

### Out of Scope
- **E2**: convertir las 18 propiedades de navegación (8 entidades) a escalares `*Id` y eliminar la duplicación de mapeo manual. Cambio estructural aparte.
- Migración a ORM (Dapper/EF) — fuera de alcance por la propuesta padre.

## Success Criteria
- [ ] `ObtenerTicketEstatus` deja de fallar en runtime (el catálogo de estatus funciona).
- [ ] `ServiceDeskDESI.sln` compila sin errores.
