-- ============================================================
-- Hotfix: enriquecer ObtenerPersonaPorId con UsuarioId / NombreUsuarioVinculado
-- ----------------------------------------------------------------------------
-- Problema: en modo edición (/Catalogs/Persona/{id}) los inputs NO se bloqueaban
-- y no se mostraba el usuario vinculado, porque ObtenerPersonaPorId no devolvía
-- las columnas UsuarioId / NombreUsuarioVinculado (solo ObtenerPersonas las tenía).
-- ============================================================
IF OBJECT_ID(N'dbo.ObtenerPersonaPorId', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerPersonaPorId;
GO
CREATE PROCEDURE [dbo].[ObtenerPersonaPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.*,
           pu.Nombre AS PuestoNombre,
           u.Id AS UsuarioId,
           u.NombreUsuario AS NombreUsuarioVinculado
    FROM Persona p
    INNER JOIN Puesto pu ON p.PuestoId = pu.Id
    LEFT JOIN Usuarios u ON u.PersonaId = p.Id AND u.Estatus = 1
    WHERE p.Id = @Id AND p.Estatus = 1 AND p.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO
