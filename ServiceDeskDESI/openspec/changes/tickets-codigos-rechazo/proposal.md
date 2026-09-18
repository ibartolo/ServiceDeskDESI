# Proposal: Códigos de rechazo en TransicionarTicket

- **Change**: `tickets-codigos-rechazo`
- **Fecha**: 2026-09-09
- **Estado**: ✅ **Aplicado en dev** — pendiente aplicar a prod.
- **Origen**: Al tomar un ticket, el sistema mostraba un mensaje genérico ("Verifique que sea un agente, que el ticket sea de su área…") sin saber la causa real.

## Problema

1. El SP `TransicionarTicket` devolvía **`0`** ante **cualquier** rechazo → el DAL no podía distinguir el motivo.
2. La validación de **'Tomar'** exigía que el agente fuera **del mismo ÁREA del ticket** (`Usuarios.AreaId = Ticket.AreaId`) → inconsistente con la visibilidad global (los agentes ya ven todos los tickets).

## Cambios

### SP `TransicionarTicket` (BD)
- Ahora devuelve un **código negativo por motivo** de rechazo (antes: `0` genérico).
- Se **quitó la validación de área** en `Tomar` y `Retomar` (un agente puede tomar cualquier ticket de su empresa).

**Tabla de códigos:**

| Código | Significado |
|---|---|
| `> 0` | ✅ Éxito (Id de la asignación creada) |
| `0` | Error inesperado (catch) |
| `-1` | Usuario no encontrado o inactivo |
| `-2` | Ticket no encontrado / no pertenece a la empresa |
| `-3` | El usuario no es agente (no puede atender tickets) |
| `-4` | El estatus del ticket no permite ese movimiento |
| `-5` | El ticket no está disponible (ya tiene asignación activa) — *Tomar* |
| `-6` | El usuario no es el agente asignado (dueño) — *Resolver/Pausas/Reanudar* |
| `-7` | Comentario requerido o inválido (1..300) |
| `-8` | El usuario no es responsable del área del ticket — *Reasignar* |
| `-9` | El usuario destino no es un agente válido del área — *Reasignar* |
| `-10` | El usuario no es el creador del ticket — *Cerrar/Rechazar* |

### DAL `DbWrapper.Ticket.cs` (código)
- Nuevo helper `MensajeRechazoTransicion(long codigo)` que traduce el código a mensaje claro.
- Los 8 métodos (`TomarTicket`, `ReasignarTicket`, `ResolverTicket`, `RechazarTicket`, `CerrarTicket`, `RetomarTicket`, `PausarTicket`, `ReanudarTicket`) ahora usan el helper en vez del mensaje genérico.

## Verificación

- ✅ Build WebApi: 0 errores.
- ✅ Códigos probados contra la BD: ticket inexistente → `-2`; estatus inválido → `-4`.
- ✅ Ticket 17 (área LeaderPeople) quedó **disponible** (estatus Nuevo, sin asignación) para que un agente lo tome.

## Archivos

- `openspec/changes/tickets-codigos-rechazo/migration.sql`
- `ServiceDeskDESIWebApi/DAL/DbWrapper.Ticket.cs`

## Despliegue a prod

```powershell
& "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE" `
  -S sql8005.site4now.net -U db_9c7990_helpdeskdesi_admin -P "<pass>" `
  -d db_9c7990_helpdeskdesi -C -i openspec\changes\tickets-codigos-rechazo\migration.sql -b
```
(y publicar el WebApi con el DAL actualizado)
