# Archive Report — configuracion-empresa

- **Change**: `configuracion-empresa`
- **Archived on**: 2026-09-07
- **Archive location**: `openspec/changes/archive/2026-09-07-configuracion-empresa/`
- **Artifact store**: openspec (file-based)
- **Veredicto**: **PASS** (0 issues CRÍTICOS, 0 WARNINGS, 3 sugerencias no bloqueantes)

---

## Resumen del cambio

Nueva página de menú independiente `ConfiguracionEmpresa` ("Configuración de Empresa", ícono `fa-cog`, `OrdenB=9`) donde el usuario autenticado **ve** los datos generales de su empresa (solo lectura) y **edita** únicamente dos criterios: (a) el horario laboral (7 filas Lun–Dom, editor 12h AM/PM, guardado atómico en una única transacción) y (b) el logotipo de la empresa (SVG/PNG, validación server-side, subida/visualización/quitar). Incluye footer estático "by DESi" en el layout y aislamiento multi-tenant por usuario autenticado. No existen operaciones de eliminación.

Implementado sobre el patrón N-capas existente (MVC → HttpClient → WebApi → `DbWrapper` → SPs), replicando precedentes como foto de perfil (subida MVC-side) y `GuardarPermisosRolMasivo` (transacción).

## Sync de specs (delta → main specs)

La capability `configuracion-empresa` era **nueva** (no existía main spec previo en `openspec/specs/`), y su delta es un spec completo (sin anotaciones "(Previously: …)"). Acción: **copia íntegra**.

| Delta (origen) | Main spec (destino) | Acción |
|---|---|---|
| `specs/configuracion-empresa/spec.md` | `openspec/specs/configuracion-empresa/spec.md` | Copiado íntegro (9 reqs CE-001..CE-009) |

## Estado de la migración

**NO aplicada — pendiente de aplicación manual por el usuario.** `migration.sql` es idempotente (guards `sys.columns`/`OBJECT_ID(...) IS NULL`), escrito contra el esquema real hosted. Contiene: `CREATE TABLE EmpresaHorarioLaboral` (+ `UQ_EmpresaHorarioLaboral_EmpresaDia` + FK→`Empresa`), `ALTER TABLE Empresa ADD LogoUrl NVARCHAR(500) NULL`, seed `Pagina`/`RolPaginaAccion` (solo rol Administrador, `PuedeLeer=1,PuedeEditar=1`), backfill idempotente de horario (Lun–Vie 09:00–17:00, Sáb/Dom no laborables) y 3 SPs (`ObtenerHorarioLaboral`, `GuardarHorarioLaboralDia`, `GuardarLogoEmpresa`), todos multi-tenant resolviendo `@EmpresaId` desde `@Usuario`. `rollback.sql` en orden inverso con guards.

> Nota: el seed de `Pagina`/`RolPaginaAccion` asume nombres de columna (`PuedeLeer/PuedeEditar/...`, `OrdenB`, `PermisosPadreId`) que pueden diferir en la BD hosted (drift conocido). Verificar columnas reales antes/después de ejecutar.

## Veredicto de verificación

**PASS** — 34/34 tareas `[x]`, build MSBuild VS2022 Debug con **0 errores** en los 3 proyectos (Entities / MVC / WebApi), 9/9 requisitos (CE-001..CE-009) y 28/28 escenarios cubiertos estáticamente, 7/7 decisiones de diseño seguidas. Registros `.csproj` old-style completos. Sin issues CRÍTICOS ni WARNING.

### Enmiendas registradas (post-apply)

- **CE-004**: editor de hora 12h AM/PM (3 `<select>`: Hora 1–12, Minuto paso 5, AM/PM) en lugar de `datetime-local`; contrato "HH:mm" con backend intacto.
- **CE-006**: acción "Quitar logo" (con confirmación) que borra el archivo físico y persiste `Empresa.LogoUrl = NULL` vía `(object)logoUrl ?? DBNull.Value`, restaurando el fallback DESi.

### Sugerencias (no bloqueantes)

1. **MIME de SVG como `application/octet-stream`**: algunos navegadores envían SVG con MIME genérico; la validación MIME estricta (`ConfiguracionEmpresaController`) puede dar falsos negativos. Si falla en smoke manual, relajar a "extensión permitida + peso" manteniendo `accept=".svg,.png"`.
2. **`MvcBuildViews` desactivado**: la sintaxis Razor de `Index.cshtml` se validó solo estáticamente; recomendado smoke-test de `/ConfiguracionEmpresa` antes de liberar.
3. **Drift de columnas `Pagina`/`RolPaginaAccion`**: la migración ya fue aplicada por el usuario; si el seed no se reflejó, verificar columnas reales en BD hosted.

## Next steps

1. **Aplicar `migration.sql`** contra la BD hosted (manual), verificando `sys.columns`/`sys.objects`.
2. **Smoke-test** de `/ConfiguracionEmpresa` (card solo lectura, editor horario 12h, subida/preview/quitado de logo, footer, sidebar).
3. (Opcional) atender las 3 sugerencias de verificación (MIME SVG, `MvcBuildViews`, drift de columnas).

## Contenido del archivo

| Artefacto | Presente |
|-----------|----------|
| `proposal.md` | ✅ |
| `design.md` | ✅ |
| `explore.md` | ✅ |
| `tasks.md` (34/34 `[x]`) | ✅ |
| `verify-report.md` (PASS) | ✅ |
| `migration.sql` | ✅ |
| `rollback.sql` | ✅ |
| `specs/configuracion-empresa/spec.md` | ✅ |

## Notas de trazabilidad

- Origen: exploración del change (`explore.md`) + decisiones fijas del usuario (D1–D5) + Anexo A de `metricas-desempeno` (tabla `EmpresaHorarioLaboral` reutilizada).
- Regla respetada: **sin tabla genérica clave/valor** `EmpresaConfiguracion` (se usó columna tipada `Empresa.LogoUrl` + tabla hija `EmpresaHorarioLaboral`); **logo solo en sidebar** (login sin cambios); **sin "Eliminar"**.
- Migración **no ejecutada** por el agente (constraint de sdd-apply) — este archive registra el estado pre-migración; la aplicación manual queda a cargo del usuario.
