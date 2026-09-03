/* ============================================================================
   ServiceDeskDESI — Limpieza total de datos para rondas de pruebas
   ----------------------------------------------------------------------------
   BORRA los datos de TODAS las tablas y REINICIA los IDENTITY (RESEED a 0;
   el siguiente registro insertado empezará en 1).

   Se CONSERVAN (tablas GLOBALES de la aplicación):
     - dbo.Pagina          -> páginas/menús del sistema
     - dbo.TicketEstatus   -> estatus de tickets (catálogo global)
     - dbo.PlantillaRol    -> plantillas de rol usadas al registrar empresa

   Se BORRAN (datos por empresa / transaccionales): Empresa, Usuarios, Roles,
   catálogos operativos (TipoActivo, Marca, Modelo, Area, Categoria, Sucursal,
   Puesto, Compania, Persona, ...), Tickets, evidencias, Foliador, permisos.

   >>> PRECAUCIONES <<<
   1. Verifica que apuntas a la BD correcta (la de PRUEBAS).
   2. Haz un backup antes: esto NO se puede deshacer.
   3. Si quieres CONSERVAR la empresa de prueba y sus usuarios/roles ya
      registrados, agrégalas al bloque @tablasGlobales (viene comentado).
   4. Las evidencias (TicketEvidencia) guardan archivos en App_Data del WebApi:
      borrar la tabla NO borra los archivos físicos.
   5. Si la BD está recién creada, las tablas globales pueden estar vacías:
      revisa que Pagina / TicketEstatus / PlantillaRol tengan sus registros
      semilla (el seed de PlantillaRol está en openspec/basededatosservicedesk.txt).
   ============================================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- ------------------------------------------------------------
-- 0) Tablas GLOBALES que NO se tocan
-- ------------------------------------------------------------
DECLARE @tablasGlobales TABLE (Nombre SYSNAME NOT NULL PRIMARY KEY);
INSERT INTO @tablasGlobales (Nombre) VALUES
    (N'Pagina'),
    (N'TicketEstatus'),
    (N'PlantillaRol');
-- Opcional: si quieres conservar la empresa/usuario/roles ya registrados,
-- descomenta las que necesites (separadas por coma):
--     ,(N'Empresa')
--     ,(N'Usuarios')
--     ,(N'Rol')
--     ,(N'RolPaginaAccion')
--     ,(N'UsuarioRol')

DECLARE @sql NVARCHAR(MAX);

-- ------------------------------------------------------------
-- 1) Deshabilitar TODAS las FKs (evita problemas de orden de borrado)
-- ------------------------------------------------------------
SELECT @sql = NULL;
SELECT @sql = CONCAT(@sql, N'ALTER TABLE ',
       QUOTENAME(SCHEMA_NAME(t.schema_id)), N'.', QUOTENAME(t.name),
       N' NOCHECK CONSTRAINT ALL;', NCHAR(10))
FROM sys.tables t
WHERE t.is_ms_shipped = 0;
EXEC sp_executesql @sql;
PRINT N'1) FKs deshabilitadas.';

-- ------------------------------------------------------------
-- 2) DELETE + RESEED de IDENTITY por tabla (excepto globales)
-- ------------------------------------------------------------
DECLARE @schema SYSNAME, @tabla SYSNAME, @nombreCompleto NVARCHAR(300);

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT SCHEMA_NAME(t.schema_id), t.name
    FROM sys.tables t
    WHERE t.is_ms_shipped = 0
      AND NOT EXISTS (SELECT 1 FROM @tablasGlobales g WHERE g.Nombre = t.name)
    ORDER BY t.name;

OPEN cur;
FETCH NEXT FROM cur INTO @schema, @tabla;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @nombreCompleto = QUOTENAME(@schema) + N'.' + QUOTENAME(@tabla);

    -- 2.1 Borrar filas
    SET @sql = N'DELETE FROM ' + @nombreCompleto + N';';
    EXEC sp_executesql @sql;

    -- 2.2 Reiniciar identity a 0 (solo si la tabla tiene columna IDENTITY)
    IF EXISTS (SELECT 1 FROM sys.identity_columns ic
               WHERE ic.object_id = OBJECT_ID(@nombreCompleto))
    BEGIN
        SET @sql = N'DBCC CHECKIDENT (''' + @nombreCompleto + N''', RESEED, 0) WITH NO_INFOMSGS;';
        EXEC sp_executesql @sql;
    END

    PRINT N'   Limpiada: ' + @nombreCompleto;
    FETCH NEXT FROM cur INTO @schema, @tabla;
END;
CLOSE cur;
DEALLOCATE cur;

-- ------------------------------------------------------------
-- 3) Rehabilitar y VALIDAR las FKs
-- ------------------------------------------------------------
SELECT @sql = NULL;
SELECT @sql = CONCAT(@sql, N'ALTER TABLE ',
       QUOTENAME(SCHEMA_NAME(t.schema_id)), N'.', QUOTENAME(t.name),
       N' WITH CHECK CHECK CONSTRAINT ALL;', NCHAR(10))
FROM sys.tables t
WHERE t.is_ms_shipped = 0;
EXEC sp_executesql @sql;
PRINT N'3) FKs rehabilitadas y validadas.';

-- ------------------------------------------------------------
-- 4) Resumen: tablas globales conservadas y sus filas
-- ------------------------------------------------------------
SELECT t.name AS TablaGlobal, SUM(p.rows) AS FilasConservadas
FROM sys.tables t
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
WHERE t.name IN (SELECT Nombre FROM @tablasGlobales)
GROUP BY t.name
ORDER BY t.name;
