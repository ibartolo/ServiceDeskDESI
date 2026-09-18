# Proposal: Visibilidad de tickets para agentes (todas las áreas)

- **Change**: `tickets-visibilidad-agente`
- **Fecha**: 2026-09-09
- **Estado**: ✅ **Aplicado en dev** — pendiente aplicar a prod (script listo).
- **Origen**: Escenario real — los 2 usuarios de TI (área TI) no podían ver tickets levantados por otras áreas.

## Intent

En una mesa de ayuda, quien **atiende** (rol con `PuedeAtenderTickets = 1`) debe ver **todos** los tickets de su empresa, sin importar el área del ticket. Antes solo veía los de **su propia área** (`t.AreaId = @AreaId`), lo cual está al revés para una mesa de ayuda (las áreas *solicitan*, TI *atiende*).

## Decisión (opción B, aprobada)

- **Agente** (`PuedeAtenderTickets = 1`) → ve **TODOS** los tickets de la empresa.
- **No agente** → sigue viendo **solo los suyos** (`t.CreadoPor = @Usuario`).

No se agrega flag nuevo. Si más adelante se requiere separación por área, se evaluará un flag explícito (`Rol.VeTodosLosTickets`).

## Cambio

SP `ObtenerTickets` — cláusula final:
```sql
-- ANTES
AND ((@EsAgente = 0 AND t.CreadoPor = @Usuario)
     OR (@EsAgente = 1 AND (t.CreadoPor = @Usuario OR t.AreaId = @AreaId)))

-- AHORA
AND ((@EsAgente = 0 AND t.CreadoPor = @Usuario)
     OR (@EsAgente = 1))
```

## Fuera de alcance

- `ObtenerTicketsPorArea` se deja igual: es un **filtro explícito por área** (el usuario elige el área), no depende del rol.
  - ⚠️ Nota: `ObtenerTicketsPorArea` **no valida el rol**; cualquier usuario de la empresa podría listar los tickets de otra área si conoce el `AreaId`. Revisar si se desea restringir.

## Verificación

- ✅ Aplicado en dev (`SQL5105` / `db_9c7990_servicedeskdesi`).
- ✅ Prueba: `ivan.bartolo` (agente, empresa 27) ahora ve el ticket del área *LeaderPeople* (antes invisible).

## Despliegue a prod

```powershell
& "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE" `
  -S sql8005.site4now.net -U db_9c7990_helpdeskdesi_admin -P "<pass>" `
  -d db_9c7990_helpdeskdesi -C -i openspec\changes\tickets-visibilidad-agente\migration.sql -b
```

## Archivos

- `openspec/changes/tickets-visibilidad-agente/migration.sql`
- `openspec/changes/tickets-visibilidad-agente/proposal.md` (este archivo)
