# Proposal: Fortificar el filtro multi-tenant (username + EmpresaId)

- **Change**: `filtro-multitenant-fortificado`
- **Fecha**: 2026-09-09
- **Estado**: 📋 **PLANEADO** — pendiente de exploración e implementación (cambio mayor).
- **Origen**: Indicación del usuario: *"tenemos que fortificar nuestro filtro garantizando que se filtre por username y empresa id cada dato que sale de la api, a excepción de lo que sean globales como las páginas"*.

## Intent

Garantizar que **cada dato que devuelve la API** esté filtrado por **`EmpresaId` (y/o el `NombreUsuario` del solicitante)** de forma **explícita y verificable**, salvo los catálogos **globales** (p. ej. `Pagina`).

Hoy el aislamiento es **por inferencia**: muchas tablas de dominio no tienen `EmpresaId` y la pertenencia se deduce con `CreadoPor = Usuarios.NombreUsuario`. Eso es frágil y depende de que cada SP lo haga bien.

## Alcance (a confirmar en exploración)

- **Auditar todos los SPs de lectura** y clasificarlos:
  - **Globales** (catálogos compartidos): `Pagina`, `TicketEstatus`, etc. → sin filtro de empresa.
  - **Por empresa**: todo lo demás → deben filtrar por `EmpresaId` (columna) y/o por el usuario solicitante (`@Usuario`).
- **Tablas sin `EmpresaId`** (se infiere por `CreadoPor`): decidir si se agrega `EmpresaId` explícito (migración) o se refuerza el join por usuario.
- **Capa DAL/API**: garantizar que el `@Usuario`/`EmpresaId` se resuelva **server-side desde el token** (no confiar en parámetros del cliente) — esto ya se inició en el cambio `tenant-isolation`.
- **Pruebas**: casos negativos (un usuario de la empresa A no debe ver datos de la empresa B).

## Contexto previo

- `tenant-isolation` y `tenant-estructural` ya avanzaron en cerrar huecos; quedaron pendientes.
- `security-remediation` menciona SPs de lectura sin filtro (`ObtenerModelos`, `ObtenerUsuarioPagina*`, `ObtenerEmpresa*`) restringidos a admin/billing.
- `database-review` documentó el modelo de multi-tenancy por inferencia.

## Siguiente paso

Fase de **exploración** (`sdd-explore`): inventariar los SPs de lectura, clasificar global vs por-empresa, y decidir si se introduce `EmpresaId` explícito en las tablas que hoy infieren. **No implementar aún.**

## Archivos

- (pendiente) `openspec/changes/filtro-multitenant-fortificado/explore.md`
- (pendiente) inventario de SPs y migraciones
