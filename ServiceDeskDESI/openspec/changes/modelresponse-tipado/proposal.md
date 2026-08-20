# Proposal: Contrato de respuesta tipado — `ModelResponse<T>`

- **Change**: `modelresponse-tipado`
- **Fase**: propose
- **Fecha**: 2026-08-18
- **Origen**: cierra el hallazgo **E8** (`entities-review` #8, `mvc-review` #7) del plan `security-remediation`, **Fase 2, severidad ALTO**.

## Intent

`ModelResponse.cs` inicializa `IsSuccess = true` (cualquier camino que olvide setear `false` reporta éxito) y `Response` es `object`, lo que fuerza al MVC a deserializar dos veces (`DeserializeObject<ModelResponse>` → `Response.ToString()` → `DeserializeObject<T>`), con los NRE asociados (`mvc-review` #7). Tipar el contrato y nacer en `false`.

## Estado

Propuesto — pendiente de spec (sdd-spec) y design (sdd-design). Cierra E8. No depende de cambios previos (los hallazgos de la Fase 2 ya están cerrados).

## Scope

### In Scope
- Nueva clase `ModelResponse<T>` y corregir `ModelResponse` (no-genérico) para que `IsSuccess` nazca en `false`.
- Migrar las firmas de `DbWrapper` / `Services` / `Controllers` del WebApi a `ModelResponse<T>` donde el payload tiene tipo conocido.
- Eliminar la doble deserialización en el MVC: un solo parse a `ModelResponse<T>` y uso directo de `.Response` tipado.
- Centralizar el parse en `HttpClientBase` (minimiza cambios por método).

### Out of Scope
- Los pocos casos heterogéneos/escalares (`Convert.ToInt64(...)`, `TokenRecuperacion` anónimo) quedan en `ModelResponse` / `ModelResponse<object>`.
- Cambiar la forma del JSON (`IsSuccess` / `Message` / `Response` se mantienen igual).
- Manejo global de excepciones y códigos HTTP (otro ítem de `manejo-errores`).

## Approach

**Decisión de diseño: `ModelResponse<T>` independiente (sin herencia), conservando `ModelResponse` no-genérico.**

```csharp
public class ModelResponse<T>
{
    public bool IsSuccess { get; set; }  // default false
    public string Message { get; set; }
    public T Response { get; set; }
}
```

- **Por qué no herencia con `new T Response`** (Opción B del análisis): ocultar `Response` con `new` genera dos propiedades públicas con la misma key `"Response"` → Newtonsoft serializa/deserializa duplicado o ambiguo. Se evita con una clase independiente.
- **Por qué no migrar TODO de golpe** (Opción A): hay ~10 paths con payload heterogéneo (`Convert.ToInt64`, anónimo `TokenRecuperacion`) sin un único `T`. Conservar `ModelResponse` permite migrar incrementalmente y por entidad.
- **Contrato JSON intacto**: `ModelResponse<T>` serializa el mismo JSON (`isSuccess`/`message`/`response` camelCase desde WebApi; Newtonsoft del MVC es case-insensitive) → los endpoints y el cliente existente no se rompen aunque no migren a la vez.
- **Cliente MVC**: añadir overload `RequestAsync<TResponse>` en `HttpClientBase` que deserializa el string crudo **una vez** a `ModelResponse<TResponse>` (conservando el manejo de error no-2xx actual). Cada método de `HttpClientConnection.*.cs` pasa a devolver `ModelResponse<T>` y Services/Controllers usan `.Response` tipado, sin `Response.ToString()` ni reparse.

### Tabla de impacto (números reales)

| Capa | Cantidad | Cambio |
|---|---|---|
| WebApi `DAL/DbWrapper*.cs` | 109 métodos / 20 archivos | firma → `ModelResponse<T>` |
| WebApi `Services/*.cs` | 93 firmas / 17 archivos | firma passthrough → `ModelResponse<T>` |
| WebApi `Controllers/*.cs` | 96 acciones / 19 archivos | tipo de retorno → `ModelResponse<T>` |
| MVC `DAL/HttpClientConnection*.cs` | 88 métodos / 18 partials | `DeserializeObject<ModelResponse>` → 1 parse `ModelResponse<T>` |
| MVC `Services/*.cs` + `Controllers/*.cs` | 58 usos `.Response.ToString()` (41 controllers / 17 services) | eliminar reparse; usar `.Response` |

## Capabilities

### New Capabilities
- `modelresponse`: contrato de respuesta tipado (`ModelResponse<T>` con `T Response`, `IsSuccess=false` por defecto) y deserialización única en el cliente MVC.

### Modified Capabilities
- None (`openspec/specs/` está vacío).

## Affected Areas

| Área | Impacto | Descripción |
|---|---|---|
| `ServiceDeskDESIEntities/Seguridad/ModelResponse.cs` | Modificado | `IsSuccess=false` por defecto + nueva `ModelResponse<T>` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper*.cs` | Modificado | firmas tipadas |
| `ServiceDeskDESIWebApi/Services/*.cs` | Modificado | firmas passthrough tipadas |
| `ServiceDeskDESIWebApi/Controllers/*.cs` | Modificado | tipo de retorno |
| `ServiceDeskDESIMVC/DAL/HttpClientBase.cs` | Modificado | overload `RequestAsync<TResponse>` (parse único) |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.*.cs` | Modificado | métodos tipados |
| `ServiceDeskDESIMVC/Services/*.cs` + `Controllers/*.cs` | Modificado | quitar `.Response.ToString()` |

## Riesgos

| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Migración parcial: DAL tipado pero Service aún hace `Response.ToString()` → `ToString()` de un objeto tipado no es JSON | Alta | Migrar por entidad DAL+Service+Controller **juntos**; checklist por entidad en `tasks.md` |
| `IsSuccess=false` por defecto revela caminos que no setean `true` (112 `new ModelResponse()` en WebApi, 4 en MVC) | Media | Auditoría de `new ModelResponse()` sin seteo; smoke test por endpoint |
| Cambiar la key `Response` o añadir `[JsonProperty]` con otro nombre rompe cliente y endpoints | Baja | No se añaden atributos de serialización; contrato se documenta en spec |
| Payload escalar/anónimo no encaja en un `T` único | Media | Quedan en `ModelResponse`/`ModelResponse<object>`; no se fuerza |
| Regresión funcional sin tests (107 SPs sin pruebas) | Media | Smoke test manual por entidad tras migrar |

## Rollback Plan

El cambio es de **tipos C# sin cambio de JSON** y el `ModelResponse` no-genérico se conserva: revertir por commit. Si una entidad queda inestable, se revierte solo su migración (DAL+Service+Controller) sin afectar al resto.

## Dependencies

- Ninguna externa. Requiere compilar `ServiceDeskDESIEntities`, `ServiceDeskDESIWebApi` y `ServiceDeskDESIMVC` juntos (assembly compartido).

## Success Criteria

- [ ] `IsSuccess` nace en `false` en `ModelResponse` y en `ModelResponse<T>`.
- [ ] Cero usos de `Response.ToString()` + `DeserializeObject` en el MVC.
- [ ] El JSON de WebApi y MVC es idéntico (3 keys) — sin cambio de contrato.
- [ ] Los tres proyectos compilan y un smoke test por endpoint (área, ticket, autenticación) pasa.
- [ ] Los métodos migrados devuelven `.Response` tipado y los caminos de error reportan `IsSuccess=false`.
