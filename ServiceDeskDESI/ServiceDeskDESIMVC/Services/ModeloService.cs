using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class ModeloService
    {
        private readonly HttpClientConnection _httpClient;

        public ModeloService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModeloDTO> ObtenerModeloPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("ModeloService.ObtenerModeloPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerModeloPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("ModeloService.ObtenerModeloPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("ModeloService.ObtenerModeloPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<Modelo>> GuardarOActualizarModelo(Modelo modelo)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("ModeloService.GuardarOActualizarModelo ENTRADA: ModeloId={ModeloId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                modelo?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarOActualizarModelo(modelo);
            Log.Information("ModeloService.GuardarOActualizarModelo SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarModelo(Modelo modelo)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("ModeloService.EliminarModelo ENTRADA: ModeloId={ModeloId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                modelo?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarModelo(modelo);
            Log.Information("ModeloService.EliminarModelo SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<ModeloDTO>>> ConsultarTodosLosModelos()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("ModeloService.ConsultarTodosLosModelos ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTodosLosModelos();
            Log.Information("ModeloService.ConsultarTodosLosModelos SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaModelo()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("ModeloService.ObtenerPermisosParaModelo ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Modelos");
                Log.Information("ModeloService.ObtenerPermisosParaModelo SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("ModeloService.ObtenerPermisosParaModelo SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
