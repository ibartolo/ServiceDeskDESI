using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
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
            var response = await _httpClient.ObtenerEmpresaPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<Empresa>> GuardarOActualizarEmpresa(Empresa empresa)
        {
            return await _httpClient.GuardarOActualizarEmpresa(empresa);
        }

        public async Task<ModelResponse<Empresa>> GuardarNuevaEmpresa(Empresa empresa)
        {
            return await _httpClient.GuardarNuevaEmpresa(empresa);
        }

        public async Task<ModelResponse<Empresa>> RegistrarEmpresa(Empresa empresa)
        {
            return await _httpClient.RegistrarEmpresa(empresa);
        }

        public async Task<ModelResponse<Empresa>> GuardarNuevaEmpresaCompleta(Empresa empresa)
        {
            return await _httpClient.GuardarNuevaEmpresaCompleta(empresa);
        }

        public async Task<ModelResponse> EliminarEmpresa(Empresa empresa)
        {
            return await _httpClient.EliminarEmpresa(empresa);
        }

        public async Task<object> ObtenerPermisosParaEmpresa()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Compañías");
            }
            return null;
        }
    }
}
