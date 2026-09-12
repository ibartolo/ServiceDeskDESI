-- ============================================================
-- Migration: tickets-estatus-espera
-- Estatus de espera por dependencia externa:
--   6 "Pendiente de Materiales" (compra/insumo o mantenimiento de tercero)
--   7 "En Espera de Terceros"   (respuesta de proveedor externo)
-- Idempotente. Ejecutar contra la BD viva tras tickets-ciclo-vida.
-- Fecha: 2026-09-11
-- ============================================================

-- 1. Catálogo: estatus 6 y 7 (Id explícito porque TransicionarTicket los mapea en duro)
IF NOT EXISTS (SELECT 1 FROM [dbo].[TicketEstatus] WHERE [Id] = 6)
BEGIN
    SET IDENTITY_INSERT [dbo].[TicketEstatus] ON;
    INSERT INTO [dbo].[TicketEstatus] ([Id],[Nombre],[Descripcion],[Color],[Orden],[CreadoPor],[FechaCreacion],[Estatus])
    VALUES (6, N'Pendiente de Materiales',
            N'El avance depende de una compra, insumo o mantenimiento de un tercero que aún no se ejecuta.',
            N'#fd7e14', 6, N'sistema', GETDATE(), 1);
    SET IDENTITY_INSERT [dbo].[TicketEstatus] OFF;
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[TicketEstatus] WHERE [Id] = 7)
BEGIN
    SET IDENTITY_INSERT [dbo].[TicketEstatus] ON;
    INSERT INTO [dbo].[TicketEstatus] ([Id],[Nombre],[Descripcion],[Color],[Orden],[CreadoPor],[FechaCreacion],[Estatus])
    VALUES (7, N'En Espera de Terceros',
            N'El avance depende de la respuesta de un proveedor externo; el tiempo de espera no lo controla la empresa.',
            N'#6f42c1', 7, N'sistema', GETDATE(), 1);
    SET IDENTITY_INSERT [dbo].[TicketEstatus] OFF;
END
GO

-- 2. Columna aditiva (nullable para permitir rollback)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TicketAsignacion]') AND name = 'FechaEstimada')
    ALTER TABLE [dbo].[TicketAsignacion] ADD [FechaEstimada] DATE NULL;
GO

-- 3. SP TransicionarTicket (+ pausas + reanudar + @FechaEstimada)
IF OBJECT_ID(N'[dbo].[TransicionarTicket]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[TransicionarTicket];
GO
CREATE PROCEDURE [dbo].[TransicionarTicket]
(
    @TicketId        BIGINT,
    @TipoMovimiento  NVARCHAR(20),   -- Tomar|Resolver|Retomar|Cerrar|Rechazar|Reasignar|PendienteMateriales|EnEsperaTerceros|Reanudar
    @Comentario      NVARCHAR(300) = NULL,
    @NuevoUsuarioId  BIGINT = NULL,
    @FechaEstimada   DATE = NULL,
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

    -- Pausa: agente dueño, desde En Progreso, comentario obligatorio (1..300)
    IF @TipoMovimiento IN ('PendienteMateriales','EnEsperaTerceros') AND NOT (
           @EsAgente = 1
           AND @EstatusActual = 2
           AND EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId = @TicketId AND EsActiva = 1 AND Estatus = 1 AND UsuarioId = @UsuarioId)
           AND @Comentario IS NOT NULL AND LEN(LTRIM(RTRIM(@Comentario))) BETWEEN 1 AND 300
       ) BEGIN SELECT 0; RETURN; END

    -- Reanudar: agente dueño, desde 6/7
    IF @TipoMovimiento = 'Reanudar' AND NOT (
           @EsAgente = 1
           AND @EstatusActual IN (6, 7)
           AND EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId = @TicketId AND EsActiva = 1 AND Estatus = 1 AND UsuarioId = @UsuarioId)
       ) BEGIN SELECT 0; RETURN; END

    SET @Resultado = CASE @TipoMovimiento
        WHEN 'Tomar' THEN 2 WHEN 'Resolver' THEN 3 WHEN 'Retomar' THEN 2
        WHEN 'Cerrar' THEN 5 WHEN 'Rechazar' THEN 4 WHEN 'Reasignar' THEN 2
        WHEN 'PendienteMateriales' THEN 6 WHEN 'EnEsperaTerceros' THEN 7 WHEN 'Reanudar' THEN 2 END;

    SET @AgenteFinal = CASE WHEN @TipoMovimiento = 'Reasignar' THEN @NuevoUsuarioId ELSE @UsuarioId END;
    SET @EsActiva = CASE WHEN @TipoMovimiento IN ('Cerrar','Rechazar') THEN 0 ELSE 1 END;

    -- La fecha estimada solo aplica a las pausas
    IF @TipoMovimiento NOT IN ('PendienteMateriales','EnEsperaTerceros') SET @FechaEstimada = NULL;

    BEGIN TRY
        BEGIN TRAN;
        UPDATE TicketAsignacion SET EsActiva = 0, ModificadoPor = @Usuario, FechaModificacion = GETDATE()
            WHERE TicketId = @TicketId AND EsActiva = 1 AND Estatus = 1;
        INSERT INTO TicketAsignacion (TicketId, UsuarioId, Comentario, EsActiva, TipoMovimiento, TicketEstatusId, FechaEstimada, CreadoPor, FechaCreacion, Estatus, EmpresaId)
            VALUES (@TicketId, @AgenteFinal, @Comentario, @EsActiva, @TipoMovimiento, @Resultado, @FechaEstimada, @Usuario, GETDATE(), 1, @EmpresaId);
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

-- 4. ObtenerTickets (+ FechaEstimada de la asignación activa)
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
           ta.UsuarioId AS AgenteId, ag.Nombre AS AgenteNombre, ag.Apellido AS AgenteApellido, ag.NombreUsuario AS AgenteNombreUsuario,
           ta.FechaEstimada AS FechaEstimada
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

-- 5. ObtenerTicketsPorArea (+ FechaEstimada)
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
           ta.UsuarioId AS AgenteId, ag.Nombre AS AgenteNombre, ag.Apellido AS AgenteApellido, ag.NombreUsuario AS AgenteNombreUsuario,
           ta.FechaEstimada AS FechaEstimada
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

-- 6. ObtenerTicketAsignaciones (+ FechaEstimada)
IF OBJECT_ID(N'[dbo].[ObtenerTicketAsignaciones]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ObtenerTicketAsignaciones];
GO
CREATE PROCEDURE [dbo].[ObtenerTicketAsignaciones] (@TicketId BIGINT) AS
BEGIN
    SET NOCOUNT ON;
    SELECT ta.Id, ta.TicketId, ta.UsuarioId, ta.Comentario, ta.EsActiva, ta.CreadoPor, ta.FechaCreacion,
           ta.ModificadoPor, ta.FechaModificacion, ta.Estatus, ta.EmpresaId,
           ta.TipoMovimiento, ta.TicketEstatusId, ta.FechaEstimada,
           u.Nombre AS AgenteNombre, u.Apellido AS AgenteApellido, u.NombreUsuario AS AgenteNombreUsuario,
           te.Nombre AS EstatusNombre, te.Color AS EstatusColor
    FROM TicketAsignacion ta
    INNER JOIN Usuarios u ON ta.UsuarioId = u.Id
    LEFT JOIN TicketEstatus te ON ta.TicketEstatusId = te.Id
    WHERE ta.TicketId = @TicketId AND ta.Estatus = 1
    ORDER BY ta.FechaCreacion DESC;
END
GO

-- 7. ObtenerIndicadoresDashboard (ActivosSemana incluye 6/7)
IF OBJECT_ID(N'dbo.ObtenerIndicadoresDashboard', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerIndicadoresDashboard;
GO
CREATE PROCEDURE [dbo].[ObtenerIndicadoresDashboard]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EmpresaId BIGINT;
    DECLARE @UsuarioId BIGINT;

    SELECT @EmpresaId = EmpresaId, @UsuarioId = Id
    FROM Usuarios
    WHERE NombreUsuario = @Usuario AND Estatus = 1;

    IF @EmpresaId IS NULL
    BEGIN
        SELECT 0 AS ActivosSemana, 0 AS ResueltosSemana, 0 AS Trabajando, 0 AS CerradosSemana;
        RETURN;
    END

    -- Lunes de la semana actual (independiente de DATEFIRST).
    DECLARE @InicioSemana DATE = DATEADD(day, -((DATEPART(weekday, GETDATE()) + @@DATEFIRST - 2) % 7), CAST(GETDATE() AS date));

    -- 1. Tickets activos de la semana (Nuevo=1 / En Progreso=2 / Pendiente de Materiales=6 / En Espera de Terceros=7).
    DECLARE @ActivosSemana INT = (
        SELECT COUNT(*)
        FROM Ticket
        WHERE EmpresaId = @EmpresaId
          AND Estatus = 1
          AND TicketEstatusId IN (1, 2, 6, 7)
          AND FechaCreacion >= @InicioSemana
    );

    -- 2. Tickets resueltos de la semana (movimiento 'Resolver' esta semana).
    DECLARE @ResueltosSemana INT = (
        SELECT COUNT(DISTINCT TicketId)
        FROM TicketAsignacion
        WHERE EmpresaId = @EmpresaId
          AND Estatus = 1
          AND TipoMovimiento = 'Resolver'
          AND FechaCreacion >= @InicioSemana
    );

    -- 3. Tickets en los que estoy trabajando (asignados a mí, actualmente en progreso).
    DECLARE @Trabajando INT = (
        SELECT COUNT(*)
        FROM TicketAsignacion ta
        INNER JOIN Ticket t ON t.Id = ta.TicketId AND t.Estatus = 1 AND t.EmpresaId = @EmpresaId
        WHERE ta.UsuarioId = @UsuarioId
          AND ta.EsActiva = 1
          AND ta.Estatus = 1
          AND t.TicketEstatusId = 2
    );

    -- 4. Tickets cerrados de la semana (movimiento 'Cerrar' esta semana).
    DECLARE @CerradosSemana INT = (
        SELECT COUNT(DISTINCT TicketId)
        FROM TicketAsignacion
        WHERE EmpresaId = @EmpresaId
          AND Estatus = 1
          AND TipoMovimiento = 'Cerrar'
          AND FechaCreacion >= @InicioSemana
    );

    SELECT
        @ActivosSemana   AS ActivosSemana,
        @ResueltosSemana AS ResueltosSemana,
        @Trabajando      AS Trabajando,
        @CerradosSemana  AS CerradosSemana;
END
GO
