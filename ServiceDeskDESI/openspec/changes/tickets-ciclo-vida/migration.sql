-- ============================================================
-- Migration: tickets-ciclo-vida
-- Migración directa (idempotente). Ejecutar contra la BD viva.
-- Fecha: 2026-08-19
-- ============================================================

-- 1. Renombrar estatus 4 "Reabierto" -> "Rechazado"
UPDATE [dbo].[TicketEstatus] SET Nombre = 'Rechazado' WHERE Id = 4;
GO

-- 2. Columnas aditivas en TicketAsignacion (nullable para permitir rollback)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TicketAsignacion]') AND name = 'TipoMovimiento')
    ALTER TABLE [dbo].[TicketAsignacion] ADD [TipoMovimiento] NVARCHAR(20) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TicketAsignacion]') AND name = 'TicketEstatusId')
    ALTER TABLE [dbo].[TicketAsignacion] ADD [TicketEstatusId] INT NULL;
GO

-- 3. Backfill de filas existentes
--    Activas: representan un "Tomar" aún vigente
UPDATE ta SET ta.TipoMovimiento = 'Tomar', ta.TicketEstatusId = t.TicketEstatusId
FROM [dbo].[TicketAsignacion] ta
INNER JOIN [dbo].[Ticket] t ON ta.TicketId = t.Id
WHERE ta.TipoMovimiento IS NULL AND ta.EsActiva = 1;
GO

--    Históricas: fueron cerradas por una reasignación
UPDATE [dbo].[TicketAsignacion]
SET TipoMovimiento = 'Reasignar', TicketEstatusId = 2
WHERE TipoMovimiento IS NULL AND EsActiva = 0;
GO

-- 4. SP TransicionarTicket (unificado)
IF OBJECT_ID(N'[dbo].[TransicionarTicket]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[TransicionarTicket];
GO
CREATE PROCEDURE [dbo].[TransicionarTicket]
(
    @TicketId        BIGINT,
    @TipoMovimiento  NVARCHAR(20),   -- Tomar|Resolver|Retomar|Cerrar|Rechazar|Reasignar
    @Comentario      NVARCHAR(300) = NULL,
    @NuevoUsuarioId  BIGINT = NULL,
    @Usuario         NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UsuarioId BIGINT, @EmpresaId BIGINT, @EstatusActual INT, @EsAgente BIT = 0,
            @Resultado INT, @AgenteFinal BIGINT, @EsActiva BIT = 1, @AsignacionId BIGINT;

    SELECT @UsuarioId = Id, @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @UsuarioId IS NULL BEGIN SELECT 0; RETURN; END

    SELECT @EstatusActual = TicketEstatusId FROM Ticket WHERE Id = @TicketId AND Estatus = 1 AND EmpresaId = @EmpresaId;
    IF @EstatusActual IS NULL BEGIN SELECT 0; RETURN; END

    IF EXISTS(SELECT 1 FROM UsuarioRol ur INNER JOIN Rol r ON ur.RolId = r.Id
              WHERE ur.UsuarioId = @UsuarioId AND r.PuedeAtenderTickets = 1 AND ur.Estatus = 1 AND r.Estatus = 1)
        SET @EsAgente = 1;

    -- Validaciones por movimiento
    IF @TipoMovimiento = 'Tomar' AND NOT (
           @EsAgente = 1
           AND @EstatusActual = 1
           AND NOT EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId = @TicketId AND EsActiva = 1 AND Estatus = 1)
           AND EXISTS(SELECT 1 FROM Usuarios WHERE Id = @UsuarioId AND AreaId = (SELECT AreaId FROM Ticket WHERE Id = @TicketId))
       ) BEGIN SELECT 0; RETURN; END

    IF @TipoMovimiento = 'Resolver' AND NOT (
           @EsAgente = 1
           AND @EstatusActual = 2
           AND EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId = @TicketId AND EsActiva = 1 AND Estatus = 1 AND UsuarioId = @UsuarioId)
           AND @Comentario IS NOT NULL AND LEN(LTRIM(RTRIM(@Comentario))) BETWEEN 1 AND 300
       ) BEGIN SELECT 0; RETURN; END

    IF @TipoMovimiento = 'Retomar' AND NOT (
           @EsAgente = 1
           AND @EstatusActual = 4
           AND EXISTS(SELECT 1 FROM Usuarios WHERE Id = @UsuarioId AND AreaId = (SELECT AreaId FROM Ticket WHERE Id = @TicketId))
       ) BEGIN SELECT 0; RETURN; END

    IF @TipoMovimiento IN ('Cerrar','Rechazar') AND NOT (
           (SELECT CreadoPor FROM Ticket WHERE Id = @TicketId) = @Usuario
           AND @EstatusActual = 3
           AND @Comentario IS NOT NULL AND LEN(LTRIM(RTRIM(@Comentario))) BETWEEN 1 AND 300
       ) BEGIN SELECT 0; RETURN; END

    IF @TipoMovimiento = 'Reasignar' AND NOT (
           @NuevoUsuarioId IS NOT NULL
           AND @EstatusActual IN (2, 4)
           AND @Comentario IS NOT NULL AND LEN(LTRIM(RTRIM(@Comentario))) BETWEEN 1 AND 300
           AND EXISTS(SELECT 1 FROM Area WHERE Id = (SELECT AreaId FROM Ticket WHERE Id = @TicketId) AND UsuarioResponsableId = @UsuarioId)
           AND EXISTS(SELECT 1 FROM Usuarios u INNER JOIN UsuarioRol ur ON u.Id = ur.UsuarioId INNER JOIN Rol r ON ur.RolId = r.Id
                      WHERE u.Id = @NuevoUsuarioId AND u.EmpresaId = @EmpresaId AND u.Estatus = 1
                        AND ur.Estatus = 1 AND r.Estatus = 1 AND r.PuedeAtenderTickets = 1
                        AND u.AreaId = (SELECT AreaId FROM Ticket WHERE Id = @TicketId))
       ) BEGIN SELECT 0; RETURN; END

    SET @Resultado = CASE @TipoMovimiento
        WHEN 'Tomar' THEN 2 WHEN 'Resolver' THEN 3 WHEN 'Retomar' THEN 2
        WHEN 'Cerrar' THEN 5 WHEN 'Rechazar' THEN 4 WHEN 'Reasignar' THEN 2 END;

    SET @AgenteFinal = CASE WHEN @TipoMovimiento = 'Reasignar' THEN @NuevoUsuarioId ELSE @UsuarioId END;
    SET @EsActiva = CASE WHEN @TipoMovimiento IN ('Cerrar','Rechazar') THEN 0 ELSE 1 END;

    BEGIN TRY
        BEGIN TRAN;
        UPDATE TicketAsignacion SET EsActiva = 0, ModificadoPor = @Usuario, FechaModificacion = GETDATE()
            WHERE TicketId = @TicketId AND EsActiva = 1 AND Estatus = 1;
        INSERT INTO TicketAsignacion (TicketId, UsuarioId, Comentario, EsActiva, TipoMovimiento, TicketEstatusId, CreadoPor, FechaCreacion, Estatus, EmpresaId)
            VALUES (@TicketId, @AgenteFinal, @Comentario, @EsActiva, @TipoMovimiento, @Resultado, @Usuario, GETDATE(), 1, @EmpresaId);
        SET @AsignacionId = SCOPE_IDENTITY();
        UPDATE Ticket SET TicketEstatusId = @Resultado, ModificadoPor = @Usuario, FechaModificacion = GETDATE() WHERE Id = @TicketId;
        COMMIT TRAN;
        SELECT @AsignacionId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        SELECT 0;
    END CATCH
END
GO

-- 5. SP ObtenerUsuariosArea (nuevo)
IF OBJECT_ID(N'[dbo].[ObtenerUsuariosArea]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ObtenerUsuariosArea];
GO
CREATE PROCEDURE [dbo].[ObtenerUsuariosArea]
(
    @AreaId BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT u.Id, u.NombreUsuario, u.Nombre, u.Apellido, u.Correo, u.AreaId, u.EmpresaId, u.Estatus, a.Nombre AS AreaNombre
    FROM Usuarios u
    INNER JOIN Area a ON u.AreaId = a.Id
    INNER JOIN UsuarioRol ur ON ur.UsuarioId = u.Id AND ur.Estatus = 1
    INNER JOIN Rol r ON ur.RolId = r.Id AND r.PuedeAtenderTickets = 1 AND r.Estatus = 1
    WHERE u.AreaId = @AreaId AND u.Estatus = 1
      AND u.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY u.Nombre, u.Apellido;
END
GO

-- 6. ObtenerTickets (modificar: añadir CreadoPorId)
IF OBJECT_ID(N'[dbo].[ObtenerTickets]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ObtenerTickets];
GO
CREATE PROCEDURE [dbo].[ObtenerTickets] (@Usuario NVARCHAR(25)) AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT, @UsuarioId BIGINT, @AreaId BIGINT, @EsAgente BIT = 0;
    SELECT @EmpresaId = EmpresaId, @UsuarioId = Id, @AreaId = AreaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF EXISTS(SELECT 1 FROM UsuarioRol ur INNER JOIN Rol r ON ur.RolId = r.Id WHERE ur.UsuarioId = @UsuarioId AND r.PuedeAtenderTickets = 1 AND ur.Estatus = 1 AND r.Estatus = 1) SET @EsAgente = 1;
    SELECT t.*, a.Nombre AS AreaNombre, c.Nombre AS CategoriaNombre, sc.Nombre AS SubcategoriaNombre,
           u.Nombre AS UsuarioCreadorNombre, u.Apellido AS UsuarioCreadorApellido,
           u.Id AS CreadoPorId,
           te.Nombre AS EstatusNombre, te.Color AS EstatusColor,
           ta.UsuarioId AS AgenteId, ag.Nombre AS AgenteNombre, ag.Apellido AS AgenteApellido, ag.NombreUsuario AS AgenteNombreUsuario
    FROM Ticket t
    INNER JOIN Area a ON t.AreaId = a.Id
    INNER JOIN Categoria c ON t.CategoriaId = c.Id
    LEFT JOIN Categoria sc ON t.SubcategoriaId = sc.Id
    INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
    INNER JOIN TicketEstatus te ON t.TicketEstatusId = te.Id
    LEFT JOIN TicketAsignacion ta ON ta.TicketId = t.Id AND ta.EsActiva = 1 AND ta.Estatus = 1
    LEFT JOIN Usuarios ag ON ta.UsuarioId = ag.Id
    WHERE t.Estatus = 1 AND t.EmpresaId = @EmpresaId
      AND ((@EsAgente = 0 AND t.CreadoPor = @Usuario) OR (@EsAgente = 1 AND (t.CreadoPor = @Usuario OR t.AreaId = @AreaId)))
    ORDER BY t.FechaCreacion DESC;
END
GO

-- 7. ObtenerTicketsPorArea (modificar: añadir agent JOIN + CreadoPorId)
IF OBJECT_ID(N'[dbo].[ObtenerTicketsPorArea]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ObtenerTicketsPorArea];
GO
CREATE PROCEDURE [dbo].[ObtenerTicketsPorArea]
(
    @AreaId BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.*, a.Nombre AS AreaNombre, c.Nombre AS CategoriaNombre, sc.Nombre AS SubcategoriaNombre,
           u.Nombre AS UsuarioCreadorNombre, u.Apellido AS UsuarioCreadorApellido,
           u.Id AS CreadoPorId,
           te.Nombre AS EstatusNombre, te.Color AS EstatusColor,
           ta.UsuarioId AS AgenteId, ag.Nombre AS AgenteNombre, ag.Apellido AS AgenteApellido, ag.NombreUsuario AS AgenteNombreUsuario
    FROM Ticket t
    INNER JOIN Area a ON t.AreaId = a.Id
    INNER JOIN Categoria c ON t.CategoriaId = c.Id
    LEFT JOIN Categoria sc ON t.SubcategoriaId = sc.Id
    INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
    INNER JOIN TicketEstatus te ON t.TicketEstatusId = te.Id
    LEFT JOIN TicketAsignacion ta ON ta.TicketId = t.Id AND ta.EsActiva = 1 AND ta.Estatus = 1
    LEFT JOIN Usuarios ag ON ta.UsuarioId = ag.Id
    WHERE t.AreaId = @AreaId AND t.Estatus = 1 AND t.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY t.FechaCreacion DESC;
END
GO

-- 8. ObtenerTicketAsignaciones (modificar: añadir TipoMovimiento/TicketEstatusId/EstatusNombre/EstatusColor)
IF OBJECT_ID(N'[dbo].[ObtenerTicketAsignaciones]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ObtenerTicketAsignaciones];
GO
CREATE PROCEDURE [dbo].[ObtenerTicketAsignaciones] (@TicketId BIGINT) AS
BEGIN
    SET NOCOUNT ON;
    SELECT ta.Id, ta.TicketId, ta.UsuarioId, ta.Comentario, ta.EsActiva, ta.CreadoPor, ta.FechaCreacion,
           ta.ModificadoPor, ta.FechaModificacion, ta.Estatus, ta.EmpresaId,
           ta.TipoMovimiento, ta.TicketEstatusId,
           u.Nombre AS AgenteNombre, u.Apellido AS AgenteApellido, u.NombreUsuario AS AgenteNombreUsuario,
           te.Nombre AS EstatusNombre, te.Color AS EstatusColor
    FROM TicketAsignacion ta
    INNER JOIN Usuarios u ON ta.UsuarioId = u.Id
    LEFT JOIN TicketEstatus te ON ta.TicketEstatusId = te.Id
    WHERE ta.TicketId = @TicketId AND ta.Estatus = 1
    ORDER BY ta.FechaCreacion DESC;
END
GO
