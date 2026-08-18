using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class PersonaService
    {
        private readonly HttpClientConnection _httpClient;

        public PersonaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse> ObtenerPersonaPorId(long id)
        {
            return await _httpClient.ObtenerPersonaPorId(id);
        }

        public async Task<ModelResponse> GuardarOActualizarPersona(Persona persona)
        {
            return await _httpClient.GuardarOActualizarPersona(persona);
        }

        public async Task<ModelResponse> EliminarPersona(Persona persona)
        {
            return await _httpClient.EliminarPersona(persona);
        }

        public async Task<ModelResponse> ConsultarTodasLasPersonas()
        {
            return await _httpClient.ObtenerTodasLasPersonas();
        }

        public async Task<object> ObtenerPermisosParaPersona()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "People");
            }
            return null;
        }
    }
}