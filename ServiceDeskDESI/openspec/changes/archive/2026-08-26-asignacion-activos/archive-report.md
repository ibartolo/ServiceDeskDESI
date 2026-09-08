# Archive Report — `asignacion-activos`

- **Change**: `asignacion-activos`
- **Fase**: archive
- **Fecha de archivo**: 2026-08-26
- **Veredicto**: **Implementado + migrado + verificado (PASS WITH WARNINGS en su momento) — SUPERSEDIDO**
- **Superseded by**: `vinculacion-persona-usuario` (archivado el 2026-08-26)

---

## 1. Qué se entregó (flujo base de asignación de activos)

El cambio entregó el **flujo base de asignación/desvinculación de activos a personas**, más la infraestructura de notificación por correo y el flujo de confirmación de recepción. Todo quedó **migrado a la BD hosted `db_9c7990_servicedeskdesi`**.

### Base de datos (`PersonaActivo` + SPs)
- Tabla `PersonaActivo` y los 5 SPs del núcleo:
  - `AsignarActivoPersona`
  - `DesvincularActivoPersona`
  - `ObtenerActivosPorPersona`
  - `ObtenerActivosDisponibles`
  - `ObtenerActivos`
- Columnas aditivas en `PersonaActivo`:
  - `FechaConfirmacion DATETIME NULL`
  - `TokenConfirmacion UNIQUEIDENTIFIER NULL`
- Índice no único `IX_PersonaActivo_TokenConfirmacion`.
- Tabla `BitacoraCorreo` (bitácora ligera de envíos, append-only, soft reference `ReferenciaId` sin FK).
- 3 SPs nuevos: `GenerarTokenConfirmacion`, `ConfirmarRecepcionActivo`, `RegistrarBitacoraCorreo`.

### Notificación por correo
- Template `Template_AsignacionActivo.html` (renombrado desde `Templat_AsignacionActivo.html`, con los 11 placeholders resueltos null-safe).
- Envío vía `EmailHelper.EnvioEmaiil` (nombre con typo conservado, usado por otros flujos).
- Registro de cada intento en `BitacoraCorreo` con estado `Enviado`/`Fallido`, error y `ReferenciaId` (= `PersonaActivoId`).
- **Compensación ante fallo**: si el correo falla, se desvincula (`DesvincularActivoPersona`) y la API devuelve `IsSuccess=false` (nunca éxito).

### Confirmación de recepción (flujo anónimo original)
- Endpoint anónimo `[AllowAnonymous] GET api/PersonaActivo/confirmarRecepcion/{token:guid}` (idempotente, tri-estado 0/1/2).
- Página MVC pública `Home.ConfirmarRecepcion` + vista `ConfirmarRecepcion.cshtml`.
- Token GUID **sin caducidad**; confirmación obligatoria (`FechaConfirmacion IS NULL` = pendiente).

---

## 2. Supersedido por `vinculacion-persona-usuario`

El flujo de confirmación/recepción fue **rediseñado y reemplazado** por el cambio `vinculacion-persona-usuario` (ya archivado). Los cambios clave:

- La **confirmación anónima** fue reemplazada por una **aceptación AUTENTICADA**: página anónima con token + modal de login que crea sesión antes de fijar `FechaConfirmacion`.
- El correo de asignación pasó de **1 destinatario** (usuario) a **2 destinatarios** (correo DUAL: admin informativo sin liga + usuario con liga de aceptación).
- Se añadieron **desvincular** (iniciada por admin, completada por el usuario) y la vista **"Mis Activos"**.
- Se añadió validación de usuario vinculado (`-2` persona sin usuario) y correo de desvinculación.

Las versiones **finales** de las 2 capabilities quedaron en:
- `openspec/specs/confirmacion-recepcion-activo/spec.md` (CRA-001…009)
- `openspec/specs/notificacion-asignacion-activo/spec.md` (NAA-001…007)

---

## 3. Sync de specs: NO re-sincronizado

**No se copió ni sobrescribió ningún spec hacia `openspec/specs/`.** Las versiones finales ya están sincronizadas en `openspec/specs/` (por el archive de `vinculacion-persona-usuario`). Re-sincronizar las deltas de este cambio habría **pisado** esas versiones finales. Los specs principales están correctos tal como están.

---

## 4. Veredicto

- **Implementación**: completa en código (T1–T27 de 28), build con 0 errores.
- **Migración**: aplicada y verificada en BD hosted (columnas, tabla, índice, 3 SPs).
- **Verificación**: **PASS WITH WARNINGS** — 10/10 requisitos con evidencia estática; único gap: T28 (smoke manual de runtime) pendiente.
- **Estado final**: **supersedido** por `vinculacion-persona-usuario` (implementado, migrado y archivado).

---

## 5. Contenido del archivo

| Artefacto | Presente |
|-----------|----------|
| `proposal.md` | ✅ |
| `design.md` | ✅ |
| `explore.md` | ✅ |
| `tasks.md` (27/28 `[x]`) | ✅ |
| `verify-report.md` (PASS WITH WARNINGS) | ✅ |
| `migration.sql` | ✅ |
| `rollback.sql` | ✅ |
| `specs/confirmacion-recepcion-activo/spec.md` (delta) | ✅ |
| `specs/notificacion-asignacion-activo/spec.md` (delta) | ✅ |

---

## 6. Notas de trazabilidad

- Origen: exploración de `asignacion-activos` (commits `bd47e85` + `e88aea0`).
- El núcleo asignar/desvincular + UI ya estaba implementado y desplegado antes de este cambio; este cambio cerró los vacíos de notificación/confirmación/bitácora.
- Regla respetada: no re-DROP/CREATE de los 5 SPs existentes; no renombrar `EnvioEmaiil`.
