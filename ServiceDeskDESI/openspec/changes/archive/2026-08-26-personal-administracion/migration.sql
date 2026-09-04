-- ============================================================
-- Migration: personal-administracion
-- Renombrar menús "Personas" -> "Personal" y "Usuarios" -> "Administración"
-- Migración idempotente. Ejecutar contra la BD viva (hosted).
-- Fecha: 2026-08-26
-- ============================================================

-- 1. Añadir columna NombreVisible (nullable) si no existe
IF COL_LENGTH('dbo.Pagina','NombreVisible') IS NULL
    ALTER TABLE [dbo].[Pagina] ADD [NombreVisible] NVARCHAR(250) NULL;
GO

-- 2. Backfill: NombreVisible = Nombre para filas existentes
UPDATE [dbo].[Pagina] SET [NombreVisible] = [Nombre] WHERE [NombreVisible] IS NULL;
GO

-- 3. Renombrar las 2 filas objetivo (por llave Nombre)
UPDATE [dbo].[Pagina] SET [NombreVisible] = 'Personal' WHERE [Nombre] = 'Personas';
GO
UPDATE [dbo].[Pagina] SET [NombreVisible] = 'Administración' WHERE [Nombre] = 'Usuarios';
GO
