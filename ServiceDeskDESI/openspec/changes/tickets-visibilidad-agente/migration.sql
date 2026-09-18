/* ============================================================================
   ServiceDeskDESI — Migración: agentes ven TODOS los tickets de la empresa
   ----------------------------------------------------------------------------
   Cambio: tickets-visibilidad-agente
   Fecha:  2026-09-09
   Motivo: hoy un usuario con rol `PuedeAtenderTickets = 1` (agente) solo veía
           los tickets de SU área (t.AreaId = @AreaId). En una mesa de ayuda el
           agente (p.ej. TI) debe ver TODO lo que levantan las demás áreas.
   Decisión: si `PuedeAtenderTickets = 1` => ve TODOS los tickets de la empresa.
             Si no es agente => sigue viendo SOLO los suyos.
   Aplica a: db_9c7990_servicedeskdesi (dev) y db_9c7990_helpdeskdesi (prod).
   Idempotente: sí (CREATE OR ALTER).
   ============================================================================ */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerTickets] (@Usuario NVARCHAR(25))
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT, @UsuarioId BIGINT, @AreaId BIGINT, @EsAgente BIT = 0;
    SELECT @EmpresaId = EmpresaId, @UsuarioId = Id, @AreaId = AreaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF EXISTS(SELECT 1 FROM UsuarioRol ur INNER JOIN Rol r ON ur.RolId = r.Id WHERE ur.UsuarioId = @UsuarioId AND r.PuedeAtenderTickets = 1 AND ur.Estatus = 1 AND r.Estatus = 1) SET @EsAgente = 1;

    SELECT t.*, a.Nombre AS AreaNombre, c.Nombre AS CategoriaNombre, sc.Nombre AS SubcategoriaNombre,
           u.Nombre AS UsuarioCreadorNombre, u.Apellido AS UsuarioCreadorApellido,
           u.Id AS CreadoPorId,
           te.Nombre AS EstatusNombre, te.Color AS EstatusColor,
           ta.UsuarioId AS AgenteId, ag.Nombre AS AgenteNombre, ag.Apellido AS AgenteApellido, ag.NombreUsuario AS AgenteNombreUsuario,
           ta.FechaEstimada AS FechaEstimada
    FROM Ticket t
    INNER JOIN Area a ON t.AreaId = a.Id
    INNER JOIN Categoria c ON t.CategoriaId = c.Id
    LEFT JOIN Categoria sc ON t.SubcategoriaId = sc.Id
    INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
    INNER JOIN TicketEstatus te ON t.TicketEstatusId = te.Id
    LEFT JOIN TicketAsignacion ta ON ta.TicketId = t.Id AND ta.EsActiva = 1 AND ta.Estatus = 1
    LEFT JOIN Usuarios ag ON ta.UsuarioId = ag.Id
    WHERE t.Estatus = 1 AND t.EmpresaId = @EmpresaId
      -- Agente (PuedeAtenderTickets = 1): ve TODOS los tickets de la empresa.
      -- No agente: ve solo los suyos.
      AND ((@EsAgente = 0 AND t.CreadoPor = @Usuario) OR (@EsAgente = 1))
    ORDER BY t.FechaCreacion DESC;
END
GO
