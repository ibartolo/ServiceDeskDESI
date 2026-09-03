-- ============================================================
-- Hotfix: limpiar PersonaActivo huérfano + FKs de integridad referencial
-- ----------------------------------------------------------------------------
-- Problema: 3 filas de PersonaActivo apuntaban a PersonaId=1 (persona inexistente),
-- lo que hacía que los activos parecieran "sin asignar" en el catálogo pero no
-- aparecieran como disponibles en el modal.
-- ============================================================

-- 1. Eliminar asignaciones huérfanas (PersonaId que ya no existe en Persona)
DELETE FROM dbo.PersonaActivo
WHERE PersonaId NOT IN (SELECT Id FROM dbo.Persona);

-- 2. Foreign keys que validan la relación y evitan borrados físicos
--    que dejarían registros huérfanos (ON DELETE NO ACTION por defecto).
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PersonaActivo_Persona')
    ALTER TABLE dbo.PersonaActivo
        ADD CONSTRAINT FK_PersonaActivo_Persona FOREIGN KEY (PersonaId) REFERENCES dbo.Persona(Id);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PersonaActivo_Activo')
    ALTER TABLE dbo.PersonaActivo
        ADD CONSTRAINT FK_PersonaActivo_Activo FOREIGN KEY (ActivoId) REFERENCES dbo.Activo(Id);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PersonaActivo_Empresa')
    ALTER TABLE dbo.PersonaActivo
        ADD CONSTRAINT FK_PersonaActivo_Empresa FOREIGN KEY (EmpresaId) REFERENCES dbo.Empresa(Id);
