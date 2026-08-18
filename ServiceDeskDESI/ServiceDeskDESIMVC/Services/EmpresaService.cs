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

        public async Task<ModelResponse> ObtenerTodasLasEmpresas()
        {
            return await _httpClient.ObtenerTodasLasEmpresas();
        }

        public async Task<ModelResponse> ObtenerEmpresaPorId(long id)
        {
            return await _httpClient.ObtenerEmpresaPorId(id);
        }

        public async Task<ModelResponse> ObtenerEmpresasPorRFC(string rfc)
        {
            return await _httpClient.ObtenerEmpresasPorRFC(rfc);
        }

        public async Task<ModelResponse> GuardarOActualizarEmpresa(Empresa empresa)
        {
            return await _httpClient.GuardarOActualizarEmpresa(empresa);
        }

        public async Task<ModelResponse> GuardarNuevaEmpresa(Empresa empresa)
        {
            return await _httpClient.GuardarNuevaEmpresa(empresa);
        }

        public async Task<ModelResponse> RegistrarEmpresa(Empresa empresa)
        {
            return await _httpClient.RegistrarEmpresa(empresa);
        }

        public async Task<ModelResponse> GuardarNuevaEmpresaCompleta(Empresa empresa)
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
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Compañías");
            }
            return null;
        }
    }
}