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
    public class EmpresaService
    {
        private readonly HttpClientConnection _httpClient;

        public EmpresaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Empresa> ObtenerEmpresaPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EmpresaService.ObtenerEmpresaPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerEmpresaPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("EmpresaService.ObtenerEmpresaPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("EmpresaService.ObtenerEmpresaPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<Empresa>> GuardarOActualizarEmpresa(Empresa empresa)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EmpresaService.GuardarOActualizarEmpresa ENTRADA: EmpresaId={EmpresaId}, Usuario={Usuario}, EmpresaIdSesion={EmpresaIdSesion}, UserId={UserId}",
                empresa?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarOActualizarEmpresa(empresa);
            Log.Information("EmpresaService.GuardarOActualizarEmpresa SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<Empresa>> GuardarNuevaEmpresa(Empresa empresa)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EmpresaService.GuardarNuevaEmpresa ENTRADA: EmpresaId={EmpresaId}, Usuario={Usuario}, EmpresaIdSesion={EmpresaIdSesion}, UserId={UserId}",
                empresa?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarNuevaEmpresa(empresa);
            Log.Information("EmpresaService.GuardarNuevaEmpresa SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<Empresa>> RegistrarEmpresa(Empresa empresa)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EmpresaService.RegistrarEmpresa ENTRADA: EmpresaId={EmpresaId}, Usuario={Usuario}, EmpresaIdSesion={EmpresaIdSesion}, UserId={UserId}",
                empresa?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.RegistrarEmpresa(empresa);
            Log.Information("EmpresaService.RegistrarEmpresa SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<Empresa>> GuardarNuevaEmpresaCompleta(Empresa empresa)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EmpresaService.GuardarNuevaEmpresaCompleta ENTRADA: EmpresaId={EmpresaId}, Usuario={Usuario}, EmpresaIdSesion={EmpresaIdSesion}, UserId={UserId}",
                empresa?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarNuevaEmpresaCompleta(empresa);
            Log.Information("EmpresaService.GuardarNuevaEmpresaCompleta SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarEmpresa(Empresa empresa)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EmpresaService.EliminarEmpresa ENTRADA: EmpresaId={EmpresaId}, Usuario={Usuario}, EmpresaIdSesion={EmpresaIdSesion}, UserId={UserId}",
                empresa?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarEmpresa(empresa);
            Log.Information("EmpresaService.EliminarEmpresa SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> GuardarLogoEmpresa(string logoUrl)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EmpresaService.GuardarLogoEmpresa ENTRADA: LogoUrl={LogoUrl}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                logoUrl, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarLogoEmpresa(logoUrl);
            Log.Information("EmpresaService.GuardarLogoEmpresa SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaEmpresa()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EmpresaService.ObtenerPermisosParaEmpresa ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Compañías");
                Log.Information("EmpresaService.ObtenerPermisosParaEmpresa SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("EmpresaService.ObtenerPermisosParaEmpresa SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
