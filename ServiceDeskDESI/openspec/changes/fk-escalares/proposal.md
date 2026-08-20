# Proposal: FKs escalares `*Id` + DTOs de lectura (E2)

- **Change**: `fk-escalares`
- **Fase**: propose
- **Fecha**: 2026-08-18
- **Origen**: `security-remediation` Fase 2 — hallazgo ALTO "Mapeo por reflection frágil + FKs como navegación vs `*Id` + `TicketEstatus.Id` int/long" (refs **W10, E2, E3**)

## Intent

Cerrar **E2** (único pendiente del hallazgo; E3 y W10 ya cerrados por `mapeo-reflection`): 8 entidades modelan 18 FKs como **propiedades de navegación** (`Usuario.Empresa`, `Ticket.Area`, …) en vez de escalares `*Id`. Como `LlenarEntidad<T>` empareja por nombre exacto, estas propiedades **nunca se mapean automáticamente** y cada método de `DbWrapper` las rellena a mano con alias de SP (`AreaNombre`, `EstatusColor`, …), provocando duplicación masiva (bloque `new Area(){…}` ×7 en `DbWrapper.Ticket.cs`, ×4 en `DbWrapper.Autenticacion.cs`), contrato entidad↔SP implícito y frágil, y nombres de parámetro de escritura inconsistentes (`@Area` vs `@AreaId`).

## Estado

- **E3** (`TicketEstatus.Id` int/long): cerrado por `mapeo-reflection` (endureció `LlenarEntidad<T>`).
- **W10** (reflection frágil): cerrado por `mapeo-reflection` (`Convert.ChangeType`).
- **E2** (FKs → `*Id`): **este cambio**.

## Scope

### In Scope
- Convertir las **18 propiedades de navegación** (8 entidades) a escalares `*Id` y **eliminar** las propiedades de navegación (fuerza el contrato escalar).
- Crear **DTOs de lectura** que preservan los datos "lookup" (nombres/colores) que hoy muestra el MVC.
- Eliminar el mapeo manual `new Area(){…}`/`new TicketEstatus(){…}` en todos los partials de `DbWrapper`.
- Unificar nombres de parámetro de escritura (`@Area`→`@AreaId`) en el SP `GuardarOActualizarTicket`.
- Actualizar MVC (controllers, vistas, JS) al nuevo contrato (escalares + campos flat de DTO).

### Out of Scope
- Migración a ORM (Dapper/EF) — se mantiene ADO.NET + SPs (según propuesta padre).
- Nullabilidad de `BaseObject` (`bool? Estatus`, `DateTime? FechaCreacion`) — hallazgo E5, fase 3.
- `ModelResponse<T>` tipado (E8) y validación por DataAnnotations (E6).
- `UsuarioPagina` legacy (deprecación por `RolPaginaAccion`, D7) — solo se toca su FK.
- `TicketEstatus.Id` sigue `long` (BaseObject) — se mantiene la conversión int→long ya endurecida.

## Approach

**1. Entidades → escalares.** Reemplazar cada nav por su escalar, respetando nullabilidad de la columna:

| Entidad | Propiedad escalar | Tipo |
|---|---|---|
| `Usuario` | `SucursalId`, `AreaId`, `EmpresaId` | `long?` (cols NULL) |
| `Persona` | `PuestoId` | `long` (NOT NULL) |
| `Categoria` | `CategoriaPadreId` / `AreaId` | `long?` / `long` |
| `CategoriaResponsable` | `CategoriaId`, `UsuarioId` | `long` |
| `Modelo` | `MarcaId` | `long?` |
| `Activo` | `TipoActivoId`, `MarcaId`, `ModeloId` | `long?` (normalizando `ID`→`Id`) |
| `Ticket` | `AreaId`, `CategoriaId` / `SubcategoriaId` / `TicketEstatusId` | `long`×2 / `long?` / `int` |
| `UsuarioPagina` | `UsuarioId`, `PaginaId` | `long?` |

> Verificar `EmpresaId` contra la migración de `tenant-estructural` (el script base lo declara `NULL`).

**2. DTOs de lectura.** Los SPs ya devuelven las columnas escalares (`t.*`, `u.*`) **y** los alias lookup. Un DTO que **hereda** la entidad escalar y añade campos flat (`string AreaNombre`, `string EstatusColor`, …) se mapea **automáticamente** por `LlenarEntidad<T>` (match por nombre), eliminando todo mapeo manual:
- `TicketDTO` (hereda `Ticket`): `AreaNombre`, `CategoriaNombre`, `SubcategoriaNombre`, `EstatusNombre`, `EstatusColor`.
- `UsuarioDTO` (hereda `Usuario`): `SucursalNombre`, `AreaNombre`, `EmpresaNombreComercial`, `EmpresaRazonSocial`, …, `EsPeriodoPrueba`, `FechaVigenciaInicio`, `FechaVigenciaFin` (preserva el check de trial en `AutenticarUsuario`).
- `ActivoDTO` (hereda `Activo`): `TipoActivoNombre`, `MarcaNombre`, `ModeloNombre` (+ `Descripcion`).
- `CategoriaDTO` (hereda `Categoria`): `AreaNombre`, `CategoriaPadreNombre`.
- `CategoriaResponsableDTO` (hereda `CategoriaResponsable`): `CategoriaNombre`, `AreaNombre`, `NombreUsuario`, `Nombre`, `Apellido`, `Correo`.
- `ModeloDTO` (hereda `Modelo`): `MarcaNombre`, `MarcaDescripcion`.
- `PersonaDTO` (hereda `Persona`): `PuestoNombre`, `PuestoDescripcion`.

`DbWrapper` devuelve el DTO en `ModelResponse.Response` (los controllers WebApi quedan casi transparentes).

**3. Escritura.** Con escalares, `ObtenerParametrosSQL` recibe la entidad directa (muere la rama "extraer `.Id` de nav") y genera `@AreaId`, `@CategoriaId`, … El parámetro de tenant `@Usuario` se sigue añadiendo explícito. Cambio de SP requerido: **`GuardarOActualizarTicket`** — renombrar `@Area`→`@AreaId`, `@Categoria`→`@CategoriaId`, `@Subcategoria`→`@SubcategoriaId`. Normalización opcional de casing en `GuardarOActualizarActivo` (`@TipoActivoID`→`@TipoActivoId`, etc.). El resto ya usa `*Id`.

**4. MVC.** Vistas/JS pasan de nav anidado a escalares + campos flat: `x => x.Area.Id` → `x => x.AreaId`; DataTable `data: 'Area.Nombre'` → `'AreaNombre'`, `'TicketEstatus.Color'` → `'EstatusColor'`; JS de guardado `Area: { Id }` → `AreaId`. Nota: `People.cshtml` y su JS ya usan `PuestoId`, confirmando la dirección escalar (hoy divergen del POCO `Puesto`).

## Capabilities

### New Capabilities
- `entidades-fk-escalares`: modelo de dominio con FKs escalares `*Id` y DTOs de lectura con datos lookup (contrato JSON de los endpoints de tickets, usuarios, activos, categorías, personas y modelos).

### Modified Capabilities
- None (`openspec/specs/` está vacío).

## Affected Areas

| Área | Impacto | Descripción |
|---|---|---|
| `ServiceDeskDESIEntities/` (8 POCOs + nuevos DTOs) | Modificado | Escalares `*Id`, se eliminan nav; se crean `*DTO` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.cs` | Modificado | Simplificar `ObtenerParametrosSQL` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.{Ticket,Autenticacion,Activo,Categoria,CategoriaResponsable,Modelo,Persona,UsuarioPagina}.cs` | Modificado | Mapeo manual → `LlenarEntidad<DTO>` |
| `ServiceDeskDESIMVC/Controllers/{Ticket,Catalogs,User,Home}Controller.cs` | Modificado | Consumir escalares + DTOs |
| `ServiceDeskDESIMVC/Views/**` (`Ticket/Index`, `Catalogs/*`, `User/*`) | Modificado | Binding y DataTables a escalares/flat |
| Script BD `openspec/basededatosservicedesk.txt` | Modificado | `GuardarOActualizarTicket` (rename params) |

## Riesgos

| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Cambio de contrato JSON rompe el MVC si no se migran todas las vistas/JS a la vez | Alta | Migración en un solo cambio; grep exhaustivo de `.Area.`/`.TicketEstatus.`/etc. y smoke test por catálogo |
| Regresión en `AutenticarUsuario` (trial) al quitar `usuario.Empresa` | Media | `UsuarioDTO` preserva `EsPeriodoPrueba`/`FechaVigenciaFin`; test manual de login con trial vencido |
| Desajuste de nullabilidad (columna `NULL` mapeada a `long`) revive el fallo de reflection | Media | Tabla de nullabilidad anterior; `long?` donde la columna es NULL; `Convert.ChangeType` ya maneja nullables |
| `GuardarOActualizarTicket` renombrado sin actualizar el DAL → error de param faltante | Media | Renombrar SP y DAL en el mismo commit; validar guardado de ticket de punta a punta |
| 107 SPs sin tests | Media | Smoke test manual por endpoint tras el cambio |

## Rollback Plan

Cambio sin migración destructiva de esquema (solo rename de parámetros en `GuardarOActualizarTicket`). Rollback por commit: revertir el `proposal`/commits de Entities + DAL + MVC + script. `GuardarOActualizarTicket` restaurado mantiene los nombres `@Area`/`@Categoria`/`@Subcategoria` originales (el rename es reversible sin pérdida de datos). Backup de BD previo a cualquier `ALTER/CREATE PROCEDURE`.

## Success Criteria

- [ ] `ServiceDeskDESI.sln` compila sin errores (Entities, WebApi, MVC).
- [ ] Ninguna propiedad de navegación FK permanece en las 8 entidades; los escalares `*Id` existen con la nullabilidad correcta.
- [ ] Los bloques `new Area(){…}`/`new TicketEstatus(){…}` desaparecen de `DbWrapper.*` (mapeo vía `LlenarEntidad<DTO>`).
- [ ] `GuardarOActualizarTicket` recibe `@AreaId`/`@CategoriaId`/`@SubcategoriaId` y guarda un ticket de punta a punta.
- [ ] Los listados del MVC (tickets, usuarios, activos, categorías, personas, responsables) muestran nombres/colores igual que antes (smoke test manual por catálogo).
- [ ] El login y el bloqueo de trial vencido siguen funcionando.
