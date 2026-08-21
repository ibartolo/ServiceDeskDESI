-- ============================================================
-- Rollback: tickets-ciclo-vida (respaldo / deshacer)
-- Ejecutar SOLO si se requiere revertir la migración.
-- ============================================================

-- 1. Revertir rename del estatus 4
UPDATE [dbo].[TicketEstatus] SET Nombre = 'Reabierto' WHERE Id = 4;
GO

-- 2. Restaurar SPs originales (los 3 que la migración sobrescribió)
IF OBJECT_ID(N'[dbo].[ObtenerTickets]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ObtenerTickets];
GO
CREATE PROCEDURE [dbo].[ObtenerTickets] (@Usuario NVARCHAR(25)) AS BEGIN SET NOCOUNT ON; DECLARE @EmpresaId BIGINT, @UsuarioId BIGINT, @AreaId BIGINT, @EsAgente BIT = 0; SELECT @EmpresaId = EmpresaId, @UsuarioId = Id, @AreaId = AreaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1; IF EXISTS(SELECT 1 FROM UsuarioRol ur INNER JOIN Rol r ON ur.RolId = r.Id WHERE ur.UsuarioId = @UsuarioId AND r.PuedeAtenderTickets = 1 AND ur.Estatus = 1 AND r.Estatus = 1) SET @EsAgente = 1; SELECT t.*, a.Nombre AS AreaNombre, c.Nombre AS CategoriaNombre, sc.Nombre AS SubcategoriaNombre, u.Nombre AS UsuarioCreadorNombre, u.Apellido AS UsuarioCreadorApellido, te.Nombre AS EstatusNombre, te.Color AS EstatusColor, ta.UsuarioId AS AgenteId, ag.Nombre AS AgenteNombre, ag.Apellido AS AgenteApellido, ag.NombreUsuario AS AgenteNombreUsuario FROM Ticket t INNER JOIN Area a ON t.AreaId = a.Id INNER JOIN Categoria c ON t.CategoriaId = c.Id LEFT JOIN Categoria sc ON t.SubcategoriaId = sc.Id INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario INNER JOIN TicketEstatus te ON t.TicketEstatusId = te.Id LEFT JOIN TicketAsignacion ta ON ta.TicketId = t.Id AND ta.EsActiva = 1 AND ta.Estatus = 1 LEFT JOIN Usuarios ag ON ta.UsuarioId = ag.Id WHERE t.Estatus = 1 AND t.EmpresaId = @EmpresaId AND ((@EsAgente = 0 AND t.CreadoPor = @Usuario) OR (@EsAgente = 1 AND (t.CreadoPor = @Usuario OR t.AreaId = @AreaId))) ORDER BY t.FechaCreacion DESC; END
GO

IF OBJECT_ID(N'[dbo].[ObtenerTicketsPorArea]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ObtenerTicketsPorArea];
GO
CREATE PROCEDURE [dbo].[ObtenerTicketsPorArea]
(
    @AreaId BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT t.*, a.Nombre as AreaNombre, c.Nombre as CategoriaNombre, sc.Nombre as SubcategoriaNombre,
           u.Nombre as UsuarioCreadorNombre, u.Apellido as UsuarioCreadorApellido, te.Nombre as EstatusNombre, te.Color as EstatusColor
    FROM Ticket t
    INNER JOIN Area a ON t.AreaId = a.Id
    INNER JOIN Categoria c ON t.CategoriaId = c.Id
    LEFT JOIN Categoria sc ON t.SubcategoriaId = sc.Id
    INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
    INNER JOIN TicketEstatus te ON t.TicketEstatusId = te.Id
    WHERE t.AreaId = @AreaId AND t.Estatus = 1 AND t.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY t.FechaCreacion DESC
END
GO

IF OBJECT_ID(N'[dbo].[ObtenerTicketAsignaciones]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ObtenerTicketAsignaciones];
GO
CREATE PROCEDURE [dbo].[ObtenerTicketAsignaciones] (@TicketId BIGINT) AS BEGIN SET NOCOUNT ON; SELECT ta.Id, ta.TicketId, ta.UsuarioId, ta.Comentario, ta.EsActiva, ta.CreadoPor, ta.FechaCreacion, ta.ModificadoPor, ta.FechaModificacion, ta.Estatus, ta.EmpresaId, u.Nombre AS AgenteNombre, u.Apellido AS AgenteApellido, u.NombreUsuario AS AgenteNombreUsuario FROM TicketAsignacion ta INNER JOIN Usuarios u ON ta.UsuarioId = u.Id WHERE ta.TicketId = @TicketId AND ta.Estatus = 1 ORDER BY ta.FechaCreacion DESC; END
GO

-- 3. Eliminar SPs nuevos (aditivos)
IF OBJECT_ID(N'[dbo].[TransicionarTicket]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[TransicionarTicket];
GO
IF OBJECT_ID(N'[dbo].[ObtenerUsuariosArea]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ObtenerUsuariosArea];
GO

-- 4. Quitar columnas aditivas (SOLO si no se quiere conservar el historial con TipoMovimiento)
-- ALTER TABLE [dbo].[TicketAsignacion] DROP COLUMN TipoMovimiento;
-- ALTER TABLE [dbo].[TicketAsignacion] DROP COLUMN TicketEstatusId;
