using ServiceDeskDESIEntities.Tickets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper
    {
        /// <summary>
        /// Inserta la evidencia y devuelve el Id generado. Devuelve 0 si el ticket
        /// no pertenece a la empresa del usuario (sin permisos).
        /// </summary>
        public long GuardarEvidencia(long ticketId, string nombreArchivo, string rutaArchivo, string usuario)
        {
            var result = ExecuteScalar("GuardarEvidencia", CommandType.StoredProcedure, new SqlParameter[]
            {
                new SqlParameter("@TicketId", ticketId),
                new SqlParameter("@NombreArchivo", nombreArchivo),
                new SqlParameter("@RutaArchivo", rutaArchivo),
                new SqlParameter("@Usuario", usuario)
            });

            return result == null || result is DBNull ? 0L : Convert.ToInt64(result);
        }

        /// <summary>
        /// Obtiene las evidencias (activas) de un ticket, filtradas por la empresa del usuario.
        /// </summary>
        public List<TicketEvidencia> ObtenerEvidenciasPorTicket(long ticketId, string usuario)
        {
            var evidencias = GetObjects("ObtenerEvidenciasPorTicket", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@TicketId", ticketId),
                    new SqlParameter("@Usuario", usuario)
                },
                new Func<IDataReader, TicketEvidencia>((reader) =>
                {
                    var e = LlenarEntidad<TicketEvidencia>(reader);
                    return e;
                }));

            return evidencias.ToList();
        }

        /// <summary>
        /// Obtiene una evidencia por Id, filtrada por la empresa del usuario.
        /// Devuelve null si no existe o no pertenece a su empresa.
        /// </summary>
        public TicketEvidencia ObtenerEvidencia(long id, string usuario)
        {
            return GetObject("ObtenerEvidencia", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@Usuario", usuario)
                },
                new Func<IDataReader, TicketEvidencia>((reader) =>
                {
                    var e = LlenarEntidad<TicketEvidencia>(reader);
                    return e;
                }));
        }
    }
}
