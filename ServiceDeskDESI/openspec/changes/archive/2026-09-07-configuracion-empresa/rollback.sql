-- ============================================================
-- Rollback: configuracion-empresa
-- Orden inverso a migration.sql.
-- ⚠️ Solo para revertir la migración de este change en la BD hosted (manual).
-- ============================================================

-- 1. DROP SPs
IF OBJECT_ID(N'dbo.ObtenerHorarioLaboral', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerHorarioLaboral;
GO
IF OBJECT_ID(N'dbo.GuardarHorarioLaboralDia', N'P') IS NOT NULL DROP PROCEDURE dbo.GuardarHorarioLaboralDia;
GO
IF OBJECT_ID(N'dbo.GuardarLogoEmpresa', N'P') IS NOT NULL DROP PROCEDURE dbo.GuardarLogoEmpresa;
GO

-- 2. Quitar RolPaginaAccion / Pagina "ConfiguracionEmpresa"
DELETE rpa
FROM RolPaginaAccion rpa
INNER JOIN Pagina p ON rpa.PaginaId = p.Id
WHERE p.Nombre = N'ConfiguracionEmpresa';
GO

DELETE FROM Pagina WHERE Nombre = N'ConfiguracionEmpresa';
GO

-- 3. DROP tabla EmpresaHorarioLaboral (su FK cae con la tabla)
IF OBJECT_ID(N'dbo.EmpresaHorarioLaboral', N'U') IS NOT NULL DROP TABLE dbo.EmpresaHorarioLaboral;
GO

-- 4. DROP columna Empresa.LogoUrl
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Empresa') AND name = N'LogoUrl')
    ALTER TABLE dbo.Empresa DROP COLUMN LogoUrl;
GO
