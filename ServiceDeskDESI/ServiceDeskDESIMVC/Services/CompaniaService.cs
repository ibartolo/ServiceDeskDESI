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
    public class CompaniaService
    {
        private readonly HttpClientConnection _httpClient;

        public CompaniaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Compania> ObtenerCompaniaPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CompaniaService.ObtenerCompaniaPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerCompaniaPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("CompaniaService.ObtenerCompaniaPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("CompaniaService.ObtenerCompaniaPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<Compania>> GuardarOActualizarCompania(Compania compania)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CompaniaService.GuardarOActualizarCompania ENTRADA: CompaniaId={CompaniaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                compania?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarActualizarCompania(compania);
            Log.Information("CompaniaService.GuardarOActualizarCompania SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarCompania(Compania compania)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CompaniaService.EliminarCompania ENTRADA: CompaniaId={CompaniaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                compania?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarCompania(compania);
            Log.Information("CompaniaService.EliminarCompania SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<Compania>>> ConsultarTodasCompanias()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CompaniaService.ConsultarTodasCompanias ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTodasCompanias();
            Log.Information("CompaniaService.ConsultarTodasCompanias SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaCompania()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CompaniaService.ObtenerPermisosParaCompania ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Compañías");
                Log.Information("CompaniaService.ObtenerPermisosParaCompania SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("CompaniaService.ObtenerPermisosParaCompania SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
