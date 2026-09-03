using Newtonsoft.Json;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ServiceDeskDESIMVC.DAL
{
    public class HttpClientBase
    {
        private HttpClient httpClient;
        private string BaseUri;
        public HttpClientBase(string baseUrl)
        {

            if (string.IsNullOrEmpty(baseUrl))
            {
                BaseUri = ConfigurationManager.AppSettings["BaseUriWebApi"];
            }
            else
            {
                BaseUri = baseUrl;
            }


            httpClient = new HttpClient()
            {
                BaseAddress = new Uri(BaseUri),
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        public async Task<T> TokenAsync<T>(string endPoint, IEnumerable<KeyValuePair<string, string>> content, string contentType = "application/json")
        {
            SetParametersHttpCliente(contentType, string.Empty);
            using (HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(endPoint, new FormUrlEncodedContent(content)))
            {
                var body = await httpResponseMessage.Content.ReadAsStringAsync();

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    Log.Information("TokenAsync OK: Endpoint={Endpoint}, StatusCode={StatusCode}", endPoint, (int)httpResponseMessage.StatusCode);
                    return JsonConvert.DeserializeObject<T>(body);
                }
                else
                {
                    Log.Warning("TokenAsync FALLÓ: Endpoint={Endpoint}, StatusCode={StatusCode}, Body={Body}", endPoint, (int)httpResponseMessage.StatusCode, body);
                    return default(T);
                }
            }
        }
        public async Task<T> RequestAsync<T>(string endPoint, HttpMethod method, T content, Func<string, T> func, string token = "", string contentType = "application/json") where T : class
        {
            // Siempre configurar headers: limpia cualquier Authorization residual de una
            // llamada previa en la misma instancia de HttpClient (evita fugas de token).
            SetParametersHttpCliente(contentType, token);

            using (var r = new HttpRequestMessage()
            {
                Content = content != null ? new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, contentType) : null,
                Method = method,
                RequestUri = new Uri(httpClient.BaseAddress, endPoint)
            })
            using (var responseMessage = await httpClient.SendAsync(r))
            {
                var stringContent = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.IsSuccessStatusCode)
                {
                    return func?.Invoke(stringContent);
                }

                // Respuesta no exitosa: devolver un ModelResponse de error en lugar de null,
                // para que los DAL no lancen NullReferenceException (result.ToString()).
                var error = new
                {
                    IsSuccess = false,
                    Message = $"Error {(int)responseMessage.StatusCode} ({responseMessage.ReasonPhrase}) al consumir {endPoint}.",
                    Response = (object)null
                };
                return func?.Invoke(JsonConvert.SerializeObject(error));
            }
        }
        public async Task<ModelResponse<TResponse>> RequestAsync<TResponse>(string endPoint, HttpMethod method, object content, string token = "", string contentType = "application/json")
        {
            // Siempre configurar headers: limpia cualquier Authorization residual de una
            // llamada previa en la misma instancia de HttpClient (evita fugas de token).
            SetParametersHttpCliente(contentType, token);

            using (var r = new HttpRequestMessage()
            {
                Content = content != null ? new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, contentType) : null,
                Method = method,
                RequestUri = new Uri(httpClient.BaseAddress, endPoint)
            })
            using (var responseMessage = await httpClient.SendAsync(r))
            {
                var stringContent = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<ModelResponse<TResponse>>(stringContent);
                }

                // Respuesta no exitosa: devolver un ModelResponse<TResponse> de error en lugar de null,
                // para que los DAL no lancen NullReferenceException.
                return new ModelResponse<TResponse>
                {
                    IsSuccess = false,
                    Message = $"Error {(int)responseMessage.StatusCode} ({responseMessage.ReasonPhrase}) al consumir {endPoint}.",
                    Response = default(TResponse)
                };
            }
        }
        public async Task<byte[]> RequestAsyncByteArray(string endPoint, HttpMethod method, object content, string token = "", string contentType = "application/json")
        {
            byte[] b = null;

            SetParametersHttpCliente(contentType, token);

            using (var r = new HttpRequestMessage()
            {
                Content = content != null ? new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, contentType) : null,
                Method = method,
                RequestUri = new Uri(httpClient.BaseAddress, endPoint)
            })
            using (var responseMessage = await httpClient.SendAsync(r))
            {
                if (responseMessage.IsSuccessStatusCode)
                {
                    var stringContent = await responseMessage.Content.ReadAsByteArrayAsync();

                    return stringContent;
                }
                else
                {
                    return b;
                }
            }
        }
        public async Task<ModelResponse<T>> SendMultipartAsync<T>(string endPoint, MultipartFormDataContent content, string token = "")
        {
            // Configurar Accept + Authorization; el Content-Type multipart lo fija el propio content.
            SetParametersHttpCliente("application/json", token);

            try
            {
                using (var responseMessage = await httpClient.PostAsync(endPoint, content))
                {
                    var stringContent = await responseMessage.Content.ReadAsStringAsync();

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return JsonConvert.DeserializeObject<ModelResponse<T>>(stringContent);
                    }

                    Log.Warning("SendMultipartAsync FALLÓ: Endpoint={Endpoint}, StatusCode={StatusCode}, Body={Body}", endPoint, (int)responseMessage.StatusCode, stringContent);
                    return new ModelResponse<T>
                    {
                        IsSuccess = false,
                        Message = $"Error {(int)responseMessage.StatusCode} ({responseMessage.ReasonPhrase}) al consumir {endPoint}.",
                        Response = default(T)
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SendMultipartAsync EXCEPCIÓN: Endpoint={Endpoint}, Inner={Inner}", endPoint, ex.InnerException != null ? ex.InnerException.Message : "(sin inner)");
                return new ModelResponse<T>
                {
                    IsSuccess = false,
                    Message = $"No se pudo enviar la solicitud a {endPoint}: {ex.Message}",
                    Response = default(T)
                };
            }
        }
        public async Task<EvidenciaDescargaDTO> RequestFileAsync(string endPoint, string token = "")
        {
            SetParametersHttpCliente("application/json", token);

            using (var responseMessage = await httpClient.GetAsync(endPoint))
            {
                if (responseMessage.IsSuccessStatusCode)
                {
                    var dto = new EvidenciaDescargaDTO
                    {
                        Contenido = await responseMessage.Content.ReadAsByteArrayAsync(),
                        ContentType = responseMessage.Content.Headers.ContentType?.MediaType ?? "application/octet-stream"
                    };

                    var disposition = responseMessage.Content.Headers.ContentDisposition;
                    if (disposition != null && !string.IsNullOrWhiteSpace(disposition.FileName))
                    {
                        dto.NombreArchivo = disposition.FileName.Trim('"');
                    }

                    return dto;
                }

                Log.Warning("RequestFileAsync FALLÓ: Endpoint={Endpoint}, StatusCode={StatusCode}", endPoint, (int)responseMessage.StatusCode);
                return new EvidenciaDescargaDTO { Contenido = null };
            }
        }
        private void SetParametersHttpCliente(string contentType, string token)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            if (!token.Equals(string.Empty))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}