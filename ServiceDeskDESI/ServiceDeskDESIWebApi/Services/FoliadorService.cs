using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;

namespace ServiceDeskDESIWebApi.Services
{
    public class FoliadorService
    {
        private readonly DbWrapper _dbWrapper;

        public FoliadorService(DbWrapper dbWrapper)
        {
            _dbWrapper = dbWrapper;
        }

        /// <summary>
        /// Consulta el consecutivo actual del foliador y calcula el folio "current+1"
        /// formateado para la vista previa (advisory). El valor persistido es el autoritativo.
        /// </summary>
        public ModelResponse<FoliadorDTO> ConsultarConsecutivo(string nombre, string usuario)
        {
            try
            {
                Log.Information("FoliadorService.ConsultarConsecutivo para Nombre {Nombre} usuario {Usuario}", nombre, usuario);

                if (string.IsNullOrWhiteSpace(nombre)) { throw new ArgumentException("El nombre del foliador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var foliador = _dbWrapper.ConsultarFoliador(nombre, usuario);

                var dto = foliador == null
                    ? null
                    : new FoliadorDTO
                    {
                        EmpresaId = foliador.EmpresaId,
                        FechaActualizacion = foliador.FechaActualizacion,
                        Nombre = foliador.Nombre,
                        Descripcion = foliador.Descripcion,
                        Consecutivo = foliador.Consecutivo,
                        FolioSiguiente = FormatearFolio(foliador.Consecutivo + 1)
                    };

                var result = new ModelResponse<FoliadorDTO>
                {
                    IsSuccess = true,
                    Response = dto,
                    Message = dto == null ? "No existe foliador para el nombre indicado." : "Foliador obtenido correctamente."
                };
                Log.Information("FoliadorService.ConsultarConsecutivo RESULTADO: IsSuccess={IsSuccess}, Consecutivo={Consecutivo}, FolioSiguiente={FolioSiguiente}",
                    result.IsSuccess, dto?.Consecutivo, dto?.FolioSiguiente);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ConsultarConsecutivo para usuario {Usuario}", usuario);
                return new ModelResponse<FoliadorDTO> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en FoliadorService.ConsultarConsecutivo para usuario {Usuario}", usuario);
                return new ModelResponse<FoliadorDTO> { IsSuccess = false, Message = "Ocurrió un error al consultar el foliador." };
            }
        }

        /// <summary>
        /// Incrementa el consecutivo del foliador y devuelve el nuevo valor.
        /// Interno al servicio (no se expone por HTTP). Debe ejecutarse dentro de la
        /// transacción ambiental del DbWrapper compartido para revertirse junto al insert.
        /// </summary>
        internal int ActualizarConsecutivo(string nombre, string usuario)
        {
            var consecutivo = _dbWrapper.ActualizarFoliador(nombre, usuario);
            if (consecutivo <= 0)
                throw new InvalidOperationException("No se pudo actualizar el foliador.");
            return consecutivo;
        }

        /// <summary>
        /// Único punto de verdad del formato del folio: T-{Consecutivo:00000}.
        /// </summary>
        public static string FormatearFolio(int consecutivo) => $"T-{consecutivo:00000}";
    }
}
