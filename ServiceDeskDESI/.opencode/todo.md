# Mission: Refrescar la cookie de sesión (TokenCookie) en UserController.ActualizarPerfilUsuario para que el navbar (_Layout.cshtml user-avatar) muestre la nueva imagen de perfil de inmediato

## Context
- Proyecto: solución legacy .NET Framework MVC (`ServiceDeskDESIMVC`) + WebApi + Entities.
- Verificación del proyecto: compilación MSBuild (VS 2022). **No existe proyecto de pruebas** en la solución.
- Trabajo de Worker (ses_1) YA completado y verificado por build — evidencias en `.opencode/work-log.md` y `.opencode/unit-tests/2026-08-30T21-28-45-UserController-ActualizarPerfilUsuario.md`.
- VERIFICADO por Reviewer (PASS): build MSBuild Debug/Rebuild 0 errores, sesión refrescada con ProfileImage. Todas las tareas completadas.

## File Manifest
| Action | File Path | Description | Dependencies |
|--------|-----------|-------------|--------------|
| MODIFY | ServiceDeskDESIMVC/Controllers/UserController.cs | Refrescar TokenCookie con ProfileImage tras ActualizarPerfilUsuario | - |

## M1: Refrescar sesión con nueva imagen de perfil en ActualizarPerfilUsuario | status: completed
### T1.1: Implementar refresco de TokenCookie en UserController.cs | agent:Worker | status: completed
- [x] S1.1.1: Aplicar reemplazo exacto del bloque en ActualizarPerfilUsuario (refresh sesión con ProfileImage) | file:ServiceDeskDESIMVC/Controllers/UserController.cs | size:S
- [x] S1.1.2: Verificar usings existentes (Newtonsoft.Json, ServiceDeskDESIMVC.Helpers) | file:ServiceDeskDESIMVC/Controllers/UserController.cs | size:S
- [x] S1.1.3: Compilar ServiceDeskDESIMVC con VS MSBuild (Debug) — sin errores nuevos | size:S

### T1.2: Revisión y verificación final | agent:Reviewer | depends:T1.1 | status: completed
- [x] S1.2.1: Verificar edición exacta en UserController.cs (solo el bloque indicado, nada más modificado) | file:ServiceDeskDESIMVC/Controllers/UserController.cs | size:S
- [x] S1.2.2: Re-ejecutar build MSBuild Debug y confirmar 0 errores | size:S
- [x] S1.2.3: Confirmar sincronización (work-log/unit-tests/contexto) y marcar T1.2 y M1 completos | size:S
