/* ============================================================================
   ServiceDeskDESI — Migración: username único POR EMPRESA (no global)
   ----------------------------------------------------------------------------
   Cambio:   username-por-empresa
   Fecha:    2026-09-09
   Origen:   Bugs de pruebas localhost (caso 6) — alta de usuario fallaba con
             excepción SQL 2601 "duplicate key" en UX_Usuarios_NombreUsuario.
   ----------------------------------------------------------------------------
   Motivo:
   - El índice UX_Usuarios_NombreUsuario era ÚNICO GLOBAL (solo NombreUsuario),
     pero los guards del SP validan POR EMPRESA (NombreUsuario + EmpresaId).
     Al crear un usuario con el mismo username que otro de OTRA empresa, el
     guard no lo detectaba y el índice global lanzaba excepción cruda.
   - Decisión: los usernames son únicos POR EMPRESA y se permite REUSAR el
     username de un usuario INACTIVO (índice filtrado por Estatus = 1).
   ----------------------------------------------------------------------------
   Aplica a: db_9c7990_servicedeskdesi (dev) y db_9c7990_helpdeskdesi (prod).
             El script NO incluye USE: se ejecuta con -d <base>.
   Idempotente: sí (guards sys.indexes).
   ============================================================================ */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------------
   1) Índice de unicidad: global -> (NombreUsuario, EmpresaId) FILTRADO a activos
   --------------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = 'UX_Usuarios_NombreUsuario'
             AND object_id = OBJECT_ID('dbo.Usuarios'))
BEGIN
    DROP INDEX [UX_Usuarios_NombreUsuario] ON [dbo].[Usuarios];
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_Usuarios_NombreUsuario_Empresa'
                 AND object_id = OBJECT_ID('dbo.Usuarios'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Usuarios_NombreUsuario_Empresa]
        ON [dbo].[Usuarios] ([NombreUsuario] ASC, [EmpresaId] ASC)
        WHERE [Estatus] = 1;
END
GO

/* ---------------------------------------------------------------------------
   2) SP GuardarOActualizarUsuarioAdmin
      Alinea el guard del INSERT con el índice filtrado: agrega "AND Estatus = 1"
      a los chequeos de duplicado de NombreUsuario y Correo (permite reusar los
      datos de un usuario inactivo de la misma empresa).
   --------------------------------------------------------------------------- */
ALTER PROCEDURE [dbo].[GuardarOActualizarUsuarioAdmin]
(
    @Id BIGINT,
    @NombreUsuario NVARCHAR(25),
    @Contrasena NVARCHAR(250) = NULL,
    @ImagenPerfil NVARCHAR(250) = NULL,
    @Correo NVARCHAR(250),
    @Nombre NVARCHAR(150),
    @Apellido NVARCHAR(250),
    @Celular NVARCHAR(50),
    @CreadoPor NVARCHAR(25),
    @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL,
    @FechaModificacion DATETIME = NULL,
    @Estatus BIT,
    @SucursalId BIGINT,
    @Firma NVARCHAR(250) = NULL,
    @RFC NVARCHAR(250),
    @AreaId BIGINT,
    @EmpresaId BIGINT,
    @UsuarioAdmin NVARCHAR(25)
)
AS
BEGIN
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;
    SET NOCOUNT ON;

    IF NOT EXISTS(
        SELECT 1
        FROM Usuarios
        WHERE NombreUsuario = @UsuarioAdmin
            AND EmpresaId = @EmpresaId
            AND Estatus = 1
    )
    BEGIN
        SELECT 0
        RETURN
    END

    IF EXISTS(SELECT 1 FROM Usuarios WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(
            SELECT 1
            FROM Usuarios
            WHERE Id = @Id
                AND EmpresaId = @EmpresaId
                AND Estatus = 1
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @NombreUsuario AND Id != @Id AND EmpresaId = @EmpresaId AND Estatus = 1)
        BEGIN
            SELECT -1
            RETURN
        END

        IF EXISTS(SELECT 1 FROM Usuarios WHERE Correo = @Correo AND Id != @Id AND EmpresaId = @EmpresaId AND Estatus = 1)
        BEGIN
            SELECT -2
            RETURN
        END

        -- UPDATE: actualiza la contraseña solo si viene con valor; si no, conserva la existente
        UPDATE Usuarios
        SET NombreUsuario = @NombreUsuario,
            Contrasena = CASE
                WHEN @Contrasena IS NOT NULL AND @Contrasena != '' THEN @Contrasena
                ELSE Contrasena
            END,
            ImagenPerfil = @ImagenPerfil,
            Correo = @Correo,
            Nombre = @Nombre,
            Apellido = @Apellido,
            Celular = @Celular,
            ModificadoPor = @ModificadoPor,
            FechaModificacion = @FechaModificacion,
            Estatus = @Estatus,
            SucursalId = @SucursalId,
            Firma = @Firma,
            RFC = @RFC,
            AreaId = @AreaId,
            EmpresaId = @EmpresaId
        WHERE Id = @Id

        SELECT @Id
    END
    ELSE
    BEGIN
        -- INSERT: unicidad por empresa y SOLO contra usuarios activos
        IF EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @NombreUsuario AND EmpresaId = @EmpresaId AND Estatus = 1)
        BEGIN
            SELECT -1
            RETURN
        END

        IF EXISTS(SELECT 1 FROM Usuarios WHERE Correo = @Correo AND EmpresaId = @EmpresaId AND Estatus = 1)
        BEGIN
            SELECT -2
            RETURN
        END

        -- INSERT: la contraseña viene ya hasheada desde el service (obligatoria al crear)
        INSERT INTO Usuarios
        (NombreUsuario, Contrasena, ImagenPerfil, Correo, Nombre, Apellido,
         Celular, CreadoPor, FechaCreacion, Estatus, SucursalId, Firma, RFC, AreaId, EmpresaId)
        VALUES
        (@NombreUsuario, @Contrasena, @ImagenPerfil, @Correo, @Nombre, @Apellido,
         @Celular, @CreadoPor, @FechaCreacion, @Estatus, @SucursalId, @Firma, @RFC, @AreaId, @EmpresaId)

        SELECT SCOPE_IDENTITY()
    END
END
GO

/* ---------------------------------------------------------------------------
   Verificación sugerida (ejecutar aparte):
     SELECT name, is_unique, filter_definition FROM sys.indexes
     WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name LIKE 'UX_Usuarios%';
   --------------------------------------------------------------------------- */
