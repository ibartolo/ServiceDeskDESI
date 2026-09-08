-- ============================================================
-- Migration: metricas-desempeno
-- Módulo "Estadísticas" (métricas y desempeño, solo lectura).
-- Fecha: 2026-09-07
-- Idempotente: guards IF OBJECT_ID / IF NOT EXISTS / NOT EXISTS.
-- NO ejecutar contra la BD desde sdd-apply; la aplica el usuario manualmente.
-- ============================================================

-- ============================================================
-- ⚠️  PRECHECK (T1) — EJECUTAR ESTE BLOQUE ANTES DE APLICAR LA MIGRACIÓN
-- --------------------------------------------------------------------
-- El CREATE de TicketAsignacion NO vive en el dump del repo (solo en la
-- BD hosted). Antes de crear los 5 SPs, verifique que las columnas que
-- consumen existan realmente. Ejecute los SELECT de abajo y compare contra
-- las listas comentadas. Si falta alguna columna (drift), NO aplique la
-- migración y ajuste los SPs a las columnas reales.
--
-- Columnas esperadas:
--   TicketAsignacion  : Id, TicketId, UsuarioId, Comentario, EsActiva,
--                       TipoMovimiento, TicketEstatusId, CreadoPor,
--                       FechaCreacion, ModificadoPor, FechaModificacion,
--                       Estatus, EmpresaId
--   Ticket            : Id, Folio, AreaId, Urgencia, TicketEstatusId,
--                       FechaCreacion, EmpresaId, Estatus
--   TicketEstatus     : Id, Nombre, Color, Orden, Estatus
--   EmpresaHorarioLaboral: EmpresaId, DiaSemana, HoraInicio, HoraFin, Estatus
--   Usuarios          : Id, EmpresaId, NombreUsuario, Nombre, Apellido, Estatus
--   Area              : Id, Nombre, Estatus
-- ============================================================

-- TicketAsignacion (debe listar 13 columnas)
SELECT 'TicketAsignacion' AS Tabla, c.name AS Columna
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'[dbo].[TicketAsignacion]')
  AND c.name IN ('Id','TicketId','UsuarioId','Comentario','EsActiva','TipoMovimiento',
                 'TicketEstatusId','CreadoPor','FechaCreacion','ModificadoPor',
                 'FechaModificacion','Estatus','EmpresaId')
ORDER BY c.name;
GO

-- Ticket (debe listar 8 columnas)
SELECT 'Ticket' AS Tabla, c.name AS Columna
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'[dbo].[Ticket]')
  AND c.name IN ('Id','Folio','AreaId','Urgencia','TicketEstatusId','FechaCreacion','EmpresaId','Estatus')
ORDER BY c.name;
GO

-- TicketEstatus (debe listar 5 columnas)
SELECT 'TicketEstatus' AS Tabla, c.name AS Columna
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'[dbo].[TicketEstatus]')
  AND c.name IN ('Id','Nombre','Color','Orden','Estatus')
ORDER BY c.name;
GO

-- EmpresaHorarioLaboral (debe listar 5 columnas)
SELECT 'EmpresaHorarioLaboral' AS Tabla, c.name AS Columna
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'[dbo].[EmpresaHorarioLaboral]')
  AND c.name IN ('EmpresaId','DiaSemana','HoraInicio','HoraFin','Estatus')
ORDER BY c.name;
GO

-- Usuarios (debe listar 6 columnas)
SELECT 'Usuarios' AS Tabla, c.name AS Columna
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'[dbo].[Usuarios]')
  AND c.name IN ('Id','EmpresaId','NombreUsuario','Nombre','Apellido','Estatus')
ORDER BY c.name;
GO

-- Area (debe listar 3 columnas)
SELECT 'Area' AS Tabla, c.name AS Columna
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'[dbo].[Area]')
  AND c.name IN ('Id','Nombre','Estatus')
ORDER BY c.name;
GO

-- ============================================================
-- 1. Seed de página "Estadisticas" (idempotente)
--    Llave sin tilde 'Estadisticas'; etiqueta visible 'Estadísticas'.
--    Ítem de primer nivel (PermisosPadreId NULL). (MD-001)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Pagina WHERE Nombre = N'Estadisticas')
BEGIN
    INSERT INTO Pagina (Nombre, NombreVisible, Descripcion, Tipo, Direccion, PermisosPadreId, Logo, OrdenB, Estatus)
    VALUES (N'Estadisticas', N'Estadísticas', N'KPIs de desempeño del equipo de soporte', N'Menu', N'/Estadisticas', NULL, N'fas fa-chart-pie', 10, 1);
END
GO

-- ============================================================
-- 2. Seed RolPaginaAccion SOLO roles Administrador y Supervisor.
--    Solo lectura: PuedeLeer=1, resto 0. Idempotente (patrón MisActivos). (MD-002)
-- ============================================================
INSERT INTO RolPaginaAccion (RolId, PaginaId, PuedeLeer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, CreadoPor, FechaCreacion, Estatus)
SELECT r.Id, p.Id, 1, 0, 0, 0, 0, N'migracion', GETDATE(), 1
FROM Rol r CROSS JOIN Pagina p
WHERE p.Nombre = N'Estadisticas' AND r.Nombre IN (N'Administrador', N'Supervisor') AND r.Estatus = 1
  AND NOT EXISTS (SELECT 1 FROM RolPaginaAccion rpa WHERE rpa.RolId = r.Id AND rpa.PaginaId = p.Id);
GO

-- ============================================================
-- 3. SP ObtenerMetricasResumen (1 fila: 8 KPI). (MD-005, MD-006, MD-012)
--    Conteos = tickets creados en rango (Ticket.FechaCreacion), Estatus=1,
--    agrupados por TicketEstatusId actual (1=Nuevo,2=En Progreso,3=Resuelto,
--    4=Rechazado,5=Cerrado). Eficiencia = Cerrados/Total*100 (1 decimal).
--    HorasPromedioResolucion = horas hábiles usando EmpresaHorarioLaboral
--    (day-walk; fallback Lun-Vie 09:00-17:00 si la empresa no tiene filas).
-- ============================================================
IF OBJECT_ID(N'dbo.ObtenerMetricasResumen', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerMetricasResumen;
GO
CREATE PROCEDURE [dbo].[ObtenerMetricasResumen]
    @Usuario NVARCHAR(25),
    @FechaInicio DATETIME,
    @FechaFin DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;

    IF @EmpresaId IS NULL
    BEGIN
        SELECT 0 AS Total, 0 AS Nuevos, 0 AS EnProgreso, 0 AS Resueltos, 0 AS Cerrados, 0 AS Rechazados,
               CAST(0 AS decimal(5,1)) AS Eficiencia, CAST(0 AS decimal(10,1)) AS HorasPromedioResolucion;
        RETURN;
    END

    -- 1. Conteos por estatus actual (tickets creados en rango, tenant-safe).
    DECLARE @Total INT, @Nuevos INT, @EnProgreso INT, @Resueltos INT, @Cerrados INT, @Rechazados INT;
    SELECT
        @Total = COUNT(*),
        @Nuevos = ISNULL(SUM(CASE WHEN TicketEstatusId = 1 THEN 1 ELSE 0 END), 0),
        @EnProgreso = ISNULL(SUM(CASE WHEN TicketEstatusId = 2 THEN 1 ELSE 0 END), 0),
        @Resueltos = ISNULL(SUM(CASE WHEN TicketEstatusId = 3 THEN 1 ELSE 0 END), 0),
        @Rechazados = ISNULL(SUM(CASE WHEN TicketEstatusId = 4 THEN 1 ELSE 0 END), 0),
        @Cerrados = ISNULL(SUM(CASE WHEN TicketEstatusId = 5 THEN 1 ELSE 0 END), 0)
    FROM Ticket
    WHERE EmpresaId = @EmpresaId AND Estatus = 1
      AND FechaCreacion >= @FechaInicio AND FechaCreacion < DATEADD(day, 1, @FechaFin);

    DECLARE @Eficiencia decimal(5,1) = ISNULL(ROUND(@Cerrados * 100.0 / NULLIF(@Total, 0), 1), 0);

    -- 2. Horario laboral (solo días laborables con ventana válida).
    --    Fallback Lun-Vie 09:00-17:00 si la empresa no tiene filas (documentado).
    CREATE TABLE #Horario (DiaSemana tinyint NOT NULL, HoraInicio time NOT NULL, HoraFin time NOT NULL);

    INSERT INTO #Horario (DiaSemana, HoraInicio, HoraFin)
    SELECT DiaSemana, CAST(HoraInicio AS time), CAST(HoraFin AS time)
    FROM EmpresaHorarioLaboral
    WHERE EmpresaId = @EmpresaId AND Estatus = 1 AND HoraInicio IS NOT NULL AND HoraFin IS NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM #Horario)
    BEGIN
        INSERT INTO #Horario (DiaSemana, HoraInicio, HoraFin) VALUES
            (1, '09:00', '17:00'), (2, '09:00', '17:00'), (3, '09:00', '17:00'),
            (4, '09:00', '17:00'), (5, '09:00', '17:00');
    END

    -- 3. Horas hábiles por ticket resuelto en rango (última fila 'Resolver').
    --    Day-walk: por cada intervalo [Ticket.FechaCreacion -> última Resolver],
    --    recorre cada día d y suma la intersección con la ventana del día laborable.
    --    Día laborable = d con fila en #Horario (DiaSemana ISO, independiente de @@DATEFIRST).
    --    Timestamps = GETDATE() (zona del servidor), limitación heredada documentada.
    DECLARE @HorasPromedioResolucion decimal(10,1) = 0;

    ;WITH E(n) AS (
        SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
        UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9
    ),
    Resolver AS (
        SELECT TicketId, FechaCreacion AS FechaFin,
               ROW_NUMBER() OVER (PARTITION BY TicketId ORDER BY FechaCreacion DESC, Id DESC) AS rn
        FROM TicketAsignacion
        WHERE EmpresaId = @EmpresaId AND Estatus = 1 AND TipoMovimiento = 'Resolver'
          AND FechaCreacion >= @FechaInicio AND FechaCreacion < DATEADD(day, 1, @FechaFin)
    ),
    Intervalos AS (
        SELECT t.FechaCreacion AS FechaInicio, r.FechaFin
        FROM Resolver r
        INNER JOIN Ticket t ON t.Id = r.TicketId AND t.Estatus = 1 AND t.EmpresaId = @EmpresaId
        WHERE r.rn = 1
    )
    SELECT @HorasPromedioResolucion = ISNULL(
        ROUND(
            SUM(horas.Horas) / NULLIF(COUNT(*), 0),
            1
        ), 0)
    FROM Intervalos i
    CROSS APPLY (
        SELECT SUM(
            CASE
                WHEN dh.Fin > dh.Inicio THEN DATEDIFF(MINUTE, dh.Inicio, dh.Fin) / 60.0
                ELSE 0
            END
        ) AS Horas
        FROM (
            SELECT DATEADD(day, d.n, CAST(i.FechaInicio AS date)) AS Dia
            FROM (SELECT a.n + 10*b.n + 100*c.n + 1000*d.n AS n FROM E a, E b, E c, E d) d
            WHERE d.n <= DATEDIFF(day, i.FechaInicio, i.FechaFin)
        ) dia
        INNER JOIN #Horario h
            -- ISO weekday (1=Lun..7=Dom): ((DATEPART(weekday, d) + @@DATEFIRST - 2) % 7) + 1
            ON h.DiaSemana = ((DATEPART(weekday, dia.Dia) + @@DATEFIRST - 2) % 7) + 1
        CROSS APPLY (
            SELECT
                CASE WHEN i.FechaInicio > DATEADD(day, DATEDIFF(day, 0, dia.Dia), CAST(h.HoraInicio AS datetime))
                     THEN i.FechaInicio
                     ELSE DATEADD(day, DATEDIFF(day, 0, dia.Dia), CAST(h.HoraInicio AS datetime))
                END AS Inicio,
                CASE WHEN i.FechaFin < DATEADD(day, DATEDIFF(day, 0, dia.Dia), CAST(h.HoraFin AS datetime))
                     THEN i.FechaFin
                     ELSE DATEADD(day, DATEDIFF(day, 0, dia.Dia), CAST(h.HoraFin AS datetime))
                END AS Fin
        ) dh
    ) horas;

    DROP TABLE #Horario;

    SELECT
        @Total AS Total,
        @Nuevos AS Nuevos,
        @EnProgreso AS EnProgreso,
        @Resueltos AS Resueltos,
        @Cerrados AS Cerrados,
        @Rechazados AS Rechazados,
        @Eficiencia AS Eficiencia,
        @HorasPromedioResolucion AS HorasPromedioResolucion;
END
GO

-- ============================================================
-- 4. SP ObtenerDistribucionEstatus (5 filas, pie de estatus). (MD-007)
--    Todas las TicketEstatus (Estatus=1) con Cantidad=0 si no hay tickets.
-- ============================================================
IF OBJECT_ID(N'dbo.ObtenerDistribucionEstatus', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerDistribucionEstatus;
GO
CREATE PROCEDURE [dbo].[ObtenerDistribucionEstatus]
    @Usuario NVARCHAR(25),
    @FechaInicio DATETIME,
    @FechaFin DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;

    SELECT te.Id AS EstatusId, te.Nombre, te.Color, ISNULL(COUNT(t.Id), 0) AS Cantidad
    FROM TicketEstatus te
    LEFT JOIN Ticket t
        ON t.TicketEstatusId = te.Id AND t.Estatus = 1 AND t.EmpresaId = @EmpresaId
       AND t.FechaCreacion >= @FechaInicio AND t.FechaCreacion < DATEADD(day, 1, @FechaFin)
    WHERE te.Estatus = 1
    GROUP BY te.Id, te.Nombre, te.Color, te.Orden
    ORDER BY te.Orden;
END
GO

-- ============================================================
-- 5. SP ObtenerEvolucionDiaria (1 fila/día del rango). (MD-008)
--    Serie de días continua; Creados = Ticket.FechaCreacion por día;
--    Resueltos = TicketAsignacion.TipoMovimiento='Resolver' por día.
-- ============================================================
IF OBJECT_ID(N'dbo.ObtenerEvolucionDiaria', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerEvolucionDiaria;
GO
CREATE PROCEDURE [dbo].[ObtenerEvolucionDiaria]
    @Usuario NVARCHAR(25),
    @FechaInicio DATETIME,
    @FechaFin DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;

    ;WITH E(n) AS (
        SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
        UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9
    ),
    Dias AS (
        SELECT DATEADD(day, d.n, CAST(@FechaInicio AS date)) AS Dia
        FROM (SELECT a.n + 10*b.n + 100*c.n + 1000*d.n AS n FROM E a, E b, E c, E d) d
        WHERE d.n <= DATEDIFF(day, @FechaInicio, @FechaFin)
    )
    SELECT
        d.Dia AS Fecha,
        ISNULL((SELECT COUNT(*) FROM Ticket t
                WHERE t.EmpresaId = @EmpresaId AND t.Estatus = 1
                  AND t.FechaCreacion >= d.Dia AND t.FechaCreacion < DATEADD(day, 1, d.Dia)), 0) AS Creados,
        ISNULL((SELECT COUNT(DISTINCT ta.TicketId) FROM TicketAsignacion ta
                WHERE ta.EmpresaId = @EmpresaId AND ta.Estatus = 1 AND ta.TipoMovimiento = 'Resolver'
                  AND ta.FechaCreacion >= d.Dia AND ta.FechaCreacion < DATEADD(day, 1, d.Dia)), 0) AS Resueltos
    FROM Dias d
    ORDER BY d.Dia;
END
GO

-- ============================================================
-- 6. SP ObtenerRankingAreas (todas las áreas con tickets, Total DESC). (MD-009)
-- ============================================================
IF OBJECT_ID(N'dbo.ObtenerRankingAreas', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerRankingAreas;
GO
CREATE PROCEDURE [dbo].[ObtenerRankingAreas]
    @Usuario NVARCHAR(25),
    @FechaInicio DATETIME,
    @FechaFin DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;

    SELECT
        a.Id AS AreaId,
        a.Nombre AS AreaNombre,
        COUNT(*) AS Total,
        ROUND(AVG(CAST(t.Urgencia AS decimal(5,1))), 1) AS PromUrgencia,
        ISNULL(SUM(CASE WHEN t.TicketEstatusId = 5 THEN 1 ELSE 0 END), 0) AS Cerrados,
        ISNULL(SUM(CASE WHEN t.TicketEstatusId = 4 THEN 1 ELSE 0 END), 0) AS Rechazados
    FROM Ticket t
    INNER JOIN Area a ON a.Id = t.AreaId AND a.Estatus = 1
    WHERE t.EmpresaId = @EmpresaId AND t.Estatus = 1
      AND t.FechaCreacion >= @FechaInicio AND t.FechaCreacion < DATEADD(day, 1, @FechaFin)
    GROUP BY a.Id, a.Nombre
    ORDER BY Total DESC;
END
GO

-- ============================================================
-- 7. SP ObtenerRankingReasignaciones (TOP N por Tipo). (MD-010)
--    Reciben = destino de 'Reasignar'; Quitan = agente anterior (LAG) de 'Reasignar'.
-- ============================================================
IF OBJECT_ID(N'dbo.ObtenerRankingReasignaciones', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerRankingReasignaciones;
GO
CREATE PROCEDURE [dbo].[ObtenerRankingReasignaciones]
    @Usuario NVARCHAR(25),
    @FechaInicio DATETIME,
    @FechaFin DATETIME,
    @TopN INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;

    ;WITH Movs AS (
        SELECT TicketId, UsuarioId, TipoMovimiento, FechaCreacion,
               LAG(UsuarioId) OVER (PARTITION BY TicketId ORDER BY FechaCreacion, Id) AS AgenteAnterior
        FROM TicketAsignacion
        WHERE Estatus = 1 AND EmpresaId = @EmpresaId
    ),
    Reciben AS (
        SELECT u.Id AS UsuarioId, u.Nombre, u.Apellido, u.NombreUsuario, 'Reciben' AS Tipo, COUNT(*) AS Cantidad
        FROM Movs m
        INNER JOIN Usuarios u ON u.Id = m.UsuarioId
        WHERE m.TipoMovimiento = 'Reasignar'
          AND m.FechaCreacion >= @FechaInicio AND m.FechaCreacion < DATEADD(day, 1, @FechaFin)
        GROUP BY u.Id, u.Nombre, u.Apellido, u.NombreUsuario
    ),
    Quitan AS (
        SELECT u.Id AS UsuarioId, u.Nombre, u.Apellido, u.NombreUsuario, 'Quitan' AS Tipo, COUNT(*) AS Cantidad
        FROM Movs m
        INNER JOIN Usuarios u ON u.Id = m.AgenteAnterior
        WHERE m.TipoMovimiento = 'Reasignar'
          AND m.FechaCreacion >= @FechaInicio AND m.FechaCreacion < DATEADD(day, 1, @FechaFin)
        GROUP BY u.Id, u.Nombre, u.Apellido, u.NombreUsuario
    ),
    Combinado AS (
        SELECT UsuarioId, Nombre, Apellido, NombreUsuario, Tipo, Cantidad FROM Reciben
        UNION ALL
        SELECT UsuarioId, Nombre, Apellido, NombreUsuario, Tipo, Cantidad FROM Quitan
    ),
    Ranking AS (
        SELECT Tipo, UsuarioId, Nombre, Apellido, NombreUsuario, Cantidad,
               ROW_NUMBER() OVER (PARTITION BY Tipo ORDER BY Cantidad DESC, UsuarioId) AS rn
        FROM Combinado
    )
    SELECT Tipo, UsuarioId, Nombre, Apellido, NombreUsuario, Cantidad
    FROM Ranking
    WHERE rn <= @TopN
    ORDER BY CASE Tipo WHEN 'Reciben' THEN 0 ELSE 1 END, Cantidad DESC;
END
GO
