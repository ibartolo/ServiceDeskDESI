# Verificación: Módulo "Configuración de Empresa"

- **Change**: `configuracion-empresa`
- **Versión del spec**: CE-001..CE-009 (enmendado post-apply: CE-004 12h AM/PM; CE-006 "Quitar logo")
- **Modo**: Standard (strict_tdd desactivado — sin test project; validación = revisión estática + confirmación de build)
- **Fecha**: 2026-09-07

---

## Completeness (tareas)

| Métrica | Valor |
|--------|-------|
| Tareas totales | 34 |
| Tareas completas `[x]` | 34 |
| Tareas incompletas `[ ]` | 0 |

T1–T34 marcadas `[x]`. Sin tareas pendientes.

---

## Build status

**Build**: ✅ Pasado (0 errores) — resultado registrado en apply-progress (G6 T30 + G7 T34).

- Comando: `MSBuild.exe ServiceDeskDESI.sln /t:Build|Rebuild /p:Configuration=Debug` (VS2022).
- Resultado: **0 errores** en los 3 proyectos (Entities / MVC / WebApi), confirmando los `<Compile Include>` manuales de G2/G3/G5.
- Warnings: solo 4 preexistentes (CS0168 `UserController:128` / `CatalogsController:647`; CS1998 `Startup:163/186`), ninguno introducido por este change.

> No se re-ejecutó el build en esta fase (se referencia el resultado registrado, conforme a la instrucción de conservar esfuerzo).

---

## Spec Compliance Matrix (revisión estática — sin test project)

| Req | Escenario(s) | Evidencia | Resultado |
|-----|--------------|-----------|-----------|
| CE-001 | Ítem visible en menú; llave sin tilde/etiqueta con tilde | `migration.sql:108-112` (`Nombre='ConfiguracionEmpresa'`, `NombreVisible='Configuración de Empresa'`, `Tipo='Menu'`, `Direccion='/ConfiguracionEmpresa'`, `Logo='fa-cog'`, `OrdenB=9`, `PermisosPadreId=NULL`); `MenusUser.cshtml:18` renderiza menús `Tipo=="Menu" && PermisosPadreId==null` | ✅ COMPLIANT |
| CE-002 | Permisos; acceso denegado; editar protegido | `migration.sql:115-119` (seed solo rol Administrador, `PuedeLeer=1,PuedeEditar=1`); MVC `[Permiso("ConfiguracionEmpresa","Leer")]` (`ConfiguracionEmpresaController:32,58`), `[Permiso(...,"Editar")]` (`:96,162,267`); WebApi `[Permiso]` (`HorarioLaboralController:17,22`, `EmpresaController:81`); redirección `Home/AccesoDenegado` (`Filters/PermisoAttribute.cs:57-64`); WebApi 403 (`WebApi/Filters/PermisoAttribute.cs:49-54`); sin acción Eliminar | ✅ COMPLIANT |
| CE-003 | Card solo lectura; no editables | `Index.cshtml:36-103` (13 campos con `<div>`, sin inputs) | ✅ COMPLIANT |
| CE-004 | 7 filas; guardado atómico; `HoraFin>HoraInicio`; ventana un día; día desmarcado; **AM/PM 12h** | `Index.cshtml:8-28` (`@helper HoraSelect` 3 selects Hora/Minuto/AM-PM), `:123-129` 7 filas; transacción `DbWrapper.HorarioLaboral.cs:45-59`; validación `HoraFin>HoraInicio` (`ConfiguracionEmpresaController:131`, `HorarioLaboralService:69`); día desmarcado→NULL (`ConfiguracionEmpresaController:143-144`, `HorarioLaboralService:74-78`, SP `migration.sql:61`); JS `setHora/getHora` conversión 24h↔12h (`Index.cshtml:236-261`) | ✅ COMPLIANT |
| CE-005 | Backfill idempotente; empresa nueva | `migration.sql:90-104` (backfill `WHERE NOT EXISTS`); hook PASO 5.2 (`EmpresaService.cs:566-581`, dentro de transacción `BeginTransaction` línea 341) | ✅ COMPLIANT |
| CE-006 | Subida svg/png; formato no permitido; vacío/oversize; fallback; re-subida; **quitar logo**; confirmación | `SubirLogo` (`ConfiguracionEmpresaController:163-265`) valida vacío/ext/MIME/peso, borra previo, `Uploads/Logos/{empresaId}/{guid}.{ext}`, persiste URL relativa; `QuitarLogo` (`:267-306`) borra físico + persiste NULL; NULL vía `(object)logoUrl ?? DBNull.Value` (`DbWrapper.Empresa.cs:306`); SP `GuardarLogoEmpresa` (`migration.sql:77-87`) `UPDATE ... SET LogoUrl=@LogoUrl`; fallback `MenusUser.cshtml:3-14`; confirmación Swal (`Index.cshtml:413-423`); login sin cambios (`HomeController.LogIn`) | ✅ COMPLIANT |
| CE-007 | Footer estático | `_Layout.cshtml:225-234` (footer "by DESi" + Ayuda/Términos/Privacidad, markup fijo) | ✅ COMPLIANT |
| CE-008 | Resolución por usuario; sin fuga | MVC vía `TokenCookie.EmpresaID` (`ConfiguracionEmpresaController:35,41,169,274`; `HomeController:160`); WebApi vía `User.Identity.Name`→`@Usuario`→`(SELECT EmpresaId FROM Usuarios ...)` en los 3 SPs (`migration.sql:44,59,82`); sin `EmpresaId` del cliente (SPs solo `@Usuario`/`@LogoUrl`; `GuardarLogoRequest` solo `LogoUrl`) | ✅ COMPLIANT |
| CE-009 | Español/acentos; BOM; spinner; Swal; preview | `Index.cshtml` UTF-8 CON BOM (bytes EF BB BF verificados); spinner "Guardando…" (`:305-306,373-374`); Swal éxito/error (`:320-338`); preview `FileReader` (`:205-214`) | ✅ COMPLIANT |

**Resumen de cumplimiento**: 9/9 requirements cumplidos; 28 escenarios cubiertos estáticamente.

---

## Correctness (evidencia estructural)

| Requisito | Estado | Notas |
|-----------|--------|-------|
| CE-001 | ✅ Implementado | Página + seed correctos; ícono `fa-cog`, `OrdenB=9`, independiente |
| CE-002 | ✅ Implementado | `[Permiso]` MVC y WebApi; sin "Eliminar" |
| CE-003 | ✅ Implementado | Card solo lectura completa |
| CE-004 | ✅ Implementado | Editor 12h AM/PM; transacción única; validaciones |
| CE-005 | ✅ Implementado | Backfill + hook PASO 5.2 |
| CE-006 | ✅ Implementado | Subida + Quitar logo + persistencia NULL |
| CE-007 | ✅ Implementado | Footer estático |
| CE-008 | ✅ Implementado | Multi-tenant por usuario en MVC y WebApi |
| CE-009 | ✅ Implementado | BOM, spinner, Swal, preview |

---

## Coherence (diseño)

| Decisión | ¿Seguida? | Notas |
|----------|-----------|-------|
| Guardado horario = `GuardarHorarioLaboralDia` × 7 en transacción C# | ✅ Sí | `DbWrapper.HorarioLaboral.cs:38-73` |
| Columnas `datetime NULL` ancla `1900-01-01` | ✅ Sí | `ConfiguracionEmpresaController:328`, backfill `CAST('09:00' AS datetime)` |
| Logo en disco MVC + `Empresa.LogoUrl` | ✅ Sí | `SubirLogo` pipeline; SP `GuardarLogoEmpresa` |
| Sidebar obtiene `LogoUrl` en `MenusUser()`, no en login | ✅ Sí | `HomeController.MenusUser:157-176`; `LogIn` sin cambios |
| Enmienda 1: Quitar logo (permite NULL) | ✅ Sí | `EmpresaService.GuardarLogoEmpresa` relajado (`:273-275`), `QuitarLogo()`, `DBNull.Value` |
| Enmienda 2: Editor 12h AM/PM | ✅ Sí | `@helper HoraSelect` + `setHora/getHora`, contrato "HH:mm" intacto |

**Registros `.csproj` (old-style)** verificados:
- Entities: `Catalogos\HorarioLaboral.cs` (`ServiceDeskDESIEntities.csproj:60`)
- WebApi: `Controllers\HorarioLaboralController.cs` (:223), `DAL\DbWrapper.HorarioLaboral.cs` (:251), `Services\HorarioLaboralService.cs` (:281)
- MVC: `Controllers\ConfiguracionEmpresaController.cs` (:154), `DAL\HttpClientConnection.HorarioLaboral.cs` (:172), `Models\HorarioViewModel.cs` (:195), `Services\HorarioLaboralService.cs` (:206), `Views\ConfiguracionEmpresa\Index.cshtml` (:249)

---

## Issues Found

**CRITICAL** (bloquean release):
- Ninguno.

**WARNING** (debería corregirse):
- Ninguno.

**SUGGESTION** (nice-to-have):
1. **MIME de SVG como `application/octet-stream`**: algunos navegadores envían SVG con MIME genérico; la validación MIME estricta (`ConfiguracionEmpresaController:200-211`) puede producir falsos negativos. Riesgo ya documentado en `design.md` ("Assumptions / Open items"). Verificar en smoke manual; si falla, relajar a "extensión permitida + peso" manteniendo `accept=".svg,.png"`.
2. **`MvcBuildViews` desactivado**: el compilador NO valida sintaxis Razor (`@helper`) en build; la vista `Index.cshtml` solo se valida al renderizar. Recomendado un smoke manual de `/ConfiguracionEmpresa` antes de liberar.
3. **Drift de columnas `Pagina`/`RolPaginaAccion`**: el seed asume nombres de columna (`PuedeLeer/PuedeEditar/...`, `OrdenB`, `PermisosPadreId`) que pueden diferir en la BD hosted (drift conocido, anotado en `migration.sql:107` y `design.md:178`). La migración ya fue aplicada por el usuario; si el seed no se reflejó, verificar columnas reales.

**Observación (fuera de alcance, no se califica contra CE-*)**: el cambio de tema por defecto (light/dark) implementado en `Helpers/ThemeHelper.cs`, `HomeController.GuardarTema` y `HomeController.LogIn` (llamada `ThemeHelper.AsegurarTemaCookie`) es un ajuste de UX separado, NO forma parte del spec de este change. Se menciona solo para trazabilidad; no afecta el veredicto.

---

## Verdict

**PASS**

La implementación satisface CE-001..CE-009 (incluidas las dos enmiendas CE-004 y CE-006), con build 0 errores, registros `.csproj` completos, aislamiento multi-tenant correcto (sin `EmpresaId` del cliente) y persistencia de logo NULL vía `(object)logoUrl ?? DBNull.Value`. Sin issues CRITICAL ni WARNING. Listo para `sdd-archive`.
