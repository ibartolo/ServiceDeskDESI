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
    public class CategoriaService
    {
        private readonly HttpClientConnection _httpClient;

        public CategoriaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CategoriaDTO> ObtenerCategoriaPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaService.ObtenerCategoriaPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerCategoriaPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("CategoriaService.ObtenerCategoriaPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("CategoriaService.ObtenerCategoriaPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<Categoria>> GuardarOActualizarCategoria(Categoria categoria)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaService.GuardarOActualizarCategoria ENTRADA: CategoriaId={CategoriaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                categoria?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarOActualizarCategoria(categoria);
            Log.Information("CategoriaService.GuardarOActualizarCategoria SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarCategoria(Categoria categoria)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaService.EliminarCategoria ENTRADA: CategoriaId={CategoriaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                categoria?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarCategoria(categoria);
            Log.Information("CategoriaService.EliminarCategoria SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<CategoriaDTO>>> ConsultarTodasCategorias()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaService.ConsultarTodasCategorias ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerCategorias();
            Log.Information("CategoriaService.ConsultarTodasCategorias SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaCategoria()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaService.ObtenerPermisosParaCategoria ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Categorías");
                Log.Information("CategoriaService.ObtenerPermisosParaCategoria SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("CategoriaService.ObtenerPermisosParaCategoria SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }

        public async Task<ModelResponse<List<CategoriaDTO>>> ObtenerCategoriasPorArea(long areaId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaService.ObtenerCategoriasPorArea ENTRADA: AreaId={AreaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                areaId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerCategoriasPorArea(areaId);
            Log.Information("CategoriaService.ObtenerCategoriasPorArea SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<CategoriaDTO>>> ObtenerCategoriasPorPadre(long categoriaPadreId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaService.ObtenerCategoriasPorPadre ENTRADA: CategoriaPadreId={CategoriaPadreId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                categoriaPadreId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerCategoriasPorPadre(categoriaPadreId);
            Log.Information("CategoriaService.ObtenerCategoriasPorPadre SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }
    }
}
