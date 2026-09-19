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
    public class MarcaService
    {
        private readonly HttpClientConnection _httpClient;

        public MarcaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Marca> ObtenerMarcaPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("MarcaService.ObtenerMarcaPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerMarcaPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("MarcaService.ObtenerMarcaPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("MarcaService.ObtenerMarcaPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<Marca>> GuardarOActualizarMarca(Marca marca)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("MarcaService.GuardarOActualizarMarca ENTRADA: MarcaId={MarcaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                marca?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarOActualizarMarca(marca);
            Log.Information("MarcaService.GuardarOActualizarMarca SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarMarca(Marca marca)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("MarcaService.EliminarMarca ENTRADA: MarcaId={MarcaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                marca?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarMarca(marca);
            Log.Information("MarcaService.EliminarMarca SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<Marca>>> ConsultarTodosLasMarcas()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("MarcaService.ConsultarTodosLasMarcas ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTodosLasMarcas();
            Log.Information("MarcaService.ConsultarTodosLasMarcas SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaMarca()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("MarcaService.ObtenerPermisosParaMarca ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Marcas");
                Log.Information("MarcaService.ObtenerPermisosParaMarca SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("MarcaService.ObtenerPermisosParaMarca SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
