# Anomalías detectadas en pruebas (localhost)

- **Fecha inicio**: 2026-09-09
- **Contexto**: pruebas del refactor JS (`js-inline-a-scripts`) + recorrido general del proyecto.
- **Estado**: 🟡 en curso — bugs funcionales corregidos; pendiente el plan de logging.

## Casos anotados

| # | Módulo / Vista | Caso detectado | Esperado | Estado |
|---|---|---|---|---|
| 1 | Registro de empresa (usuario principal) | Username generado sin punto (`juanperez`) | Formato `nombre.apellido` (`juan.perez`) | ✅ **Corregido** |
| 2 | Registro de empresa | CP `93270` | — | ✅ **Cerrado** (no era bug: el CP sale de la dirección que registró la empresa) |
| 3 | MVC — capa de Servicios | Sin logs (datos del token, ej. `EmpresaId`) | Loguear datos derivados de sesión/token | ✅ **Aplicado** (24 services) |
| 4 | WebApi — Service → DbWrapper | No se loguea la data enviada a la BD | Loguear payload (objeto en JSON) | ✅ **Aplicado** (`BaseDbWrapper` + `LogSanitizer`) |
| 5 | WebApi — manejo de errores | El log de servicio no muestra la causa real (`ex.Message`) | Todo `Log.Error` con la excepción | 🟡 Parcial (catch OK; 12 rutas sin excepción anotadas) |
| 6 | WebApi — alta de usuario | Excepción SQL 2601 (username duplicado) | Mensaje amigable; unicidad por empresa | ✅ **Corregido** |
| 7 | Alta de usuarios | El **RFC es campo requerido** al capturar usuarios | El RFC **no** debe ser requerido (opcional) | 📝 Anotado — pendiente |
| 8 | Tickets — tomar ticket | Al **tomar mi propio ticket** (creador = quien lo toma) el SP **devuelve 0** | Debe permitir tomar el ticket | 📝 Anotado — pendiente |

## Detalle de los casos corregidos

### Caso 1 — Formato de username ✅
- `EmpresaService.NormalizarNombreResponsable`: ahora genera `tokens[0] + "." + tokens[apellido]` → `juan.perez`, `ivan.bartolo`.
- `DbWrapper.ExisteNombreUsuario` ahora es **por empresa** (`+ EmpresaId`, activos); `GenerarUsernameAdminUnico` recibe `empresaId`.

### Caso 6 — Username duplicado ✅
- **Causa raíz:** índice `UX_Usuarios_NombreUsuario` **único GLOBAL**, mientras los guards del SP validan **por empresa** → al crear `a.mariano` en empresa 26 existiendo en empresa 1, el guard no lo detectó y el índice global lanzó excepción cruda.
- **Fix aplicado (dev):**
  - Índice reemplazado por `UX_Usuarios_NombreUsuario_Empresa` = único `(NombreUsuario, EmpresaId)` **filtrado `WHERE Estatus = 1`** (permite reusar username de inactivos).
  - SP `GuardarOActualizarUsuarioAdmin`: guard del INSERT alineado con `AND Estatus = 1`.
- **Script para prod:** `openspec/changes/username-por-empresa/migration.sql` (idempotente, sin `USE`, se corre con `-d`).

### Caso 2 — CP 93270 ✅ cerrado
- No está hardcodeado: el CP proviene de la **dirección que registró la empresa**. No requiere cambio.

## Detalle de casos nuevos (pendientes)

### Caso 7 — RFC requerido en alta de usuarios
- Al capturar usuarios, el **RFC figura como campo requerido**. Debe dejar de ser obligatorio (opcional).

### Caso 8 — Tomar el propio ticket devuelve 0
- Escenario: el usuario **creó** el ticket y luego quiere **tomarlo él mismo**.
- Llamada (SP de movimiento de ticket) con:
  ```sql
  @TicketId        BIGINT = 15,
  @TipoMovimiento  NVARCHAR(20) = 'Tomar',   -- Tomar|Resolver|Retomar|Cerrar|Rechazar|Reasignar|PendienteMateriales|EnEsperaTerceros|Reanudar
  @Comentario      NVARCHAR(300) = NULL,
  @NuevoUsuarioId  BIGINT = NULL,
  @FechaEstimada   DATE = NULL,
  @Usuario         NVARCHAR(25) = 'ivan.bartolo'
  ```
- Resultado: **devuelve 0**. Hipótesis a revisar: regla que impide al creador tomar su propio ticket, o validación de permiso/estado que no lo permite.
- Pendiente: identificar el SP exacto y la causa del 0.

## Pendiente

- 📋 Casos 3, 4 y 5 → plan en `openspec/changes/logging-estandarizado/proposal.md` (pendiente aprobar plan 1 / ajustar plan 2).
- 📋 Pendiente general: smoke manual de los flujos por vista tras el refactor JS.
- ⚠️ Nota: el SP `GuardarOActualizarUsuario` (no admin) **no** tiene guards de duplicado; si se usa en flujos donde pueda repetirse el username, podría tirar la excepción del índice. Revisar si aplica.
