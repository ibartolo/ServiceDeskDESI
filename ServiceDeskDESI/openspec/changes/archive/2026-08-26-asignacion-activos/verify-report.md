# Verification Report — `asignacion-activos`

- **Change**: `asignacion-activos`
- **Fecha**: 2026-08-26
- **Modo**: Standard (strict_tdd=false → verificación estática + build + evidencia BD; sin tests)
- **Verdict**: **PASS WITH WARNINGS**

---

## Resumen ejecutivo

La implementación está **completa en código** (T1–T27) y **compila con 0 errores** (verificado re-ejecutando MSBuild sobre `ServiceDeskDESI.sln`). La migración SQL está aplicada y verificada en la BD hosted (`FechaConfirmacion`/`TokenConfirmacion`, tabla `BitacoraCorreo`, 3 SPs nuevos). Los 10 requisitos (NAA-001…005, CRA-001…005) están cubiertos estáticamente con evidencia de archivos y líneas. Único pendiente: **T28 (smoke manual)** — validación de comportamiento en runtime (envío real de correo, compensación ante SMTP fallido, idempotencia en BD hosted) que no puede probarse por verificación estática.

---

## 1. Completeness (tasks.md)

| Métrica | Valor |
|---------|-------|
| Tareas totales | 28 |
| Completadas `[x]` | 27 (T1–T27) |
| Pendientes `[ ]` | 1 (T28) |

**Pendiente**: `T28` — Smoke manual (asignar → correo Enviado + bitácora; forzar SMTP → compensación + Fallido + IsSuccess=false; abrir enlace → FechaConfirmacion; reabrir → idempotente). Requiere entorno con SMTP real/forzable y BD hosted.

---

## 2. Correctness (estática) — Compliance por requisito

| Req | Estado | Evidencia |
|-----|--------|-----------|
| NAA-001 (correo + 11 placeholders) | ✅ Implementado | `PersonaActivoService.cs:137-151` resuelve los 11 placeholders null-safe (`?? string.Empty`); destinatario `persona.Correo` en `:156`; template `Template_AsignacionActivo.html` contiene los 11 placeholders (líneas 26,35,47,51,55,59,63,67,78,82,86,96,137). |
| NAA-001 esc. Placeholder sin dato | ✅ | `activo.MarcaNombre ?? string.Empty` / `ModeloNombre ?? string.Empty` (`:146-147`) cubren nullable sin error. |
| NAA-002 (token persistido) | ✅ | `Guid.NewGuid()` (`:115`) → `_dbWrapper.GenerarTokenConfirmacion(newId, token)` (`:116`); URL `{BaseUri}Home/ConfirmarRecepcion/{token}` (`:135`). |
| NAA-003 (bitácora Enviado/Fallido) | ✅ | Enviado: `RegistrarBitacoraCorreo(..., "Enviado", null, newId)` (`:168`); Fallido: `CompensarAsignacionFallida` → `RegistrarBitacoraCorreo(..., "Fallido", error, personaActivoId)` (`:273`). `ReferenciaId` = `PersonaActivoId`. |
| NAA-004 (compensación + IsSuccess=false) | ✅ | `CompensarAsignacionFallida` (`:260-279`) desvincula (`DesvincularActivoPersona`) y registra Fallido; todos los caminos de fallo devuelven `IsSuccess=false` con mensaje exacto (`MensajeErrorCorreo`, `:18`). "Compensación también falla" → `catch` + `Log.Error`, sigue devolviendo false (`:266-269`). |
| NAA-005 (resiliencia SMTP) | ✅ | `EmailHelper.EnvioEmaiil` re-lanza (`EmailHelper.cs:83-86`); capturado en `try/catch` (`:154-163`) → compensa. No hay excepción no controlada propagada. |
| CRA-001 (endpoint anónimo + idempotente) | ✅ | `[AllowAnonymous] [HttpGet, Route("confirmarRecepcion/{token:guid}")]` (`PersonaActivoController.cs:76-81`); idempotencia vía SP `WHERE FechaConfirmacion IS NULL` + tri-estado (`migration.sql:217-239`). |
| CRA-002 (confirmación obligatoria) | ✅ | Columna `FechaConfirmacion DATETIME NULL` (`migration.sql:167`); SP fija `GETDATE()` solo si `IS NULL`; pendiente = `FechaConfirmacion IS NULL`. |
| CRA-003 (token sin caducidad) | ✅ | SP `ConfirmarRecepcionActivo` no tiene ningún chequeo de expiración (`migration.sql:219-238`). |
| CRA-004 (token inválido/desconocido) | ✅ | Desconocido → SP `SELECT 0` → servicio mapea `IsSuccess=false` "El enlace de confirmación no es válido o ha sido alterado." (`PersonaActivoService.cs:240-241`). Malformado → `{token:guid}` → 404; servicio además valida `token == Guid.Empty` (`:227`). |
| CRA-005 (admin no puede confirmar) | ✅ | No existe endpoint administrativo de confirmación. Único flujo: WebApi `confirmarRecepcion/{token:guid}` (AllowAnonymous) + página pública `Home.ConfirmarRecepcion`. |

**Compliance**: 10/10 requisitos con evidencia estática. 0 requisitos faltantes.

---

## 3. Coherence (Design D1–D7)

| Decisión | ¿Cumplida? | Notas |
|----------|-----------|-------|
| D1 migración aditiva/idempotente | ✅ | `migration.sql:164-259` usa guards `IF NOT EXISTS` (columnas/índice/tabla) y DROP/CREATE **solo** de los 3 SPs nuevos. Los 5 SPs existentes NO se re-DROP/CREAN. |
| D1.2 BitacoraCorreo sin FK | ✅ | `migration.sql:182-196` — `ReferenciaId BIGINT NULL`, sin FK. |
| D1.1 índice NO único | ✅ | `migration.sql:176-180` — `CREATE INDEX` sin `UNIQUE` (múltiples NULL permitidos). |
| D5 rename template | ✅ | `Templat_AsignacionActivo.html` ya no existe; `Template_AsignacionActivo.html` presente; `ServiceDeskDESIWebApi.csproj:206` apunta al nuevo nombre. |
| D7 `EnvioEmaiil` NO renombrado | ✅ | Sigue `EnvioEmaiil` (`EmailHelper.cs:13`) y todos los call sites (Autenticacion/Empresa/PersonaActivo). |
| D2 entidades | ✅ | `PersonaActivo.cs:11-12` + 2 props; `BitacoraCorreo.cs` no hereda `BaseObject`; registrado en `ServiceDeskDESIEntities.csproj:50`. |
| D3 orquestación | ✅ | Secuencia exacta D3.1 (persistir→token→URL→placeholders→envío→bitácora) + `CompensarAsignacionFallida` + mensaje exacto. |
| D4 flujo confirmación | ✅ | WebApi anónimo + MVC (HomeController:80-86, FilterConfig:31, HttpClientConnection:60-70, wrapper, vista). |
| D6 migración aplicada | ✅ | Evidencia apply-progress: 6 objetos en BD hosted (columnas, tabla, 3 SPs). |

---

## 4. Build

**Re-ejecutado**: `MSBuild.exe ServiceDeskDESI.sln /t:Build /p:Configuration=Debug`
**Resultado**: `EXITCODE=0` — 0 errores en los 3 proyectos. ✅

---

## 5. Issues

### CRITICAL
Ninguno.

### WARNING
1. **T28 (smoke manual) pendiente** — la validación de comportamiento en runtime no se ha ejecutado (envío real de correo, compensación ante SMTP fallido, idempotencia en BD hosted, malformado de token a 404). Es evidencia que no puede sustituirse por análisis estático. No bloquea archive si el usuario lo acepta como pendiente, pero es el único gap real.

### SUGGESTION
1. **Token malformado muestra texto de 404 crudo en MVC** — el diseño (D4.1) acepta que un GUID malformado produzca HTTP 404; el MVC lo renderiza como "Enlace inválido", pero el `Message` mostrado es el texto genérico de `HttpClientBase` ("Error 404 (Not Found) al consumir…") en lugar del mensaje limpio de CRA-004. Mejora cosmética: mapear 404 a mensaje claro.
2. **`EmailHelper.EnvioEmaiil` usa `throw ex`** (re-throw con reset de stack) — pre-existente, no introducido por este cambio. Considerar `throw;` en refactor futuro.
3. **`FechaAsignacion` usa `DateTime.Now`** del servicio, no re-lee `FechaInicio` de BD (open point #2 del design). Deriva sub-segundo, documentada y aceptada.
4. **Bitácora "Enviado" en try/catch silencioso** (`:166-173`) — si el INSERT de bitácora falla tras un envío exitoso, el registro se pierde (solo `Log.Error`). Correcto por diseño (no compensar un correo ya enviado), pero sin reintento.

---

## 6. Notas de verificación

- El patrón anónimo MVC (`ConfirmarRecepcion` sin bearer) replica exactamente `ValidarTokenRecuperacion` (`HttpClientConnection.Autentication.cs:30-41`): `RequestAsync<object>(..., Func<string,string>, ...)` sin `token` → usa el overload con `token = ""` default (`HttpClientBase.cs:61`), que limpia el header Authorization residual.
- La compensación usa `DesvincularActivoPersona` (conserva `FechaFin`/histórico, no borra), consistente con NAA-004.
- `Persona.Correo` null/blank se valida explícitamente (`:126-131`) → compensa con mensaje "El correo del destinatario no está disponible." antes de intentar el envío.
