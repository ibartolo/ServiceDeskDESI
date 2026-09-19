# Proposal: Aislamiento entre Tenants (Contención)

- **Change**: `tenant-isolation`
- **Fase**: propose
- **Fecha**: 2026-08-18
- **Origen**: `security-remediation` Fase 1 — hallazgo CRÍTICO/URGENTE "Fuga de datos entre tenants" (refs **D1, D2, D15, W6, M6, M11**)

## Intent

Cerrar la fuga de datos entre empresas en su alcance de **contención** (Fase 1): corregir los stored procedures de lectura sin filtro, cerrar el IDOR en `EliminarTicket`, eliminar los endpoints que exponen el directorio de empresas, y consolidar la resolución de tenant desde la identidad autenticada (claim `empresaId`) — sin la migración estructural de `EmpresaId` en todas las tablas, que queda para Fase 2.

## Motivación

Varios procedures devuelven datos de todas las empresas sin validar la identidad (`ObtenerUsuarioPagina*`, `ObtenerEmpresas`, `ObtenerEmpresaPorRFC`, `ObtenerUsuarioPorNombreUsuario`), `EliminarTicket` borra cualquier ticket por `Id` sin validar propiedad, y `GET api/Empresas/List` expone el directorio completo de empresas (RFC, correo, responsable). El aislamiento hoy depende de "buena fe": del `@Usuario` pasado por el cliente y de un `NombreUsuario` no único.

## Qué ya quedó cerrado en `autorizacion-e2e`

- Los controllers WebApi ya resuelven `usuario` desde `User.Identity.Name` (token), no del request → el spoofeo a nivel de request está cerrado.
- Los `[AllowAnonymous]` sueltos (`Empresas/List`, `Relacion/List`, `UsuarioPagina/List`) ya fueron retirados; todos los controllers son `[Authorize]`.

## Scope

### In Scope
1. **SPs de lectura sin filtro**: añadir filtro multi-tenant (`@Usuario`) a `ObtenerUsuarioPagina`, `ObtenerUsuarioPaginaPorId` y `ObtenerUsuarioPorNombreUsuario`.
2. **IDOR**: `EliminarTicket` valida propiedad/empresa (patrón de `EliminarTipoActivo`).
3. **Directorio de empresas**: eliminar endpoints `GET api/Empresas/List` y `POST api/Empresas/RFC`. El dedupe del registro pasa a SPs puntuales server-side.
4. **Claim de tenant**: emitir `empresaId` en el token OAuth (para que Fase 2 lo consuma sin re-emitir tokens).
5. **Hilar `@Usuario`** en `DbWrapper`/Services/Controllers afectados.

### Out of Scope (Fase 2 o cambios dedicados)
- `EmpresaId` (FK) en todas las tablas de dominio y `NombreUsuario` único global (D1 estructural).
- Exposición de `Contrasena` en respuestas (W4/D3/E1 → cambio dedicado de contraseñas/hashing).
- `ObtenerModelos` y `ObtenerEmpresasPorPeriodoPrueba` (SPs sin filtro que hoy **no están expuestos por ningún endpoint**: `ObtenerModelos` es código muerto — el WebApi usa `ObtenerModelo` filtrado — y `ObtenerEmpresasPorPeriodoPrueba` no tiene consumidor).
- `CambiarEstatusTicket`: ya queda cubierto porque `GuardarOActualizarTicket` valida propiedad del ticket (SP líneas 3122-3133).
- `ObtenerUsuarioPorCorreo`: lookup cross-tenant por correo, necesario para el flujo anónimo de recuperación de contraseña; no expuesto como endpoint.

## Approach

Contención de bajo riesgo, sin migración de esquema. Cada quick-win es un commit aislado. El tenant se sigue resolviendo por `@Usuario` (ya derivado del token) en los SPs existentes; el claim `empresaId` se emite como contrato futuro y para defense-in-depth.

## Success Criteria

- [ ] Ningún endpoint expone datos de más de una empresa (prueba de cruce de tenant).
- [ ] `EliminarTicket` no elimina un ticket de otra empresa.
- [ ] `GET api/Empresas/List` y `POST api/Empresas/RFC` ya no existen.
- [ ] El token OAuth incluye el claim `empresaId`.
- [ ] El registro de empresa sigue validando unicidad (RFC/correo/nombre/razón) server-side.
- [ ] `ServiceDeskDESI.sln` compila sin errores.

## Decisiones cerradas (con el usuario)

1. **Resolución de tenant**: contención + claim `empresaId` (sin refactor masivo a `@EmpresaId`).
2. **Directorio**: eliminar `GET api/Empresas/List` (el MVC ya no lo consume).
3. **`Contrasena` en SPs**: se trata en el cambio dedicado de contraseñas, no aquí.
