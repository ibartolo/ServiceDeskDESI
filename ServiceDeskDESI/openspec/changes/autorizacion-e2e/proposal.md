# Proposal: Autorización de Extremo a Extremo

- **Change**: `autorizacion-e2e`
- **Fase**: propose
- **Fecha**: 2026-08-18
- **Origen**: hallazgo CRÍTICO/URGENTE de Fase 1 de `security-remediation` (refs W2, W3, W5, M2)
- **Propuesta padre**: `openspec/changes/security-remediation/proposal.md`

## Problema

No existe autorización real de extremo a extremo. El backend emite tokens OAuth con `role="user"` hardcodeado (`Startup.cs:169`), `ValidateClientAuthentication` valida a ciegas (`Startup.cs:146`), `AllowInsecureHttp=true` (`Startup.cs:96`) y CORS `Access-Control-Allow-Origin: *` (`Startup.cs:151`). `AutenticationController` tiene `[AllowAnonymous]` de clase y expone acciones de lectura/escritura de usuarios sin token; además hay `[AllowAnonymous]` suelto en `EmpresaController`, `RelacionController` y `UsuarioPaginaController`. En el WebApi solo hay `[Authorize]` (valida login, no permisos) y `RolPaginaAccion`/`ValidarPermisoUsuario` nunca se fuerzan. En el MVC, `[Autenticated]` solo redirige al login (no valida permisos), `UserController` lo omite, y `PuedeCrear/Editar/Eliminar` solo ocultan botones.

## Decisiones cerradas (con el usuario, 2026-08-18)

1. **Fuente de verdad de permisos**: `RolPaginaAccion` (server-side). `UsuarioPagina` queda SOLO para render de menús dinámicos — no es referencia de seguridad.
2. **Token**: emitir identidad real (`ClaimTypes.Name`, `usuarioId`) + roles reales (leídos de `Rol`) como claims informativos, NO autoritativos. La autorización se resuelve server-side por request. No se definen roles estáticos.
3. **MVC**: filtro global con allowlist (lista blanca) de acciones públicas — "seguro por defecto" (Opción A), no atributo por acción.

## Intent

Cerrar la brecha de autorización de extremo a extremo: tokens con identidad/roles reales, clientes OAuth validados, HTTPS obligatorio, CORS restrictivo, y permisos `RolPaginaAccion` forzados server-side tanto en el WebApi como en el MVC.

## Capabilities

### New Capabilities

- `autenticacion`: emisión y validación de tokens OAuth con identidad y roles reales; validación real de clientes; HTTPS obligatorio; CORS restrictivo.
- `autorizacion`: enforcement server-side de permisos `RolPaginaAccion` en WebApi y MVC; filtro global + allowlist en MVC.

### Modified Capabilities

- None (`openspec/specs/` está vacío).

## Scope

### In Scope

- **WebApi — OAuth/CORS**: `ValidateClientAuthentication` real, `AllowInsecureHttp=false` en release, CORS restrictivo, emisión de claims reales (`Name`, `usuarioId`, roles).
- **WebApi — endpoints anónimos**: quitar `[AllowAnonymous]` de clase en `AutenticationController` (dejar anónimos solo `autenticar`, `ValidarRecetearContrasenia`, `validarToken`, `restablecerContrasenia` y el flujo de registro de empresa pre-login); quitar `[AllowAnonymous]` suelto en `EmpresaController`/`RelacionController`/`UsuarioPaginaController`.
- **WebApi — autorización**: atributo/filtro que fuerza `ValidarPermisoUsuario` (contra `RolPaginaAccion`) en acciones de escritura.
- **MVC — autorización**: filtro global con allowlist + enforcement de permisos en acciones de escritura.

### Out of Scope (otros cambios)

- Hashing de contraseñas (W4/D3/M4/E1), tenant isolation/IDOR/`EmpresaId`, trial, info disclosure, sesión/expiración forzada, rate limiting. Todo eso son otros ítems de Fase 1 o fases posteriores de `security-remediation`.

## Affected Areas

| Área | Impacto |
|---|---|
| `ServiceDeskDESIWebApi/App_Start/Startup.cs` | Modificado — OAuth, CORS, claims |
| `ServiceDeskDESIWebApi/Controllers/AutenticationController.cs` | Modificado — atributos de autorización |
| `ServiceDeskDESIWebApi/Controllers/{Empresa,Relacion,UsuarioPagina}Controller.cs` | Modificado — quitar `[AllowAnonymous]` |
| `ServiceDeskDESIWebApi/` (nuevo atributo/filtro de autorización) | Añadido |
| `ServiceDeskDESIWebApi/Services/PermisosService.cs` | Reutilizado — ya existe `ValidarPermisoUsuario` |
| `ServiceDeskDESIMVC/` (filtro global + allowlist) | Modificado |
| `ServiceDeskDESIMVC/Controllers/UserController.cs` + acciones de escritura | Modificado |

## Riesgos

| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Endurecer autorización deja usuarios legítimos sin acceso | Media | Matrix rol/página como checklist de verificación por endpoint |
| Claims de roles dinámicos desactualizados si cambian tras emitir el token | Baja | Los claims son informativos; la autorización SIEMPRE se resuelve contra BD |
| Endpoints legítimamente anónimos bloqueados por error | Media | Allowlist explícita auditada endpoint por endpoint |

## Success Criteria

- [ ] Ningún endpoint mutante es invocable sin token válido.
- [ ] El token emite identidad y roles reales (no `role="user"` hardcodeado).
- [ ] `ValidarPermisoUsuario` se fuerza server-side en las escrituras (WebApi y MVC).
- [ ] El MVC queda protegido por defecto; las acciones públicas solo vía allowlist.
- [ ] CORS restrictivo y `AllowInsecureHttp=false` en release.
