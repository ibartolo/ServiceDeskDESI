# Design: Notificación por correo en cada cambio de estatus de ticket

## Technical Approach

Centralizar la notificación en un único método privado `NotificarCambioEstatus` dentro de `ServiceDeskDESIWebApi/Services/TicketService.cs`. Cada uno de los 8 métodos de transición, **tras una transición exitosa**, invoca ese método. El método:

1. Carga el ticket con `_dbWrapper.ObtenerTicketPorId(ticketId, usuario)` (folio, título, categoría, urgencia, fecha de creación, descripción, estatus actual + color, `CreadoPor`).
2. Obtiene el creador con `_dbWrapper.ObtenerUsuarioPorNombreUsuario(ticket.CreadoPor, usuario)` (nombre, apellido, **correo**).
3. Lee `~/Template/Template_CambioEstatusTicket.html`, reemplaza los placeholders y envía con `EmailHelper.EnvioEmaiil`.

Todo dentro de `try/catch` (best-effort): un fallo de correo **no** rompe ni revierte la transición.

> **Sin cambios de BD / Entities / MVC.** Se reutilizan SPs y entidades existentes.

---

## 1. Transiciones y llamadas

En `TicketService.cs`, tras `var result = _dbWrapper.<Transición>(...);`, añadir:

```csharp
if (result != null && result.IsSuccess)
{
    NotificarCambioEstatus(ticketId, usuario, "<TipoMovimiento>", comentario[, fechaEstimada]);
}
```

| Método | `TipoMovimiento` | Comentario |
|---|---|---|
| `TomarTicket` | `Tomar` | `comentario` |
| `ReasignarTicket` | `Reasignar` | `comentario` |
| `ResolverTicket` | `Resolver` | `comentario` |
| `RechazarTicket` | `Rechazar` | `comentario` |
| `CerrarTicket` | `Cerrar` | `comentario` |
| `RetomarTicket` | `Retomar` | `null` (no recibe comentario) |
| `PausarTicket` | `tipoMovimiento` (`PendienteMateriales`/`EnEsperaTerceros`) | `comentario`, `fechaEstimada` |
| `ReanudarTicket` | `Reanudar` | `comentario` |

`using ServiceDeskDESIWebApi.Helpers;` se agrega al inicio de `TicketService.cs`.

---

## 2. `NotificarCambioEstatus`

```csharp
private void NotificarCambioEstatus(long ticketId, string usuario, string tipoMovimiento, string comentario, DateTime? fechaEstimada = null)
{
    try
    {
        var ticketResp = _dbWrapper.ObtenerTicketPorId(ticketId, usuario);
        if (ticketResp == null || !ticketResp.IsSuccess || ticketResp.Response == null)
        {
            Log.Warning("No se pudo obtener el ticket {TicketId} para notificar el cambio de estatus.", ticketId);
            return;
        }

        var ticket = ticketResp.Response;

        string correoCreador = null;
        string nombreCreador = ticket.CreadoPor;
        var creadorResp = _dbWrapper.ObtenerUsuarioPorNombreUsuario(ticket.CreadoPor, usuario);
        if (creadorResp != null && creadorResp.IsSuccess && creadorResp.Response != null)
        {
            correoCreador = creadorResp.Response.Correo;
            nombreCreador = $"{creadorResp.Response.Nombre} {creadorResp.Response.Apellido}".Trim();
        }

        if (string.IsNullOrWhiteSpace(correoCreador))
        {
            Log.Warning("El creador del ticket {TicketId} no tiene correo; no se envía notificación.", ticketId);
            return;
        }

        var templatePath = HostingEnvironment.MapPath("~/Template/Template_CambioEstatusTicket.html");
        if (string.IsNullOrEmpty(templatePath) || !File.Exists(templatePath))
        {
            Log.Error("No se encontró la plantilla de notificación de cambio de estatus en {TemplatePath}.", templatePath);
            return;
        }

        string mensaje, nota;
        ObtenerMensajeYNota(tipoMovimiento, comentario, fechaEstimada, ticket.EstatusNombre, out mensaje, out nota);

        var colorEstatus = string.IsNullOrWhiteSpace(ticket.EstatusColor) ? "#4e73df" : ticket.EstatusColor;
        var baseUri = ConfigurationManager.AppSettings["BaseUri"] ?? string.Empty;

        var html = File.ReadAllText(templatePath)
            .Replace("{{NombreUsuario}}", string.IsNullOrWhiteSpace(nombreCreador) ? ticket.CreadoPor : nombreCreador)
            .Replace("{{MensajeEstatus}}", mensaje)
            .Replace("{{ColorEstatus}}", colorEstatus)
            .Replace("{{NumeroTicket}}", string.IsNullOrWhiteSpace(ticket.Folio) ? ticket.Id.ToString() : ticket.Folio)
            .Replace("{{TituloTicket}}", ticket.Titulo ?? string.Empty)
            .Replace("{{Categoria}}", ticket.CategoriaNombre ?? string.Empty)
            .Replace("{{ColorPrioridad}}", ObtenerPrioridadColor(ticket.Urgencia))
            .Replace("{{Prioridad}}", ObtenerPrioridadTexto(ticket.Urgencia))
            .Replace("{{FechaCreacion}}", ticket.FechaCreacion.ToString("dd/MM/yyyy HH:mm"))
            .Replace("{{Estatus}}", ticket.EstatusNombre ?? string.Empty)
            .Replace("{{DescripcionTicket}}", ticket.Descripcion ?? string.Empty)
            .Replace("{{NotaAdicional}}", nota)
            .Replace("{{UrlTicket}}", $"{baseUri}Ticket/Index");

        var asunto = $"Ticket {(string.IsNullOrWhiteSpace(ticket.Folio) ? "#" + ticket.Id : ticket.Folio)} - Estatus: {ticket.EstatusNombre}";

        EmailHelper.EnvioEmaiil(new List<string> { correoCreador }, asunto, html, false);
        Log.Information("Notificación de cambio de estatus ({TipoMovimiento}) enviada para ticket {TicketId} a {Correo}.", tipoMovimiento, ticketId, correoCreador);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error al notificar el cambio de estatus del ticket {TicketId}.", ticketId);
    }
}
```

---

## 3. Mensaje y nota por movimiento (`ObtenerMensajeYNota`)

| Movimiento | Mensaje | Nota |
|---|---|---|
| `Tomar` | "Tu ticket fue tomado por un agente y está siendo atendido." | comentario o "El ticket pasó a En Progreso." |
| `Resolver` | "El agente marcó tu ticket como Resuelto. Revisa la solución y, si estás de acuerdo, ciérralo." | comentario o "El ticket fue resuelto." |
| `Rechazar` | "Tu ticket fue marcado como Rechazado. Un agente puede retomarlo." | comentario |
| `Cerrar` | "Tu ticket fue Cerrado. ¡Gracias por confirmar!" | comentario |
| `Retomar` | "El agente retomó tu ticket rechazado y vuelve a estar En Progreso." | comentario |
| `Reasignar` | "Tu ticket fue reasignado a otro agente y sigue En Progreso." | comentario |
| `PendienteMateriales` | "Tu ticket está Pendiente de Materiales…" | comentario + "Fecha estimada: dd/MM/yyyy" |
| `EnEsperaTerceros` | "Tu ticket está En Espera de Terceros…" | comentario + "Fecha estimada: dd/MM/yyyy" |
| `Reanudar` | "Tu ticket se reanudó y vuelve a estar En Progreso." | comentario |

Prioridad (`ObtenerPrioridadTexto`/`ObtenerPrioridadColor`): `1 Baja #1cc88a`, `2 Media #36b9cc`, `3 Alta #f6c23e`, `4 Crítica #e74a3b`.

---

## 4. Build Considerations

- Sin nuevos `.cs` → no se edita ningún `.csproj`.
- Solo se modifica `ServiceDeskDESIWebApi/Services/TicketService.cs`.
- Compilar `ServiceDeskDESI.sln` con MSBuild VS2022 → 0 errores.

---

## 5. Decision Log

| # | Decisión | Alternativas | Por qué |
|---|---|---|---|
| D1 | Notificar en la capa de servicio (WebApi) | Trigger en BD / en el SP | El SP no debe depender de SMTP; el servicio ya tiene el contexto y `EmailHelper` |
| D2 | Reutilizar `ObtenerTicketPorId` + `ObtenerUsuarioPorNombreUsuario` | Nuevo SP con email del creador | Evita migración; ambas consultas ya existen y traen lo necesario |
| D3 | Un único `NotificarCambioEstatus` | Lógica duplicada por método | Un solo punto, fácil de mantener y auditar |
| D4 | Best-effort (`try/catch`) | Revertir la transición si falla el correo | Un correo caído no debe impedir operar el ticket |
| D5 | Destinatario = creador | También agente/área | Es lo solicitado: "avisar a la persona que lo creó" |

---

## 6. Testing Strategy (strict_tdd=false)

| Capa | Qué | Cómo |
|---|---|---|
| WebApi | Se dispara en los 9 movimientos | Prueba manual: ejecutar cada transición y verificar correo |
| Contenido | Placeholders reemplazados | Inspeccionar el HTML recibido |
| Resiliencia | SMTP caído | Config inválida → transición OK + log de error |
| Build | 0 errores | MSBuild VS2022 sobre `ServiceDeskDESI.sln` |

---

## 7. Rollback

- Revertir los cambios en `TicketService.cs`. No hay migración ni cambios de datos.
