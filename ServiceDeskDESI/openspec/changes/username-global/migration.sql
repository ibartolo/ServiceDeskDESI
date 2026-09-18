/* ============================================================================
   ServiceDeskDESI — Migración: volver a NombreUsuario ÚNICO GLOBAL
   ----------------------------------------------------------------------------
   Cambio: username-global
   Fecha:  2026-09-09
   Motivo: se revierte la unicidad "por empresa". El username vuelve a ser ÚNICO
           GLOBAL (no se puede repetir entre empresas). Esto:
             - elimina la ambigüedad de login (se mantiene login por username),
             - hace CONFIABLE el filtrado por `CreadoPor = NombreUsuario`
               (no hay colisiones de username entre empresas).
   Aplica a: db_9c7990_servicedeskdesi (dev) y db_9c7990_helpdeskdesi (prod).
   Idempotente: sí.
   PRECONDICIÓN: no deben existir usernames duplicados (si existen, este script
   falla al crear el índice único; limpiar el duplicado antes).
   ============================================================================ */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------------
   1) Índice: quitar el de "por empresa" (filtrado) y volver al ÚNICO GLOBAL
   --------------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = 'UX_Usuarios_NombreUsuario_Empresa' AND object_id = OBJECT_ID('dbo.Usuarios'))
BEGIN
    DROP INDEX [UX_Usuarios_NombreUsuario_Empresa] ON [dbo].[Usuarios];
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_Usuarios_NombreUsuario' AND object_id = OBJECT_ID('dbo.Usuarios'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Usuarios_NombreUsuario]
        ON [dbo].[Usuarios] ([NombreUsuario] ASC);
END
GO

/* ---------------------------------------------------------------------------
   2) SP GuardarOActualizarUsuarioAdmin: guards de username GLOBALES
      (sin EmpresaId ni Estatus, para alinear con el índice global único).
      Los guards de Correo se conservan como estaban (por empresa).
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

        -- Duplicado de username: GLOBAL (cualquier empresa)
        IF EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @NombreUsuario AND Id != @Id)
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
        -- Duplicado de username: GLOBAL (cualquier empresa)
        IF EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @NombreUsuario)
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
