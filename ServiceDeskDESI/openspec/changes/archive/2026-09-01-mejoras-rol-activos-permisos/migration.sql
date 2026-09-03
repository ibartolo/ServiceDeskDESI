-- ============================================================
-- Migration: mejoras-rol-activos-permisos
-- Mejoras de Roles, Activos y Permisos (G1 — Base de datos).
--   (1) SerieLocal en Activo           (ítem 3 / CAM-001, CAM-002)
--   (2) Serial único por empresa       (ítem 2 / SUA-001..003)
--   (3) Tabla Mantenimiento            (ítem 4 / MTA-001, MTA-004, MTA-005)
--   (4) Rewrite GuardarOActualizarActivo (+@SerieLocal, NULLIF serial, retorno -2)
--   (5) SP GuardarMantenimiento        (MTA-001, MTA-004)
--   (6) SP ObtenerMantenimientosPorActivo (MTA-003, MTA-004, MTA-005)
--   (7) SP ObtenerConteoPaginasPorRol  (ítem 6)
-- Base de datos: db_9c7990_servicedeskdesi (esquema real hosted).
-- Idempotente: guards IF NOT EXISTS / IF OBJECT_ID(...) IS NULL.
-- Fecha: 2026-08-31
-- ============================================================

-- 1. SerieLocal (ítem 3) — texto libre, no único.
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.Activo') AND name = N'SerieLocal')
    ALTER TABLE dbo.Activo ADD SerieLocal NVARCHAR(100) NULL;
GO

-- 2. Índice único filtrado por empresa (ítem 2).
--    Unicidad de Serial por empresa entre activos VIGENTES (Estatus = 1)
--    y seriales NO nulos. Los seriales nulos quedan permitidos; el
--    soft-delete (Estatus = 0) libera el serial (SUA-001..003).
--
--    PRECAUCION (dedup legacy — REQUERIDO ANTES de crear el índice):
--    Si ya existen duplicados legacy (dos o más activos vigentes con el
--    mismo Serial en la misma empresa), el CREATE UNIQUE INDEX fallará.
--    Ejecutar primero este diagnóstico y depurar los duplicados:
--
--        SELECT EmpresaId, Serial, COUNT(*) AS Veces
--        FROM dbo.Activo
--        WHERE Serial IS NOT NULL AND Estatus = 1
--        GROUP BY EmpresaId, Serial
--        HAVING COUNT(*) > 1;
--
--    Opciones de depuración: anexar un sufijo al Serial duplicado,
--    normalizar los seriales vacíos a NULL, o marcar Estatus = 0 los
--    registros obsoletos. Re-ejecutar el diagnóstico hasta que no
--    devuelva filas, y solo entonces aplicar este CREATE UNIQUE INDEX.
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_Activo_EmpresaSerial'
                 AND object_id = OBJECT_ID(N'dbo.Activo'))
    CREATE UNIQUE INDEX UX_Activo_EmpresaSerial ON dbo.Activo (EmpresaId, Serial)
        WHERE Serial IS NOT NULL AND Estatus = 1;
GO

-- 3. Tabla Mantenimiento (ítem 4) — patrón PersonaActivo + tenant EmpresaId.
IF OBJECT_ID(N'dbo.Mantenimiento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Mantenimiento (
        Id                BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ActivoId          BIGINT NOT NULL,
        Comentario        NVARCHAR(500) NOT NULL,
        Fecha             DATETIME NOT NULL,          -- auto GETDATE()
        CreadoPor         NVARCHAR(25) NOT NULL,
        FechaCreacion     DATETIME NOT NULL,
        ModificadoPor     NVARCHAR(25) NULL,
        FechaModificacion DATETIME NULL,
        Estatus           BIT NOT NULL CONSTRAINT DF_Mantenimiento_Estatus DEFAULT (1),
        EmpresaId         BIGINT NOT NULL,
        CONSTRAINT FK_Mantenimiento_Activo FOREIGN KEY (ActivoId) REFERENCES dbo.Activo (Id)
    );
END
GO

-- 4. Rewrite GuardarOActualizarActivo (DROP/CREATE).
--    Cambios: (a) +@SerieLocal NVARCHAR(100) = NULL en firma y en UPDATE/INSERT;
--    (b) normalización de serial vacío -> NULL (D3) ANTES del chequeo de duplicado;
--    (c) chequeo de duplicado de Serial -> retorno -2, ANTES de las validaciones tenant.
--    Orden de validación: (1) duplicado -2, (2) tenant 0, (3) UPDATE/INSERT.
IF OBJECT_ID(N'dbo.GuardarOActualizarActivo', N'P') IS NOT NULL DROP PROCEDURE dbo.GuardarOActualizarActivo;
GO
CREATE PROCEDURE [dbo].[GuardarOActualizarActivo]
(
    @Id BIGINT,
    @Nombre NVARCHAR(50),
    @Descripcion NVARCHAR(250) = NULL,
    @TipoActivoId BIGINT,
    @Serial NVARCHAR(50) = NULL,
    @SerieLocal NVARCHAR(100) = NULL,
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
    SET NOCOUNT ON;

    -- D3: normalizar serial vacío a NULL (el índice filtra Serial IS NOT NULL).
    SET @Serial = NULLIF(LTRIM(RTRIM(@Serial)), '');

    -- (1) Duplicado de Serial: activos vigentes, misma empresa, excluyendo el Id actual.
    IF @Serial IS NOT NULL AND EXISTS (
        SELECT 1 FROM Activo
        WHERE Serial = @Serial
          AND Estatus = 1
          AND Id <> @Id
          AND EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    )
    BEGIN
        SELECT -2;
        RETURN;
    END

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
            SerieLocal = @SerieLocal,
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
        (Nombre, Descripcion, TipoActivoID, Serial, SerieLocal, MarcaID, ModeloID, Notas, FechaCompra,
         CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES
        (@Nombre, @Descripcion, @TipoActivoId, @Serial, @SerieLocal, @MarcaId, @ModeloId, @Notas, @FechaCompra,
         @CreadoPor, @FechaCreacion, @Estatus, (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))

        SELECT SCOPE_IDENTITY()
    END
END
GO

-- 5. SP GuardarMantenimiento — Fecha = GETDATE(), Estatus = 1, EmpresaId derivada de @Usuario.
--    Valida que el ActivoId pertenezca a la empresa de la sesión (si no, SELECT 0).
IF OBJECT_ID(N'dbo.GuardarMantenimiento', N'P') IS NOT NULL DROP PROCEDURE dbo.GuardarMantenimiento;
GO
CREATE PROCEDURE dbo.GuardarMantenimiento
(
    @ActivoId      BIGINT,
    @Comentario    NVARCHAR(500),
    @CreadoPor     NVARCHAR(25),
    @FechaCreacion DATETIME,
    @Usuario       NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Activo a
                   WHERE a.Id = @ActivoId AND a.Estatus = 1
                     AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
    BEGIN
        SELECT 0;
        RETURN;
    END
    INSERT INTO Mantenimiento (ActivoId, Comentario, Fecha, CreadoPor, FechaCreacion, Estatus, EmpresaId)
    VALUES (@ActivoId, @Comentario, GETDATE(), @CreadoPor, @FechaCreacion, 1,
            (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1));
    SELECT SCOPE_IDENTITY();
END
GO

-- 6. SP ObtenerMantenimientosPorActivo — solo vigentes y con fecha, orden desc.
IF OBJECT_ID(N'dbo.ObtenerMantenimientosPorActivo', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerMantenimientosPorActivo;
GO
CREATE PROCEDURE dbo.ObtenerMantenimientosPorActivo
(
    @ActivoId BIGINT,
    @Usuario  NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.*
    FROM Mantenimiento m
    INNER JOIN Activo a ON m.ActivoId = a.Id
    WHERE m.ActivoId = @ActivoId AND m.Estatus = 1 AND m.Fecha IS NOT NULL
      AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY m.Fecha DESC;
END
GO

-- 7. SP ObtenerConteoPaginasPorRol (ítem 6) — una sola query agrupada, sin N+1.
IF OBJECT_ID(N'dbo.ObtenerConteoPaginasPorRol', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerConteoPaginasPorRol;
GO
CREATE PROCEDURE dbo.ObtenerConteoPaginasPorRol
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RolId, COUNT(*) AS TotalPaginas
    FROM RolPaginaAccion
    WHERE Estatus = 1
    GROUP BY RolId;
END
GO
