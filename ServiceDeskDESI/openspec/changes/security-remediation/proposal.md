# Proposal: Remediación de Seguridad, Multi-tenancy y Contraseñas

- **Change**: `security-remediation`
- **Fase**: propose
- **Fecha**: 2026-08-17
- **Origen**: consolidación de `webapi-review`, `database-review`, `mvc-review`, `entities-review`

## Tabla resumen — hallazgos → severidad → fase

> Referencias: **W**=webapi-review, **D**=database-review, **M**=mvc-review, **E**=entities-review (número de hallazgo).

| Estado | Hallazgo consolidado (referencias) | Severidad | Fase |
|---|---|---|---|
| Pendiente | Secretos en git: connection string + password SMTP en `Web.config` (W1) | CRÍTICO/URGENTE | 1 |
| Hecho — `autorizacion-e2e` | Sin autorización real de extremo a extremo: `[AllowAnonymous]` de clase, claim `role="user"` hardcodeado, `AllowInsecureHttp`, `ValidateClientAuthentication` ciego, CORS `*`, sin `[Authorize(Roles)]`, permisos solo cosméticos (W2, W3, W5, M2) | CRÍTICO/URGENTE | 1 |
| Hecho — PBKDF2, registro con contraseña aleatoria, hash en escrituras (admin/cambio/reset), sin `Contrasena` en respuestas ni HTML, cambio de contraseña unificado | Contraseñas reversibles (Rijndael `P@@Sw0rd`) + default `Admin123!` + `Contrasena` devuelta en respuestas y renderizada en HTML (W4, D3, M4, E1) | CRÍTICO/URGENTE | 1 |
| Hecho — `tenant-isolation` (contención) | Fuga de datos entre tenants: SPs sin filtro, IDOR (`EliminarTicket`, `CambiarEstatusTicket`), endpoints anónimos, `@Usuario` spoofeable, directorio de empresas expuesto (D1, D2, D15, W6, M6, M11) | CRÍTICO/URGENTE | 1 |
| Hecho — validación de vigencia del trial en `AutenticarUsuario` | Trial sin enforcement: `AutenticarUsuario` no valida vigencia (D5) | CRÍTICO/URGENTE | 1 |
| Hecho — `debug=false` + `customErrors RemoteOnly` + `RequestAsync` sin NRE | Info disclosure: `debug=true`, `customErrors mode=Off`, NRE en cadena con stack trace (M7) | CRÍTICO/URGENTE | 1 |
| Hecho — `sesion-expiracion` | Sesión/expiración no forzada: `UserController` sin `[Autenticated]`, `BaseController` muerto, `PermissionsController` roto por DI (M1, M3, M8) | CRÍTICO/URGENTE | 1 |
| Parcial — esquema (`EmpresaId`+FK+unique+backfill) y registro listos; reescritura de `GuardarOActualizar*`/`Eliminar*`/`Obtener*` pendiente | Tenant estructural: sin `EmpresaId` en tablas de dominio, `NombreUsuario` no único, tenant vía `CreadoPor` (string) (D1) | ALTO | 2 |
| Hecho — transacción (capa app) + `PlantillaRol` | Registro de empresa sin transacción (8+ SPs sueltos) + provisioning sin template (D4) | ALTO | 2 |
| Hecho — `bugs-bd` | Bugs de BD que rompen flujos: rol sin `PuedeAtenderTickets`, typo `nvarchaR`, `@@IDENTITY`, JOIN muerto en `ObtenerEmpresas`, `Estatus` comentado (D6, D10, D11, D12, D13) | ALTO | 2 |
| Parcial — `RolPaginaAccion` autoritativo; deprecar `UsuarioPagina` pendiente | Dos sistemas de permisos en conflicto: `RolPaginaAccion` vs `UsuarioPagina` (D7) | ALTO | 2 |
| Parcial — E3+W10 listos; E2 (FKs→`*Id`) pendiente | Mapeo por reflection frágil + FKs como navegación vs `*Id` + `TicketEstatus.Id` int/long (W10, E2, E3) | ALTO | 2 |
| Pendiente | Contrato de respuesta sin tipar: `ModelResponse.Response object`, `IsSuccess=true` por defecto (E8) | ALTO | 2 |
| Pendiente | CSRF / verbos HTTP ausentes / `[FromBody]` (WebApi) dentro de MVC (M13, M14) | ALTO | 2 |
| Pendiente | FKs sin entidad (`RolPaginaAccion`, `UsuarioRol`, `TokenRecuperacion`) + `Compania` vs `Empresa` (E4, E10, D17) | MEDIO | 3 |
| Parcial — `Estatus` borrados listo; nullabilidad (E5) y `throw ex` (W9) pendientes | Robustez de datos: NULLabilidad no reflejada en POCOs, `throw ex` resetea stack, `ObtenerUsuarios` devuelve borrados (E5, W9, D13) | MEDIO | 3 |
| Pendiente | Sin manejo global de excepciones / códigos HTTP; validación manual duplicada sin ModelState (W12, W13, M15, E6) | MEDIO | 3 |
| Pendiente | Sin índices no-cluster ni paginación en listados (D8, W11, D9) | MEDIO | 3 |
| Parcial — dedupe server-side (M6) listo; N+1 (M10) pendiente | Rendimiento: N+1 en `ConsultarUsuariosQuePuedenAtender`, dedupe client-side de RFC/correo (M10, M6) | MEDIO | 3 |
| Pendiente | Higiene de código: dependencias muertas (EF Core), Swagger duplicado, layering MVC→WebApi, código muerto, lógica en controllers (W7, W8, M9, M12, M12bis, W14) | MEDIO | 3 |
| Pendiente | Naming/typos, rutas ES/EN mixtas, magic numbers, dashboard mock, `_Layout` null, seed data no reproducible (W15, W17, M16-M20, D16, D18-D21, E7, E9, E11-E14) | BAJO | 4 |

> **Estado**: **Hecho** = implementado (se indica el cambio SDD). **Parcial** = avance parcial (se indica lo que falta). **Pendiente** = sin iniciar.
>
> Cambios SDD ya cerrados: `autorizacion-e2e`, `tenant-isolation`, `sesion-expiracion`, `bugs-bd`, `mapeo-reflection`.

---

## Intent

Detener las brechas de seguridad críticas de la aplicación y ordenar su remediación por **nivel de severidad**, de modo que cada fase sea un lote independiente y desplegable por sí solo: primero los *quick wins* de seguridad que eliminan el riesgo inmediato, después el refactor estructural de multi-tenancy y, al final, la higiene de código y mantenibilidad.

## Motivación

Las 4 revisiones de exploración convergen en un mismo diagnóstico de fondo: **no existe autorización real de extremo a extremo** (OAuth emite `role="user"` hardcodeado, `ValidateClientAuthentication` valida a ciegas, los permisos `RolPaginaAccion`/`ValidarPermisoUsuario` nunca se fuerzan server-side), **los secretos de producción están commiteados en git**, **las contraseñas usan criptografía reversible con clave fija y se devuelven/renderizan en claro**, y **el aislamiento entre tenants es de "buena fe"** (depende de un `@Usuario` spoofeable y de un `NombreUsuario` no único), con fuga real de datos entre empresas. Además el período de prueba no se hace cumplir y hay bugs concretos de mapeo/deploy que rompen flujos (catálogo de estatus, registro de empresa). Es un sistema que hoy **no puede operar en producción** sin riesgo de compromiso total.

## Scope

### In Scope
- Cierre de todas las brechas de seguridad CRÍTICAS y ALTAS detectadas en las 4 revisiones (secretos, authn/authz, contraseñas, tenant isolation, trial, manejo de errores, CSRF).
- Corrección de bugs funcionales de mapeo y de BD que rompen flujos (`TicketEstatus.Id`, `PuedeAtenderTickets`, `nvarchaR`, registro de empresa transaccional).
- Consolidación de los dos sistemas de permisos en una sola fuente de verdad.
- Higiene, robustez, rendimiento y mantenibilidad (fases 3 y 4).

### Out of Scope (ver Non-goals)
- Nuevas funcionalidades de negocio (planes/suscripciones formales, portal de billing, multi-idioma, etc.).
- Migración a un ORM (EF/Dapper) — se mantiene ADO.NET + stored procedures.
- Migración de plataforma (.NET Framework 4.8 → .NET 8).

## Non-goals

- No se introduce un modelo formal de `Plan`/`Suscripcion` con renovación (solo se hace cumplir el trial existente).
- No se elimina el patrón ADO.NET + stored procedures (solo se corrigen los procedimientos inseguros/buggy).
- No se migra `CreadoPor` (string) a `Usuarios.Id` en esta iteración a menos que se decida en "Decisiones abiertas" (se mitiga el riesgo de colisión haciendo `NombreUsuario` único global).
- No se reescriben las vistas/UI; solo se corrigen las decisiones acopladas a permisos y la exposición de contraseñas.

## Approach

El trabajo se divide en **4 fases por severidad**, cada una independiente y desplegable. Cada fase lista: objetivo, hallazgos incluidos y proyectos/archivos afectados.

### Fase 1 — CRÍTICO/URGENTE (quick wins de seguridad)

**Objetivo**: eliminar el riesgo inmediato de compromiso sin refactor estructural. Desplegable de inmediato.

Hallazgos incluidos:
- **Secretos (W1)**: sacar connection string y SMTP de `ServiceDeskDESIWebApi/Web.config` → env vars/secret store + transformación en `Web.Release.config`. **Rotar YA** las credenciales de SQL Server y SMTP de Gmail filtradas.
- **Autorización real (W2, W3, W5, M2)**: quitar `[AllowAnonymous]` de clase en `AutenticationController` (dejar anónimos solo `autenticar`/`recuperar`/token); implementar `ValidateClientAuthentication` real (client id/secret) y `AllowInsecureHttp=false`; CORS restrictivo; emitir roles reales desde la identidad y forzarlos con un `AuthorizeAttribute` server-side; en MVC, forzar `PuedeCrear/Editar/Eliminar` en los métodos de escritura (no solo ocultar botones).
- **Contraseñas (W4, D3, M4, E1)**: reemplazar Rijndael por hashing (PBKDF2/bcrypt); dejar de crear `Admin123!` hardcodeado; no devolver `Contrasena` en ninguna respuesta; quitar `Cryptography.Decrypt` de `UserController.Users` (no más contraseña en HTML); unificar las 2 implementaciones divergentes de cambio de contraseña; marcar `Usuario.Contrasena` como no-serializable.
- **Fuga de tenants — contención (D1, D2, D15, W6, M6, M11)**: cerrar los SPs de lectura sin filtro (`ObtenerModelos`, `ObtenerUsuarioPagina*`, `ObtenerUsuarioPorCorreo/NombreUsuario`, `ObtenerEmpresas*`) restringiéndolos a admin/billing con autorización explícita; agregar validación de propiedad/empresa a `EliminarTicket` y `CambiarEstatusTicket` (IDOR); quitar `[AllowAnonymous]` de `EmpresaController.List`/`RelacionController.List`/`UsuarioPaginaController.List`; **resolver el tenant server-side desde el claim OAuth** en lugar de confiar en el parámetro `@Usuario` del request; mover la deduplicación de RFC/correo al servidor.
- **Trial (D5)**: validar `FechaVigenciaFin >= GETDATE()` y `EsPeriodoPrueba` en `AutenticarUsuario`.
- **Info disclosure (M7)**: `debug=false` + `customErrors mode="RemoteOnly"`; que `RequestAsync` no desreferencie `result` null (devolver `ModelResponse` de error en vez de NRE).
- **Sesión (M1, M3, M8)**: `[Autenticated]` en `UserController` y acciones sueltas de `HomeController`; arreglar `BaseController` para que la expiración realmente redirija; reparar `PermissionsController` (quitar inyección sin contenedor).

**Proyectos/archivos**: `ServiceDeskDESIWebApi` (`Web.config`, `Startup.cs`, `Controllers/AutenticationController.cs`, `Controllers/EmpresaController.cs`, `Helpers/Cryptography.cs`, `DAL/DbWrapper.*.cs`); `ServiceDeskDESIMVC` (`Controllers/UserController.cs`, `Controllers/HomeController.cs`, `Controllers/BaseController.cs`, `Controllers/PermissionsController.cs`, `Helpers/Cryptography.cs`, `DAL/HttpClientBase.cs`, `Web.config`); `ServiceDeskDESIEntities` (`Usuario.cs`); script BD (`AutenticarUsuario`, SPs de lectura sin filtro).

### Fase 2 — ALTO (refactor estructural de multi-tenancy y RBAC)

**Objetivo**: eliminar estructuralmente la fuga entre tenants y consolidar el modelo de seguridad, con la contención de Fase 1 ya en producción.

Hallazgos incluidos:
- **Tenant de primera clase (D1)**: añadir `EmpresaId` (FK NOT NULL) a las tablas de dominio (`Ticket`, `Area`, `Sucursal`, `Activo`, `Categoria`, `Rol`, etc.); hacer `NombreUsuario` único global; eliminar el patrón de inferencia por `CreadoPor` en los `Obtener*`.
- **Provisioning transaccional (D4)**: consolidar el registro de empresa (8+ SPs) en un único procedure `RegistrarEmpresa` con `BEGIN TRY/TRAN/COMMIT/ROLLBACK`, clonando un template (`PlantillaRol`/`PlantillaRolPagina`).
- **Bugs de BD (D6, D10, D11, D12, D13)**: setear `PuedeAtenderTickets` en `GuardarRolParaNuevaEmpresa`; corregir `nvarchaR` + `@@IDENTITY`→`SCOPE_IDENTITY()`; eliminar JOIN muerto de `ObtenerEmpresas`; corregir validación de `AsignarRolUsuario`; restaurar filtro `Estatus=1`.
- **RBAC único (D7)**: definir una sola fuente de verdad (recomendado `RolPaginaAccion`) y deprecar `UsuarioPagina` legacy.
- **Mapeo y entidades (W10, E2, E3)**: decidir FKs escalares `*Id` (recomendado para ADO.NET+reflection) vs navegación; corregir `TicketEstatus.Id` (int↔long) que hoy rompe el catálogo de estatus en runtime; eliminar la duplicación masiva de mapeo.
- **Contrato tipado (E8)**: `ModelResponse<T>` + `IsSuccess=false` por defecto.
- **CSRF/verbos (M13, M14)**: `[HttpPost]` + `[ValidateAntiForgeryToken]`/`@Html.AntiForgeryToken` en escrituras; eliminar `[FromBody]` (WebApi) de los controllers MVC.

**Proyectos/archivos**: script BD (migración de esquema + SPs); `ServiceDeskDESIEntities` (todos los POCOs de dominio); `ServiceDeskDESIWebApi` (`DAL/DbWrapper.cs` y partials, `Services/*`); `ServiceDeskDESIMVC` (`Controllers/*`, vistas de formularios).

### Fase 3 — MEDIO (robustez, rendimiento, higiene)

**Objetivo**: robustecer el manejo de errores y validación, y mejorar rendimiento/mantenibilidad sin cambios de comportamiento.

- Manejador global de excepciones + códigos HTTP correctos (W12).
- Validación con `DataAnnotations`/`ModelState` en entidades y services (W13, M15, E6); reflejar NULLabilidad real (`bool? Estatus`, `DateTime? FechaCreacion`, etc.) (E5).
- `throw ex;` → `throw;` para no resetear stack traces (W9).
- Índices no-cluster en columnas calientes (`CreadoPor`, `EmpresaId`, FKs) (D8); paginación en listados (W11, D9).
- Eliminar N+1 de `ConsultarUsuariosQuePuedenAtender` (M10); dedupe de empresa server-side (M6).
- Quitar dependencias muertas (EF Core, `Microsoft.Extensions.*`), eliminar Swagger duplicado y el `ProjectReference` MVC→WebApi (W7, W8).
- Crear entidades para `RolPaginaAccion`, `UsuarioRol`, `TokenRecuperacion`; resolver `Compania` vs `Empresa` (E4, E10).
- Eliminar código muerto: `josepruebaController`, `ServiceDeskIMVC`, `GuardarNuevaEmpresaCompleta`, partials sin uso (M12, M12bis, W14).

### Fase 4 — BAJO (limpieza y deuda menor)

**Objetivo**: naming, consistencia y mantenibilidad, sin riesgo funcional.

- Typos y naming (`Autenticated`, `EixstSession`, `nvarchaR` heredado, `UsuarioPagina.Usuarios`, etc.) (W15, M16, D19, E12, E13).
- Rutas ES/EN mixtas y poco descriptivas (W17, M17).
- Enums para `Urgencia` y `Tipo` de página; eliminar magic numbers (E9, D18).
- Dashboard mock (`Views/Home/Index.cshtml`) y JS muerto (M18); null-guard en `_Layout.cshtml` (M19).
- Reorganización de namespaces y atributos de serialización explícitos (E7, E11); formato/espaciado (E14).
- Seed data reproducible para catálogos base (D16); constraint único en `CategoriaResponsable.EsPrincipal` (D20); consistencia `Estatus NULL/NOT NULL` (D21).

---

## Capabilities

> Contrato con sdd-spec. `openspec/specs/` está vacío → todas son capacidades nuevas.

### New Capabilities
- `autenticacion`: emisión y validación de tokens OAuth, clientes registrados, recuperación de contraseña y bloqueo de trials vencidos.
- `autorizacion`: roles y permisos (`RolPaginaAccion`) forzados server-side en WebApi y MVC, con una única fuente de verdad.
- `contrasenas`: hashing (PBKDF2/bcrypt), políticas de longitud y no-exposición del secreto en respuestas/UI.
- `multi-tenancy`: aislamiento de datos por `EmpresaId` resuelto desde la identidad autenticada, con `NombreUsuario` único global.
- `trial`: enforcement del período de prueba (vigencia validada en login).
- `manejo-errores`: manejo global de excepciones, códigos HTTP correctos y `ModelResponse<T>` tipado.

### Modified Capabilities
- None (no hay specs existentes).

## Affected Areas

| Área | Impacto | Descripción |
|---|---|---|
| `ServiceDeskDESIWebApi/Web.config` + `Web.Release.config` | Modificado | Extracción de secretos |
| `ServiceDeskDESIWebApi/Startup.cs` | Modificado | OAuth, CORS, HTTPS, rate limiting |
| `ServiceDeskDESIWebApi/Controllers/*` | Modificado | Atributos de autorización |
| `ServiceDeskDESIWebApi/Helpers/Cryptography.cs` | Reemplazado | Hashing en vez de Rijndael |
| `ServiceDeskDESIWebApi/DAL/*` | Modificado | Filtros de tenant, mapeo, SPs |
| `ServiceDeskDESIMVC/Controllers/*` + `Helpers/*` + `DAL/HttpClientBase.cs` | Modificado | Authz server-side, CSRF, manejo de errores |
| `ServiceDeskDESIEntities/*` | Modificado | `Contrasena` no serializable, FKs escalares, `ModelResponse<T>` |
| Script BD (`openspec/basededatosservicedesk.txt`) | Modificado | `EmpresaId` en tablas, SPs corregidos, `RegistrarEmpresa`, índices |

## Riesgos

| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Rotación de credenciales rompe despliegues conectados a la BD/SMTP actuales | Alta | Coordinar rotación con un cutoff; mantener `Web.Release.config` con placeholders y documentar el proceso |
| Migrar contraseñas de ciphertext a hash deja a usuarios existentes sin poder iniciar sesión | Alta | Fase de transición: verificar contra hash, si falla verificar contra ciphertext legacy y re-hashear en caliente |
| Añadir `EmpresaId` a todas las tablas y migrar `CreadoPor`→`Id` es una migración de datos amplia con riesgo de fuga/backend incompleto | Alta | Hacerla en Fase 2 (con Fase 1 ya desplegada); migración idempotente con backfill por lotes y validación de filas huérfanas |
| Endurecer autorización puede dejar a usuarios legítimos sin acceso (regresión funcional) | Media | Matrix de roles/páginas como checklist de verificación antes de desplegar; feature flag si es viable |
| Los 107 SPs no tienen tests; corregir varios a la vez puede introducir regresiones | Media | Fases pequeñas e independientes; smoke tests manuales por endpoint tras cada fase |

## Rollback Plan

- **Fase 1**: es mayormente config/atributos/criptografía. Revertir por commit (cada quick win es un commit aislado). Las credenciales rotadas no se revierten (se re-emiten). La transición de contraseñas mantiene el verificador legacy, así que volver atrás solo requiere desactivar el hash-check.
- **Fase 2**: la migración de esquema (`EmpresaId`, `NombreUsuario` único) debe ir acompañada de un script de rollback de esquema (`ALTER ... DROP COLUMN`) y un backup previo de BD. Los SPs se reemplazan manteniendo el nombre/compatibilidad donde sea posible; rollback = restaurar versión anterior del script.
- **Fases 3/4**: sin cambios de esquema destructivos; rollback por commit.

## Dependencies

- Acceso a credenciales rotadas de SQL Server y SMTP (decisión del operador).
- Decisión sobre estrategia de hashing (PBKDF2 vs bcrypt) y política de longitud mínima (ver "Decisiones abiertas").
- Confirmación de la fuente de verdad de permisos (`RolPaginaAccion` vs `UsuarioPagina`).

## Success Criteria

- [ ] Ningún secreto en git; credenciales filtradas rotadas y validadas.
- [ ] Ningún endpoint mutante es invocable sin token válido y con permiso server-side (auditado por endpoint).
- [ ] Ninguna respuesta serializa `Contrasena` (ni en WebApi ni en HTML del MVC).
- [ ] Un usuario de la empresa A no puede leer/escribir datos de la empresa B (pruebas de cruce de tenant).
- [ ] Un trial vencido no puede autenticar ni operar.
- [ ] `debug=false` y `customErrors=RemoteOnly` en release; sin stack traces expuestos.
- [ ] El catálogo de estatus de ticket y el registro de empresa funcionan sin errores.
- [ ] Las fases 1 y 2 pasan un smoke test manual por endpoint antes de desplegarse.

## Decisiones abiertas

1. **Estrategia de hashing**: PBKDF2 (nativo en .NET Framework 4.8, sin dependencias) vs bcrypt (librería externa). Recomendado: PBKDF2 con salt único por usuario y factor de trabajo configurable.
2. **Longitud/política mínima de contraseña**: hoy es 6; se propone 12+ con reglas de complejidad. Validar con el negocio.
3. **Migración de `CreadoPor` (string) a `Usuarios.Id`**: en esta iteración se propone solo hacer `NombreUsuario` único global (mitigación). La migración completa del campo de auditoría queda para una iteración futura.
4. **Fuente de verdad de permisos**: se propone `RolPaginaAccion` y deprecar `UsuarioPagina`, pero hay que confirmar si el flujo legacy de `UsuarioPagina` se usa en clientes reales.
5. **`Compania`**: confirmar si es un residuo eliminable o si tiene propósito real (¿legal/contable?).
6. **Rate limiting en `/token`** (W5/next-steps): alcance y herramienta (middleware propio vs librería) — se deja como ítem opcional de Fase 1.
7. **Alcance de "autorización server-side" en MVC**: se recomienda un filtro global con allowlist anónima en vez de atributos por-action; confirmar si se acepta ese refactor en Fase 1 o se prefiere atributo por acción.
