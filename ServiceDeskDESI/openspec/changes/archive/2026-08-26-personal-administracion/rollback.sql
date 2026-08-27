-- ============================================================
-- Rollback: personal-administracion (respaldo / deshacer)
-- Ejecutar SOLO si se requiere revertir la migración.
-- ============================================================

-- 1. Revertir las 2 etiquetas renombradas (conservador: solo los 2 valores, NO se elimina la columna)
UPDATE [dbo].[Pagina] SET [NombreVisible] = 'Personas' WHERE [Nombre] = 'Personas';
GO
UPDATE [dbo].[Pagina] SET [NombreVisible] = 'Usuarios' WHERE [Nombre] = 'Usuarios';
GO

-- 2. (Opcional) Eliminar la columna si se decide revertir por completo:
-- IF COL_LENGTH('dbo.Pagina','NombreVisible') IS NOT NULL
--     ALTER TABLE [dbo].[Pagina] DROP COLUMN [NombreVisible];
-- GO
