# Proposal: Logging estandarizado en todo el proyecto

- **Change**: `logging-estandarizado`
- **Fecha**: 2026-09-09
- **Estado**: 📋 **PLANEADO** — pendiente de aprobación/implementación. (El usuario pidió planearlo antes de continuar las pruebas.)
- **Origen**: hallazgos durante pruebas en localhost (casos 3, 4 y 5 de `Anomalias-Pruebas-Localhost.md`).

## Intent

Estandarizar el logging en **todo** el proyecto (MVC + WebApi) para que cualquier operación sea rastreable de punta a punta y los errores muestren la **causa real**. Objetivos concretos:

1. **MVC — capa de Servicios**: registrar los datos derivados de sesión/token que modifican el request (p. ej. `EmpresaId`, `Usuario`), hoy invisibles.
2. **WebApi — Service → DbWrapper**: registrar **todos** los datos que se envían a la base (en un solo log, o el objeto completo serializado a JSON).
3. **Errores**: que el `ex.Message` (y el stack) quede en el log de errores. Aplica a **todos** los elementos del proyecto, no solo al caso reportado.

## Evidencia (estado actual)

- **MVC**: Serilog está configurado en `Global.asax.cs` y se usa en `Controllers` y `DAL` (`HttpClientBase`, `HttpClientConnection.Autentication`), pero **NO en la capa `Services`** (que es donde se resuelve el token/EmpresaId).
- **WebApi — DbWrapper**: la mayoría de los archivos loguean el `catch`, pero **no loguean la data enviada** al SP (solo el nombre del SP en algunos casos). Además hay archivos **sin ningún log**: `BaseDbWrapper.cs`, `DbWrapper.cs`, `DbWrapper.Dashboard.cs`, `DbWrapper.Estadisticas.cs`, `DbWrapper.Evidencia.cs`, `DbWrapper.Foliador.cs`, `DbWrapper.Relacion.cs`, `DbWrapper.UsuarioPagina.cs`.
- **Errores**: en el caso del alta de usuario (`log-20260918.txt`), el DAL sí registró la excepción SQL, pero **el log de la capa de servicio solo muestra el mensaje genérico** ("Ocurrió un error al guardar el usuario"), sin la causa. Se requiere auditar todo `Log.Error` para que **siempre** reciba la excepción.

## Reglas propuestas (estándar)

| Regla | Detalle |
|---|---|
| **R1 — Entrada/Salida en Services** | Todo método público de Service (MVC y WebApi) registra: parámetros relevantes de entrada, datos de sesión/token usados, y resultado (`IsSuccess`/`Message`). |
| **R2 — Payload a la BD** | En `DbWrapper`, antes de ejecutar el SP: un log (Debug/Information) con **nombre del SP + parámetros serializados a JSON** en un solo mensaje. |
| **R3 — Errores con causa** | **Siempre** `Log.Error(ex, "contexto …")`. Prohibido `Log.Error("...")` sin la excepción. Incluir contexto (usuario, entidad, Id). |
| **R4 — Datos sensibles** | Enmascarar/omitir `Contrasena`, `Firma`, tokens y cualquier secreto. **No** loguear en claro. |
| **R5 — Niveles** | `Information` = hitos de negocio; `Debug` = payloads/detalle; `Warning` = validaciones; `Error` = excepciones. |
| **R6 — Prefijo** | Formato consistente `Clase.Metodo` (ya usado parcialmente). |
| **R7 — Helper común** | Un helper de serialización (con enmascarado de sensibles) reutilizable por MVC y WebApi para no repetir lógica. |

## Alcance por capa

- **MVC `Services/*.cs`**: agregar logging (R1).
- **MVC `DAL/HttpClientConnection.*.cs`**: confirmar/completar log de request y response.
- **WebApi `Services/*.cs`**: ya loguean hitos; reforzar R3 (causa real) y R1.
- **WebApi `DAL/DbWrapper.*.cs`**: agregar R2 (payload al SP) y completar archivos sin logs.
- **WebApi `DAL/BaseDbWrapper.cs`**: evaluar log centralizado de `cmdText` + parámetros (punto único para R2).
- **Auditoría global de `Log.Error`**: garantizar que todos pasen la excepción (R3).

## Consideraciones / riesgos

- **Sensibilidad**: el payload del `Usuario` incluye `Contrasena` (hash PBKDF2) y `Firma` → **deben enmascararse** (R4). Sin esto, el cambio introduce un riesgo de seguridad.
- **Volumen de logs**: los payloads en Debug pueden crecer; definir retención/rotación.
- **Rendimiento**: serializar a JSON tiene costo; usar nivel `Debug` y evitar serializar en bucles calientes.
- **Alcance amplio**: toca muchas clases (~25 Services WebApi, ~28 DbWrapper, Services MVC). Hacerlo en **un solo barrido** para no re-probar por partes (objetivo explícito del usuario).

## Tareas (alto nivel)

- [ ] T1 — Definir y crear el helper de serialización con enmascarado de sensibles.
- [ ] T2 — Auditar todos los `Log.Error` del proyecto y asegurar que reciban la excepción (R3).
- [ ] T3 — MVC: agregar logging a la capa `Services` (R1).
- [ ] T4 — WebApi: agregar log de payload al SP en `DbWrapper` (R2) — idealmente centralizado en `BaseDbWrapper`.
- [ ] T5 — Completar los `DbWrapper` sin logs.
- [ ] T6 — Revisar MVC `HttpClientConnection.*` (request/response).
- [ ] T7 — Verificación: reproducir el alta de usuario y confirmar que el log muestre datos enviados + causa real del error.

## Ajustes del "plan 2" (indicados por el usuario — pendientes de acordar)

Estos ajustes van **más allá del logging** y deben acordarse al aprobar el plan:

1. **`MappingColumSecurity` obsoleto:** hoy se hace en el `HttpClient` del MVC (ej. `MappingColumSecurity(compania)`); se considera **obsoleto** y debe **moverse a la capa de servicio del WebApi**, resolviéndolo desde el **token**.
2. **Auditoría desde el token:**
   - `Id = 0` (registro nuevo) → `CreadoPor` se toma del **token**.
   - `Id > 0` (registro existente) → `ModificadoPor` se toma del **token**.
   - El MVC manda los datos en inputs ocultos que viajan por el flujo; el WebApi los **resuelve/valida con el token**.
3. **Fecha desde el token/servidor:** `FechaModificacion` (y de creación) se resuelve en el WebApi, garantizando la **hora del centro de México** (no confiar en la fecha del cliente).

> Impacto: estos ajustes cambian **comportamiento**, no solo logging. Evaluar si van dentro de este cambio o en un cambio aparte (p. ej. `auditoria-desde-token`).

## Fuera de alcance

- Cambios de comportamiento/funcionalidad (los del "plan 2" se acuerdan aparte).
- El **bug de username duplicado** (caso 6): ✅ ya corregido en `username-por-empresa`.
- Los casos 1 (formato de username ✅ corregido) y 2 (CP ✅ cerrado).

## Verificación

- Reproducir el flujo de alta de usuario: el log debe mostrar (a) datos derivados del token en el MVC, (b) payload enviado al SP, (c) la causa real del error con stack.
- Confirmar que no se loguean datos sensibles.
