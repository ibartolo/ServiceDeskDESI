# Proposal: Trial enforcement + Info disclosure (D5, M7)

- **Change**: `trial-info-disclosure`
- **Fecha**: 2026-08-18
- **Origen**: `security-remediation` Fase 1 — hallazgos CRÍTICOS/URGENTE (refs **D5, M7**)

## Intent

Cerrar los dos últimos hallazgos CRÍTICOS: hacer cumplir la vigencia del período de prueba en el login, y eliminar la exposición de información (debug + customErrors + stack traces).

## Hecho

- **D5 — Trial enforcement**: `DbWrapper.AutenticarUsuario` ahora bloquea el login si `Empresa.EsPeriodoPrueba = true` y `Empresa.FechaVigenciaFin < DateTime.Now`, devolviendo un mensaje específico ("El periodo de prueba ... ha expirado"). Aplica a `/token` y a `api/Autentication/autenticar`.
- **M7 — Info disclosure**:
  - `customErrors mode="RemoteOnly"` en `ServiceDeskDESIMVC/Web.config` (antes `Off`).
  - `debug="false"` en `ServiceDeskDESIMVC/Web.config` y `ServiceDeskDESIWebApi/Web.config` (antes `true`).
  - El NRE en cadena de `RequestAsync` ya se había corregido (no devuelve null).

## Archivos
- `ServiceDeskDESIWebApi/DAL/DbWrapper.Autenticacion.cs`
- `ServiceDeskDESIMVC/Web.config`
- `ServiceDeskDESIWebApi/Web.config`

## Nota
- `debug="false"` afecta el debugging local (sin edit-and-continue). Para depurar temporalmente se puede volver a `true` en dev.
- El HTML 500 con stack trace del middleware OWIN del WebApi corresponde a W12 (manejador global de excepciones, Fase 3), no a M7.
