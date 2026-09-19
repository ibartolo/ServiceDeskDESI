# Design: Aislamiento entre Tenants (Contención)

## Technical Approach

Corregir en la capa de datos (SPs) y en el contrato OAuth, sin tocar el esquema. El patrón de filtro multi-tenant existente (visto en `ObtenerMarca`, `ObtenerModelo`, `EliminarTipoActivo`) se replica en los SPs que hoy carecen de él. Los endpoints que exponen el directorio de empresas se eliminan; el dedupe del registro se resuelve con SPs puntuales server-side.

## Architecture Decisions

### Decision: Filtro de tenant por `@Usuario` (patrón existente)
**Choice**: añadir el parámetro `@Usuario NVARCHAR(25)` y el filtro `INNER JOIN Usuarios u ON <tabla>.CreadoPor = u.NombreUsuario` + `u.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)` a los SPs de lectura sin filtro. Para `UsuarioPagina`, la pertenencia se resuelve vía `UsuarioPagina.UsuarioId → Usuarios.Id → EmpresaId`.
**Alternatives considered**: migrar a `@EmpresaId` (Fase 2, requiere refactor de todos los SPs y servicios); resolver por `usuarioId` (más robusto, pero cambia firmas en todos lados).
**Rationale**: es el patrón ya usado en 80+ SPs; el `@Usuario` ya viene del token (`User.Identity.Name`), no del request.

### Decision: Emitir claim `empresaId` en el token
**Choice**: `GrantResourceOwnerCredentials` añade `identity.AddClaim(new Claim("empresaId", usuario.Empresa.Id.ToString()))` (guardando null). El MVC ya guarda `EmpresaID` en el `TokenCookie`.
**Rationale**: deja el tenant disponible como claim de identidad para Fase 2 (que consumirá `empresaId` en vez de `NombreUsuario`) sin re-emitir tokens.

### Decision: Eliminar endpoints de directorio y dedupe puntual
**Choice**: quitar `GET api/Empresas/List` y `POST api/Empresas/RFC`. Añadir SPs `ObtenerEmpresaPorCorreoContacto`, `ObtenerEmpresaPorNombreComercial`, `ObtenerEmpresaPorRazonSocial` (lookups cross-tenant server-side, no expuestos como endpoint) y usarlos en `EmpresaService.RegistrarEmpresa` en vez de cargar todas las empresas.
**Rationale**: el MVC ya no consume `List`/`RFC` (el registro usa `Registrar`); el dedupe sigue siendo server-side pero sin exponer el directorio ni cargar O(n) en cada alta.

### Decision: IDOR `EliminarTicket` por validación de propiedad
**Choice**: añadir `@Usuario` a `EliminarTicket` y validar existencia/propiedad antes del `UPDATE` (espejo de `EliminarTipoActivo`). Devolver `SELECT 0` si no pertenece a la empresa del usuario.
**Rationale**: cierra la brecha de borrar tickets de otras empresas por `Id`.

## Data Flow

```
/token → GrantResourceOwnerCredentials → AutenticarUsuario → claims [Name, usuarioId, empresaId, roles]
Lectura UsuarioPagina → [Authorize] → UsuarioPaginaController → DbWrapper(usuario=User.Identity.Name) → ObtenerUsuarioPagina(@Usuario) → solo filas de la empresa
EliminarTicket → [Permiso] → DbWrapper(@Usuario) → EliminarTicket valida propiedad → soft delete
Registro → RegistrarEmpresa → dedupe por RFC/correo/nombre/razón (SPs puntuales) → transacción
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `openspec/basededatosservicedesk.txt` | Modify | `ObtenerUsuarioPagina`, `ObtenerUsuarioPaginaPorId`, `ObtenerUsuarioPorNombreUsuario` (+filtro); `EliminarTicket` (+`@Usuario`+propiedad); 3 SPs de dedupe nuevos |
| `ServiceDeskDESIWebApi/App_Start/Startup.cs` | Modify | Emitir claim `empresaId` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.UsuarioPagina.cs` | Modify | `ObtenerUsuarioPagina`/`PorId` reciben `usuario` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Autenticacion.cs` | Modify | `ObtenerUsuarioPorNombreUsuario(nombreUsuario, usuario)` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Empresa.cs` | Modify | Quitar `ObtenerTodasLasEmpresas`; añadir 3 SPs de dedupe |
| `ServiceDeskDESIWebApi/Services/EmpresaService.cs` | Modify | Quitar `ObtenerTodasLasEmpresas`; dedupe puntual en `RegistrarEmpresa` |
| `ServiceDeskDESIWebApi/Services/PermisosService.cs` | Modify | `ObtenerUsuarioPorNombreUsuario(usuario, usuario)` |
| `ServiceDeskDESIWebApi/Services/AutenticacionService.cs` | Modify | `ObtenerUsuarioPorNombreUsuario(nombreUsuario, nombreUsuario)` |
| `ServiceDeskDESIWebApi/Controllers/EmpresaController.cs` | Modify | Quitar `List` y `RFC` |
| `ServiceDeskDESIWebApi/Controllers/UsuarioPaginaController.cs` | Modify | Pasar `User.Identity.Name` |
| `ServiceDeskDESIWebApi/Controllers/RolController.cs` | Modify | `ObtenerUsuarioPorNombreUsuario(usuario, usuario)` |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.Empresa.cs` | Modify | Quitar `ObtenerTodasLasEmpresas` y `ObtenerEmpresasPorRFC` muertos |
| `ServiceDeskDESIMVC/Services/EmpresaService.cs` | Modify | Quitar wrappers muertos |

## Testing Strategy

| Capa | Qué probar | Enfoque |
|------|-----------|---------|
| Smoke WebApi | Token incluye `empresaId`; `Empresas/List` y `Empresas/RFC` → 404; `UsuarioPagina/List` solo devuelve filas de la propia empresa | Swagger/Postman |
| Smoke IDOR | `EliminarTicket` de un ticket de otra empresa → no se elimina | Postman con dos tenants |
| Regresión | Registro de empresa sigue validando unicidad RFC/correo/nombre/razón | Pantalla de alta |

## Migration / Rollout

Sin migración de datos. Los SPs se reemplazan manteniendo nombre (solo se añade el parámetro `@Usuario` en 3 de ellos). Rollback por commit. `EliminarTicket` mantiene su nombre y firma de salida (devuelve `SELECT 0` cuando no hay permiso), coherente con el resto de `Eliminar*`.
