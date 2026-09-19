-- ============================================================
-- Migration: dashboard-indicadores
-- Indicadores del dashboard del agente (4 métricas de la semana).
-- Fecha: 2026-08-21
-- ============================================================

-- 1. SP ObtenerIndicadoresDashboard
--    Devuelve una sola fila con 4 indicadores del usuario autenticado (multi-tenant):
--      ActivosSemana   = tickets abiertos (Nuevo/En Progreso) creados esta semana
--      ResueltosSemana = tickets resueltos esta semana (movimiento 'Resolver')
--      Trabajando      = tickets asignados a mí y actualmente en progreso (EsActiva = 1)
--      CerradosSemana  = tickets cerrados esta semana (movimiento 'Cerrar')
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

    -- 1. Tickets activos de la semana (abiertos: Nuevo=1 / En Progreso=2, creados esta semana).
    DECLARE @ActivosSemana INT = (
        SELECT COUNT(*)
        FROM Ticket
        WHERE EmpresaId = @EmpresaId
          AND Estatus = 1
          AND TicketEstatusId IN (1, 2)
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
