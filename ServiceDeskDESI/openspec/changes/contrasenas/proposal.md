# Proposal: Hashing de contraseñas (W4, D3, M4, E1)

- **Change**: `contrasenas`
- **Fecha**: 2026-08-18
- **Origen**: `security-remediation` Fase 1 — "Contraseñas reversibles" (refs **W4, D3, M4, E1**)

## Intent

Reemplazar el cifrado reversible (Rijndael con clave fija) por hashing PBKDF2, eliminar la contraseña hardcodeada `Admin123!`, y dejar de exponer `Contrasena` en respuestas.

## Hecho
- **PBKDF2** en `ServiceDeskDESIWebApi/Helpers/Cryptography.cs`: `HashPassword`, `VerifyPassword` (con fallback a ciphertext legacy Rijndael) y `GeneratePassword`.
- **Registro de empresa**: el admin nuevo recibe una contraseña aleatoria (16 chars), se almacena **hasheada** y se envía en claro por correo de bienvenida (antes: `Admin123!` + Rijndael).
- **Login**: el MVC envía el password en texto plano (antes Rijndael); el WebApi verifica PBKDF2. El SP `AutenticarUsuario` ya no compara contraseña en SQL.
- **No exposición**: `Contrasena` se anula en `ObtenerUsuarios`, `ObtenerUsuarioPorId`, `ObtenerUsuarioPorCorreo`, `ObtenerUsuarioPorNombreUsuario` y `AutenticarUsuario`.

## Pendiente (cambio de seguimiento)
- `GuardarOActualizarUsuarioAdmin`, `ActualizarContrasena` y `RestablecerContrasena` siguen guardando ciphertext (funcionan vía fallback, pero no hashean).
- MVC `UserController.Users` desencripta y renderiza la contraseña en HTML.
- Unificar las 2 implementaciones divergentes de cambio de contraseña.

## Archivos
- `ServiceDeskDESIWebApi/Helpers/Cryptography.cs`
- `ServiceDeskDESIWebApi/Services/EmpresaService.cs`
- `ServiceDeskDESIWebApi/DAL/DbWrapper.Autenticacion.cs`
- `ServiceDeskDESIMVC/Controllers/HomeController.cs`
- `openspec/basededatosservicedesk.txt` (SP AutenticarUsuario)
- `openspec/changes/contrasenas/migration.sql`
