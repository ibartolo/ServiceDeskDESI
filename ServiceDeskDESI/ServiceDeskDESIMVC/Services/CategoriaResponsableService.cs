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
    public class CategoriaResponsableService
    {
        private readonly HttpClientConnection _httpClient;

        public CategoriaResponsableService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<CategoriaResponsableDTO>>> ObtenerResponsablesPorCategoria(long categoriaId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaResponsableService.ObtenerResponsablesPorCategoria ENTRADA: CategoriaId={CategoriaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                categoriaId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerResponsablesPorCategoria(categoriaId);
            Log.Information("CategoriaResponsableService.ObtenerResponsablesPorCategoria SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<CategoriaResponsableDTO>>> ObtenerCategoriasPorResponsable(long usuarioId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaResponsableService.ObtenerCategoriasPorResponsable ENTRADA: UsuarioId={UsuarioId}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                usuarioId, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.ObtenerCategoriasPorResponsable(usuarioId);
            Log.Information("CategoriaResponsableService.ObtenerCategoriasPorResponsable SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<CategoriaResponsableDTO>>> ObtenerTodosLosResponsables()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaResponsableService.ObtenerTodosLosResponsables ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTodosLosResponsables();
            Log.Information("CategoriaResponsableService.ObtenerTodosLosResponsables SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<CategoriaResponsable>> GuardarOActualizarCategoriaResponsable(CategoriaResponsable categoriaResponsable)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaResponsableService.GuardarOActualizarCategoriaResponsable ENTRADA: Id={Id}, CategoriaId={CategoriaId}, UsuarioId={UsuarioId}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                categoriaResponsable?.Id, categoriaResponsable?.CategoriaId, categoriaResponsable?.UsuarioId, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.GuardarOActualizarCategoriaResponsable(categoriaResponsable);
            Log.Information("CategoriaResponsableService.GuardarOActualizarCategoriaResponsable SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarCategoriaResponsable(CategoriaResponsable categoriaResponsable)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("CategoriaResponsableService.EliminarCategoriaResponsable ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                categoriaResponsable?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarCategoriaResponsable(categoriaResponsable);
            Log.Information("CategoriaResponsableService.EliminarCategoriaResponsable SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }
    }
}
