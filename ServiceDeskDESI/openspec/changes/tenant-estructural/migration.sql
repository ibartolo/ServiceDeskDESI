/* ============================================================================
   ServiceDeskDESI — Migración: Tenant de primera clase (D1) — Fase 1
   ----------------------------------------------------------------------------
   Cambio: tenant-estructural (ref D1)
   Fecha:  2026-08-18
   Base de datos: db_9c7990_servicedeskdesi

   IMPORTANTE — estado actual (parcial):
     - Se añade EmpresaId (NULL) + FK + backfill a las 12 tablas de dominio.
     - Se hace NombreUsuario único global (cierra la fuga por colisión de tenant).
     - Se reescriben los SPs de REGISTRO y los GuardarOActualizar de Activo/Area/Categoria.
     - PENDIENTE (siguiente cambio): reescribir el resto de GuardarOActualizar*,
       Eliminar* y Obtener* para filtrar por EmpresaId (hoy siguen por CreadoPor).
============================================================================ */

USE [db_9c7990_servicedeskdesi];
GO

-- 1. Columna EmpresaId (nullable para no romper inserts existentes; backfill abajo)
ALTER TABLE [dbo].[Activo] ADD [EmpresaId] [bigint] NULL
GO
ALTER TABLE [dbo].[Area] ADD [EmpresaId] [bigint] NULL
GO
ALTER TABLE [dbo].[Categoria] ADD [EmpresaId] [bigint] NULL
GO
ALTER TABLE [dbo].[CategoriaResponsable] ADD [EmpresaId] [bigint] NULL
GO
ALTER TABLE [dbo].[Marca] ADD [EmpresaId] [bigint] NULL
GO
ALTER TABLE [dbo].[Modelo] ADD [EmpresaId] [bigint] NULL
GO
ALTER TABLE [dbo].[Persona] ADD [EmpresaId] [bigint] NULL
GO
ALTER TABLE [dbo].[Puesto] ADD [EmpresaId] [bigint] NULL
GO
ALTER TABLE [dbo].[Rol] ADD [EmpresaId] [bigint] NULL
GO
ALTER TABLE [dbo].[Sucursal] ADD [EmpresaId] [bigint] NULL
GO
ALTER TABLE [dbo].[Ticket] ADD [EmpresaId] [bigint] NULL
GO
ALTER TABLE [dbo].[TipoActivo] ADD [EmpresaId] [bigint] NULL
GO

-- 2. Backfill de EmpresaId desde CreadoPor -> Usuarios.EmpresaId
UPDATE a SET a.EmpresaId = u.EmpresaId FROM Activo a INNER JOIN Usuarios u ON a.CreadoPor = u.NombreUsuario
GO
UPDATE a SET a.EmpresaId = u.EmpresaId FROM Area a INNER JOIN Usuarios u ON a.CreadoPor = u.NombreUsuario
GO
UPDATE c SET c.EmpresaId = u.EmpresaId FROM Categoria c INNER JOIN Usuarios u ON c.CreadoPor = u.NombreUsuario
GO
UPDATE cr SET cr.EmpresaId = u.EmpresaId FROM CategoriaResponsable cr INNER JOIN Usuarios u ON cr.CreadoPor = u.NombreUsuario
GO
UPDATE m SET m.EmpresaId = u.EmpresaId FROM Marca m INNER JOIN Usuarios u ON m.CreadoPor = u.NombreUsuario
GO
UPDATE mo SET mo.EmpresaId = u.EmpresaId FROM Modelo mo INNER JOIN Usuarios u ON mo.CreadoPor = u.NombreUsuario
GO
UPDATE p SET p.EmpresaId = u.EmpresaId FROM Persona p INNER JOIN Usuarios u ON p.CreadoPor = u.NombreUsuario
GO
UPDATE p SET p.EmpresaId = u.EmpresaId FROM Puesto p INNER JOIN Usuarios u ON p.CreadoPor = u.NombreUsuario
GO
UPDATE r SET r.EmpresaId = u.EmpresaId FROM Rol r INNER JOIN Usuarios u ON r.CreadoPor = u.NombreUsuario
GO
UPDATE s SET s.EmpresaId = u.EmpresaId FROM Sucursal s INNER JOIN Usuarios u ON s.CreadoPor = u.NombreUsuario
GO
UPDATE t SET t.EmpresaId = u.EmpresaId FROM Ticket t INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
GO
UPDATE ta SET ta.EmpresaId = u.EmpresaId FROM TipoActivo ta INNER JOIN Usuarios u ON ta.CreadoPor = u.NombreUsuario
GO

-- 3. FKs a Empresa
ALTER TABLE [dbo].[Activo]  WITH CHECK ADD  CONSTRAINT [FK_Activo_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[Area]  WITH CHECK ADD  CONSTRAINT [FK_Area_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[Categoria]  WITH CHECK ADD  CONSTRAINT [FK_Categoria_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[CategoriaResponsable]  WITH CHECK ADD  CONSTRAINT [FK_CategoriaResponsable_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[Marca]  WITH CHECK ADD  CONSTRAINT [FK_Marca_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[Modelo]  WITH CHECK ADD  CONSTRAINT [FK_Modelo_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[Persona]  WITH CHECK ADD  CONSTRAINT [FK_Persona_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[Puesto]  WITH CHECK ADD  CONSTRAINT [FK_Puesto_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[Rol]  WITH CHECK ADD  CONSTRAINT [FK_Rol_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[Sucursal]  WITH CHECK ADD  CONSTRAINT [FK_Sucursal_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[Ticket]  WITH CHECK ADD  CONSTRAINT [FK_Ticket_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[TipoActivo]  WITH CHECK ADD  CONSTRAINT [FK_TipoActivo_Empresa] FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id])
GO

-- 4. NombreUsuario único global (cierra la fuga por colisión de tenant)
--    NOTA: si existen duplicados, este índice fallará; resolverlos antes de ejecutar.
CREATE UNIQUE NONCLUSTERED INDEX [UX_Usuarios_NombreUsuario] ON [dbo].[Usuarios] ([NombreUsuario] ASC)
GO

-- 5. SPs de registro (EmpresaId explícito)
CREATE OR ALTER PROCEDURE [dbo].[GuardarNuevaAreaParaEmpresa]
(
    @Nombre NVARCHAR(250),
    @Descripcion NVARCHAR(500) = NULL,
    @Correo NVARCHAR(100) = NULL,
    @CreadoPor NVARCHAR(25),
    @FechaCreacion DATETIME,
    @EmpresaId BIGINT
)
AS
BEGIN
    INSERT INTO Area (Nombre, Descripcion, Correo, CreadoPor, FechaCreacion, Estatus, EmpresaId)
    VALUES (@Nombre, @Descripcion, @Correo, @CreadoPor, @FechaCreacion, 1, @EmpresaId)
    SELECT SCOPE_IDENTITY()
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GuardarNuevaSucursalParaEmpresa]
(
    @Nombre NVARCHAR(250),
    @Descripcion NVARCHAR(500) = NULL,
    @Calle NVARCHAR(100) = NULL,
    @Ciudad NVARCHAR(100) = NULL,
    @Colonia NVARCHAR(100) = NULL,
    @CodigoPostal NVARCHAR(10) = NULL,
    @CreadoPor NVARCHAR(25),
    @FechaCreacion DATETIME,
    @EmpresaId BIGINT
)
AS
BEGIN
    INSERT INTO Sucursal (Nombre, Descripcion, Calle, Ciudad, Colonia, CodigoPostal, CreadoPor, FechaCreacion, Estatus, EmpresaId)
    VALUES (@Nombre, @Descripcion, @Calle, @Ciudad, @Colonia, @CodigoPostal, @CreadoPor, @FechaCreacion, 1, @EmpresaId)
    SELECT SCOPE_IDENTITY()
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GuardarRolParaNuevaEmpresa]
(
    @Nombre NVARCHAR(50),
    @Descripcion NVARCHAR(250) = NULL,
    @PuedeAtenderTickets BIT,
    @CreadoPor NVARCHAR(25),
    @FechaCreacion DATETIME,
    @EmpresaId BIGINT
)
AS
BEGIN
    INSERT INTO Rol (Nombre, Descripcion, PuedeAtenderTickets, CreadoPor, FechaCreacion, Estatus, EmpresaId)
    VALUES (@Nombre, @Descripcion, @PuedeAtenderTickets, @CreadoPor, @FechaCreacion, 1, @EmpresaId)
    SELECT SCOPE_IDENTITY()
END
GO

-- 6. GuardarOActualizarActivo — filtro/insert por EmpresaId
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarActivo]
(
    @Id BIGINT, @Nombre NVARCHAR(50), @Descripcion NVARCHAR(250) = NULL,
    @TipoActivoID BIGINT, @Serial NVARCHAR(50) = NULL, @MarcaID BIGINT, @ModeloID BIGINT,
    @Notas NVARCHAR(250) = NULL, @FechaCompra DATETIME = NULL,
    @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL,
    @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF EXISTS(SELECT * FROM Activo WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Activo a WHERE a.Id = @Id AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        IF NOT EXISTS(SELECT 1 FROM TipoActivo ta WHERE ta.Id = @TipoActivoID AND ta.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        IF NOT EXISTS(SELECT 1 FROM Marca m WHERE m.Id = @MarcaID AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        IF NOT EXISTS(SELECT 1 FROM Modelo mo WHERE mo.Id = @ModeloID AND mo.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        UPDATE Activo SET Nombre=@Nombre, Descripcion=@Descripcion, TipoActivoID=@TipoActivoID, Serial=@Serial, MarcaID=@MarcaID, ModeloID=@ModeloID, Notas=@Notas, FechaCompra=@FechaCompra, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @CreadoPor AND EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1) AND Estatus = 1)
        BEGIN SELECT 0 RETURN END
        IF NOT EXISTS(SELECT 1 FROM TipoActivo ta WHERE ta.Id = @TipoActivoID AND ta.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        IF NOT EXISTS(SELECT 1 FROM Marca m WHERE m.Id = @MarcaID AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        IF NOT EXISTS(SELECT 1 FROM Modelo mo WHERE mo.Id = @ModeloID AND mo.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        INSERT INTO Activo (Nombre, Descripcion, TipoActivoID, Serial, MarcaID, ModeloID, Notas, FechaCompra, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@Nombre, @Descripcion, @TipoActivoID, @Serial, @MarcaID, @ModeloID, @Notas, @FechaCompra, @CreadoPor, @FechaCreacion, @Estatus, (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        SELECT SCOPE_IDENTITY()
    END
END
GO

-- 7. GuardarOActualizarArea — insert por EmpresaId
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarArea]
(
    @Id BIGINT, @Nombre NVARCHAR(250), @Descripcion NVARCHAR(500) = NULL, @Correo NVARCHAR(100) = NULL,
    @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL,
    @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @CreadoPor AND EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1) AND Estatus = 1)
    BEGIN SELECT 0 RETURN END
    IF EXISTS(SELECT * FROM Area WHERE Id = @Id)
    BEGIN
        UPDATE Area SET Nombre=@Nombre, Descripcion=@Descripcion, Correo=@Correo, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        INSERT INTO Area (Nombre, Descripcion, Correo, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@Nombre, @Descripcion, @Correo, @CreadoPor, @FechaCreacion, @Estatus, (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        SELECT SCOPE_IDENTITY()
    END
END
GO

-- 8. GuardarOActualizarCategoria — filtro/insert por EmpresaId
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarCategoria]
(
    @Id BIGINT, @Nombre NVARCHAR(250), @Descripcion NVARCHAR(500) = NULL,
    @CategoriaPadreId BIGINT = NULL, @AreaId BIGINT, @Orden INT,
    @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL,
    @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF EXISTS(SELECT * FROM Categoria WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Categoria c WHERE c.Id = @Id AND c.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        UPDATE Categoria SET Nombre=@Nombre, Descripcion=@Descripcion, CategoriaPadreId=@CategoriaPadreId, AreaId=@AreaId, Orden=@Orden, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @CreadoPor AND EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1) AND Estatus = 1)
        BEGIN SELECT 0 RETURN END
        IF NOT EXISTS(SELECT 1 FROM Area a WHERE a.Id = @AreaId AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        INSERT INTO Categoria (Nombre, Descripcion, CategoriaPadreId, AreaId, Orden, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@Nombre, @Descripcion, @CategoriaPadreId, @AreaId, @Orden, @CreadoPor, @FechaCreacion, @Estatus, (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        SELECT SCOPE_IDENTITY()
    END
END
GO
