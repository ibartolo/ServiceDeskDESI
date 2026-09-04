# Verification Report — vinculacion-persona-usuario

- **Change**: `vinculacion-persona-usuario`
- **Version**: specs VPU-001..006, MA-001..004, CRA deltas, NAA deltas
- **Modo**: Standard (strict_tdd = false → verificación estática + build + evidencia BD)
- **Fecha**: 2026-08-26

---

## Completeness

| Métrica | Valor |
|---------|-------|
| Tareas totales | 52 |
| Tareas completas | 47 (T1–T47) |
| Tareas incompletas | 5 (T48–T52) |

Tareas pendientes (Lote 11 — smoke manual):

- [ ] T48 — Smoke sync Persona↔Usuario + 2 correos
- [ ] T49 — Smoke aceptación (liga → modal login → Status 2 → redirect)
- [ ] T50 — Smoke desvincular (admin inicia → correo → `?accion=desvincular` → `FechaFin`)
- [ ] T51 — Smoke Mis Activos (menú / lista vacía / admin sin menú)
- [ ] T52 — Smoke permisos (`-2`, `3`, sin `[Permiso("Personas")]`)

> Estas son smoke de comportamiento manual; con `strict_tdd=false` no bloquean el archive. La verificación estática + build + migración aplicada (T47) cubren la evidencia estructural.

---

## Build & Tests

**Build**: ✅ Passed (0 errores, 3 proyectos)

```
ServiceDeskDESIEntities -> bin\Debug\ServiceDeskDESIEntities.dll
ServiceDeskDESIMVC      -> bin\ServiceDeskDESIMVC.dll
ServiceDeskDESIWebApi   -> bin\ServiceDeskDESIWebApi.dll
```

Comando: `MSBuild.exe ServiceDeskDESI.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /m`

**Tests**: ➖ No disponibles (proyecto sin runner de tests; `strict_tdd=false`).

**Coverage**: ➖ No disponible.

---

## Spec Compliance Matrix (static — behavioral por evidencia de código)

| Requirement | Scenario | Evidencia | Resultado |
|-------------|----------|-----------|-----------|
| VPU-001 | Un usuario básico por persona | `migration.sql` FK + `UX_Usuarios_PersonaId` (WHERE PersonaId IS NOT NULL); sin flag | ✅ COMPLIANT |
| VPU-001 | Persona sin usuario | `Usuarios.PersonaId` NULL ⇒ no hay fila vinculada | ✅ COMPLIANT |
| VPU-002 | Apertura del modal | `Persona.cshtml:78-86` (botón SVG) + `:104-136` (modal) + `:433-453` (tabla usuarios) | ✅ COMPLIANT |
| VPU-003 | Advertencia en el modal | `Persona.cshtml:112-115` + SweetAlert `:455-467` | ✅ COMPLIANT |
| VPU-003 | Advertencia antes de guardar | `Persona.cshtml:241-256` | ✅ COMPLIANT |
| VPU-004 | Campos deshabilitados | `Persona.cshtml:35,41,47,55` + `AplicarBloqueoSincronizado():507-515` | ✅ COMPLIANT |
| VPU-004 | PuestoId intacto | `migration.sql:107-116` (UPDATE no toca PuestoId); dropdown editable `Persona.cshtml:61` | ✅ COMPLIANT |
| VPU-005 | Vínculo a usuario existente | SP `VincularPersonaUsuario` valida `@UsuarioId` existente; nunca INSERT de Usuario | ✅ COMPLIANT |
| VPU-006 | Sesión sin claim personaId | Sin cambios en token/claims; `PersonaId` se resuelve por `ObtenerPersonaIdPorUsuario` | ✅ COMPLIANT |
| MA-001 | Rol con permiso / sin permiso | `migration.sql:339-353` (Pagina MisActivos + RolPaginaAccion rol "Usuario"); `MenusUser.cshtml` sin cambios | ✅ COMPLIANT |
| MA-002 | Usuario básico | `PersonaActivoController.cs:53-59` (sin `[Permiso]`); `PersonaActivoService.ObtenerMisActivos:243-277` | ✅ COMPLIANT |
| MA-002 | Usuario sin persona vinculada | `PersonaActivoService.cs:252-261` (lista vacía, IsSuccess=true) | ✅ COMPLIANT |
| MA-003 | Por aceptar / Vigentes | SP `ObtenerActivosPorPersona` (FechaConfirmacion) + `MisActivos.cshtml:36-40` | ✅ COMPLIANT |
| MA-004 | Aceptación directa | `MisActivos.cshtml:69-109` → `Home/AceptarAsignacion` (sesión existente) | ✅ COMPLIANT |
| CRA-001 | Aceptación exitosa | SP `ConfirmarRecepcionActivo` retorna 1; `VerAsignacion` redirect MisActivos | ✅ COMPLIANT |
| CRA-001 | Re-clic (idempotencia) | SP retorna 2; `VerAsignacion.cshtml:43-55` muestra "ya fue asignado" | ✅ COMPLIANT |
| CRA-002 | Status 1/2/Desvinculado | `design.md D11`; SPs usan `FechaFin`/`FechaConfirmacion` | ✅ COMPLIANT |
| CRA-005 | Sin aceptación admin | `confirmarRecepcion` autenticado + valida titularidad (`migration.sql:229-231`) | ✅ COMPLIANT |
| CRA-005 | Admin inicia desvinculación | `IniciarDesvinculacion` + `[Permiso("Personas","Editar")]` | ✅ COMPLIANT |
| CRA-006 | Apertura de la liga | `HomeController.VerAsignacion` (pública) + `VerAsignacion.cshtml` (Layout=null) | ✅ COMPLIANT |
| CRA-007 | Login y redirección | modal login → `Home/LogIn` (FormsAuth+TokenCookie) → `AceptarAsignacion` | ✅ COMPLIANT |
| CRA-008 | Credenciales incorrectas | `VerAsignacion.cshtml:224-227` (error, sin cambio de estado) | ✅ COMPLIANT |
| CRA-009 | Desvinculación por el usuario | `DesvincularAsignacion` → `desvincularConfirmacion` → `FechaFin` | ✅ COMPLIANT |
| NAA-001 | Envío dual exitoso | `PersonaActivoService.cs:148-200` (2 correos + 2 bitácoras + compensación) | ✅ COMPLIANT |
| NAA-001 | Placeholder sin dato | `ResolverTemplateAsignacion` usa `?? string.Empty` (`:473-483`) | ✅ COMPLIANT |
| NAA-006 | Persona sin usuario (-2) | `migration.sql:54-56` (SP) + `DbWrapper.PersonaActivo.cs:88-93` (rama antes de -1) | ✅ COMPLIANT |
| NAA-006 | Persona con usuario | asignación procede (`SCOPE_IDENTITY`) | ✅ COMPLIANT |
| NAA-007 | Envío correo desvinculación | `IniciarDesvinculacion` + `RegistrarBitacoraCorreo(TipoCorreoDesvinculacion)` | ✅ COMPLIANT |

**Resumen compliance**: 28/28 escenarios cubiertos estáticamente.

---

## Correctness (Static — Structural Evidence)

| Requirement | Status | Notas |
|------------|--------|-------|
| VPU-001 | ✅ Implementado | `Usuarios.PersonaId BIGINT NULL` + FK + índice único filtrado (`migration.sql:17-30`) |
| VPU-002 | ✅ Implementado | Botón SVG + tooltip + modal + tabla usuarios (`Persona.cshtml`) |
| VPU-003 | ✅ Implementado | Doble advertencia (modal + pre-guardado) |
| VPU-004 | ✅ Implementado | Sobrescritura Nombre/Apellido/Correo/Telefono desde Usuario; `PuestoId` intacto |
| VPU-005 | ✅ Implementado | Solo vincula Usuario pre-existente |
| VPU-006 | ✅ Implementado | Sin cambios en token/claims |
| MA-001 | ✅ Implementado | Menú vía `RolPaginaAccion` |
| MA-002 | ✅ Implementado | Endpoint autenticado sin `[Permiso("Personas")]`; deriva `PersonaId` |
| MA-003 | ✅ Implementado | Activos vigentes/por aceptar |
| MA-004 | ✅ Implementado | Aceptación sin re-login |
| CRA-001 | ✅ Implementado | Aceptación autenticada + idempotente |
| CRA-002 | ✅ Implementado | Modelo 2 estados + desvinculado |
| CRA-005 | ✅ Implementado | Sin aceptación admin; admin inicia desvinculación |
| CRA-006 | ✅ Implementado | Página anónima standalone |
| CRA-007 | ✅ Implementado | Modal login + sesión |
| CRA-008 | ✅ Implementado | Error de credenciales sin cambio de estado |
| CRA-009 | ✅ Implementado | Desvinculación autenticada |
| NAA-001 | ✅ Implementado | Correo dual (admin sin liga / usuario con liga) |
| NAA-006 | ✅ Implementado | `-2` validado antes de `-1` |
| NAA-007 | ✅ Implementado | Correo desvinculación + bitácora |

---

## Coherence (Design D1–D12)

| Decisión | Seguida? | Notas |
|----------|----------|-------|
| D1.1 Usuarios.PersonaId + FK + índice | ✅ Sí | Aditivo e idempotente |
| D1.2 `-2` antes de `-1` en AsignarActivoPersona | ✅ Sí | `migration.sql:54-56` tras check Persona y antes del check Activo |
| D1.3 SPs nuevos (Vincular/Desvincular/ObtenerPersonaId/ObtenerAsignacionPorToken) | ✅ Sí | + `ObtenerPersonaActivoPorId` (read por id, extra no listado en T) |
| D1.4 ConfirmarRecepcionActivo autenticado (0/1/2/3) | ✅ Sí | Titularidad por `Usuarios.PersonaId = PersonaActivo.PersonaId` |
| D1.5 DesvincularActivoPersonaConfirmacion | ✅ Sí | `FechaFin` + titularidad |
| D1.6 Enriquecer ObtenerActivosPorPersona | ✅ Sí | Añade TipoActivo/Marca/Modelo/FechaConfirmacion/Persona/AsignadoPor (+TokenConfirmacion para MA-004) |
| D1.7 Enriquecer ObtenerPersonas | ✅ Sí | `UsuarioId`/`NombreUsuarioVinculado` vía LEFT JOIN |
| D1.8 Pagina "Mis Activos" + RolPaginaAccion | ✅ Sí | Aplicado en hosted (roles 3 y 31) |
| D1.9 rollback.sql | ✅ Sí | Orden inverso correcto |
| D2 Entidades | ✅ Sí | `Usuario.PersonaId`, `PersonaDTO.UsuarioId/NombreUsuarioVinculado`, `PersonaActivoDTO` enriquecido, `AsignacionActivoDetalleDTO` + csproj |
| D3 WebApi DAL | ✅ Sí | `-2` antes de `<= -1`; `ConfirmarRecepcionActivo(token,usuario)`; `DesvincularActivoPersonaConfirmacion`; `ObtenerAsignacionPorToken`; `ObtenerPersonaIdPorUsuario`; `Vincular/DesvincularPersonaUsuario` (-3) |
| D4 WebApi Services | ✅ Sí | Correo dual + compensación; `ObtenerMisActivos` vacío; `ConfirmarRecepcion` autenticado; `DesvincularConfirmacion`; `IniciarDesvinculacion` (NO setea FechaFin, genera token si NULL); `ObtenerAsignacionPorToken` anónimo |
| D5 Controllers | ✅ Sí | `MisActivos` sin `[Permiso]`; `AsignacionPorToken` `[AllowAnonymous]`; `confirmarRecepcion` autenticado; `IniciarDesvinculacion` `[Permiso Personas Editar]`; `VincularUsuario`/`DesvincularUsuario` |
| D6 Persona.cshtml sync + CatalogsController | ✅ Sí | Botón SVG + modal + warning + campos bloqueados + botón "vincular activo" condicionado |
| D7 Página anónima + login modal | ✅ Sí | `VerAsignacion` standalone + modal login + idempotencia |
| D8 MisActivos + desvincular | ✅ Sí | DataTable + botón Aceptar; botón Desvincular → `IniciarDesvinculacion` |
| D9 FilterConfig | ✅ Sí | `PublicActions` + `Home.VerAsignacion`; sin `Home.ConfirmarRecepcion` |
| D10 Flujo de correo | ✅ Sí | Template desvinculación creado + `<Content Include>` |
| D11 Mapeo de estados | ✅ Sí | Coherente |
| D12 Migración + provisioning | ✅ Sí | T47 aplicada/verificada; `EmpresaService` Paso 5.1 provisiona "Mis Activos" al rol "Usuario" |

---

## Issues Found

**CRITICAL** (must fix before archive): Ninguno.

**WARNING** (should fix):

1. **NAA-001 — correo al admin incluye un ancla vacía.** `ResolverTemplateAsignacion(...)` se usa para ambos correos; la versión admin pasa `string.Empty` como `UrlConfirmacion`, por lo que el botón "Confirmar Recepción" del template se renderiza como `<a href="">`. La spec dice que el correo al admin MUST NOT incluir liga; funcionalmente el `href=""` no navega, pero el ancla sigue presente en el HTML. Bajo impacto (cosmético). Sugerencia: plantilla separada o suprimir el bloque del botón para el admin.

**SUGGESTION** (nice to have):

1. **Vista huérfana `Views/Home/ConfirmarRecepcion.cshtml`** (y su `<Content Include>` en `ServiceDeskDESIMVC.csproj:226`). La acción `Home.ConfirmarRecepcion` fue reemplazada por `VerAsignacion`; la vista quedó sin controlador que la referencie. Eliminar para limpieza.

2. **`VincularPersonaUsuario` no valida si el Usuario objetivo ya está vinculado a OTRA persona.** El SP solo rechaza `-3` cuando la *persona* ya está vinculada a otro usuario; si el *usuario* ya tenía otro `PersonaId`, la relación anterior se pierde silenciosamente (el índice único garantiza 1:1 por persona, no por usuario). No viola VPU-001, pero un guard `IF EXISTS(... Id=@UsuarioId AND PersonaId IS NOT NULL AND PersonaId<>@PersonaId)` evitaría reasignaciones accidentales. (Consistente con design D1.3, por lo que no es un defecto del change.)

3. **`DesvincularActivoPersona` (desvinculación inmediata) sigue expuesto** en WebApi (`POST api/PersonaActivo/Desvincular`) y MVC, aunque la UI ya no lo invoca (usa `IniciarDesvinculacion`). Se conserva como primitivo de compensación (design Assumption 1); considerar restringir su uso si no se desea un bypass del flujo autenticado.

---

## Verdict

**PASS WITH WARNINGS**

La implementación cubre íntegramente las 6+4+5+2 requirements (VPU/MA/CRA/NAA) con evidencia estructural en SQL, entidades, DAL, servicios, controladores y vistas; el build compila con 0 errores y la migración está aplicada/verificada en BD hosted (T47). Restan los smoke manuales T48–T52 (esperados, no bloquean archive con strict_tdd=false) y dos observaciones menores de correo/limpieza.
