-- ============================================================
-- Rollback: asignacion-activos (notificación, confirmación y bitácora)
-- Orden inverso a migration.sql. NO toca los 5 SPs existentes.
-- ============================================================

-- 1. DROP de los 3 SPs nuevos
IF OBJECT_ID(N'dbo.GenerarTokenConfirmacion', N'P') IS NOT NULL DROP PROCEDURE dbo.GenerarTokenConfirmacion;
GO
IF OBJECT_ID(N'dbo.ConfirmarRecepcionActivo', N'P') IS NOT NULL DROP PROCEDURE dbo.ConfirmarRecepcionActivo;
GO
IF OBJECT_ID(N'dbo.RegistrarBitacoraCorreo', N'P') IS NOT NULL DROP PROCEDURE dbo.RegistrarBitacoraCorreo;
GO

-- 2. DROP índice no único
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = N'IX_PersonaActivo_TokenConfirmacion'
             AND object_id = OBJECT_ID(N'dbo.PersonaActivo'))
    DROP INDEX IX_PersonaActivo_TokenConfirmacion ON dbo.PersonaActivo;
GO

-- 3. DROP tabla BitacoraCorreo
IF OBJECT_ID(N'dbo.BitacoraCorreo', N'U') IS NOT NULL DROP TABLE dbo.BitacoraCorreo;
GO

-- 4. DROP columnas (guard IF EXISTS en sys.columns)
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID(N'dbo.PersonaActivo') AND name = N'TokenConfirmacion')
    ALTER TABLE dbo.PersonaActivo DROP COLUMN TokenConfirmacion;
GO
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID(N'dbo.PersonaActivo') AND name = N'FechaConfirmacion')
    ALTER TABLE dbo.PersonaActivo DROP COLUMN FechaConfirmacion;
GO
