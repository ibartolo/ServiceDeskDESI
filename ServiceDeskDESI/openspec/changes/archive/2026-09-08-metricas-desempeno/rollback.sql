-- ============================================================
-- Rollback: metricas-desempeno
-- Orden inverso a migration.sql. Idempotente (guards IF EXISTS).
-- ⚠️ Solo para revertir la migración de este change en la BD hosted (manual).
-- ============================================================

-- 1. DROP los 5 SPs
IF OBJECT_ID(N'dbo.ObtenerMetricasResumen', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerMetricasResumen;
GO
IF OBJECT_ID(N'dbo.ObtenerDistribucionEstatus', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerDistribucionEstatus;
GO
IF OBJECT_ID(N'dbo.ObtenerEvolucionDiaria', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerEvolucionDiaria;
GO
IF OBJECT_ID(N'dbo.ObtenerRankingAreas', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerRankingAreas;
GO
IF OBJECT_ID(N'dbo.ObtenerRankingReasignaciones', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerRankingReasignaciones;
GO

-- 2. Quitar RolPaginaAccion / Pagina "Estadisticas"
DELETE rpa
FROM RolPaginaAccion rpa
INNER JOIN Pagina p ON rpa.PaginaId = p.Id
WHERE p.Nombre = N'Estadisticas';
GO

DELETE FROM Pagina WHERE Nombre = N'Estadisticas';
GO
