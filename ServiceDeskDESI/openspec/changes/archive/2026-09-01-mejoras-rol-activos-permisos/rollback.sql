-- ============================================================
-- Rollback: mejoras-rol-activos-permisos (G1 — Base de datos)
-- Orden inverso a migration.sql.
--   (1) DROP de los 3 SPs nuevos
--   (2) DROP TABLE Mantenimiento
--   (3) DROP INDEX UX_Activo_EmpresaSerial
--   (4) DROP COLUMN Activo.SerieLocal
--   (5) Restaurar definición previa de GuardarOActualizarActivo
-- Base de datos: db_9c7990_servicedeskdesi.
-- Cada DROP con guard IF EXISTS. Fecha: 2026-08-31
-- ============================================================

-- 1. DROP de los 3 SPs nuevos
IF OBJECT_ID(N'dbo.GuardarMantenimiento', N'P') IS NOT NULL DROP PROCEDURE dbo.GuardarMantenimiento;
GO
IF OBJECT_ID(N'dbo.ObtenerMantenimientosPorActivo', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerMantenimientosPorActivo;
GO
IF OBJECT_ID(N'dbo.ObtenerConteoPaginasPorRol', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerConteoPaginasPorRol;
GO

-- 2. DROP tabla Mantenimiento
IF OBJECT_ID(N'dbo.Mantenimiento', N'U') IS NOT NULL DROP TABLE dbo.Mantenimiento;
GO

-- 3. DROP índice único filtrado
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = N'UX_Activo_EmpresaSerial'
             AND object_id = OBJECT_ID(N'dbo.Activo'))
    DROP INDEX UX_Activo_EmpresaSerial ON dbo.Activo;
GO

-- 4. DROP columna SerieLocal
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID(N'dbo.Activo') AND name = N'SerieLocal')
    ALTER TABLE dbo.Activo DROP COLUMN SerieLocal;
GO

-- 5. Restaurar definición previa de GuardarOActualizarActivo
--    (versión sin @SerieLocal, sin normalización NULLIF ni chequeo -2;
--    igual a la versión vigente antes de este cambio — fk-escalares).
IF OBJECT_ID(N'dbo.GuardarOActualizarActivo', N'P') IS NOT NULL DROP PROCEDURE dbo.GuardarOActualizarActivo;
GO
CREATE PROCEDURE [dbo].[GuardarOActualizarActivo]
(
    @Id BIGINT,
    @Nombre NVARCHAR(50),
    @Descripcion NVARCHAR(250) = NULL,
    @TipoActivoId BIGINT,
    @Serial NVARCHAR(50) = NULL,
    @MarcaId BIGINT,
    @ModeloId BIGINT,
    @Notas NVARCHAR(250) = NULL,
    @FechaCompra DATETIME = NULL,
    @CreadoPor NVARCHAR(25),
    @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL,
    @FechaModificacion DATETIME = NULL,
    @Estatus BIT,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF EXISTS(SELECT * FROM Activo WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(
            SELECT 1
            FROM Activo a
            WHERE a.Id = @Id
                AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM TipoActivo ta
            WHERE ta.Id = @TipoActivoId
                AND ta.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM Marca m
            WHERE m.Id = @MarcaId
                AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM Modelo mo
            WHERE mo.Id = @ModeloId
                AND mo.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        UPDATE Activo
        SET Nombre = @Nombre,
            Descripcion = @Descripcion,
            TipoActivoID = @TipoActivoId,
            Serial = @Serial,
            MarcaID = @MarcaId,
            ModeloID = @ModeloId,
            Notas = @Notas,
            FechaCompra = @FechaCompra,
            ModificadoPor = @ModificadoPor,
            FechaModificacion = @FechaModificacion,
            Estatus = @Estatus
        WHERE Id = @Id

        SELECT @Id
    END
    ELSE
    BEGIN
        IF NOT EXISTS(
            SELECT 1
            FROM Usuarios
            WHERE NombreUsuario = @CreadoPor
                AND EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
                AND Estatus = 1
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM TipoActivo ta
            WHERE ta.Id = @TipoActivoId
                AND ta.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM Marca m
            WHERE m.Id = @MarcaId
                AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM Modelo mo
            WHERE mo.Id = @ModeloId
                AND mo.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        INSERT INTO Activo
        (Nombre, Descripcion, TipoActivoID, Serial, MarcaID, ModeloID, Notas, FechaCompra,
         CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES
        (@Nombre, @Descripcion, @TipoActivoId, @Serial, @MarcaId, @ModeloId, @Notas, @FechaCompra,
         @CreadoPor, @FechaCreacion, @Estatus, (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))

        SELECT SCOPE_IDENTITY()
    END
END
GO
