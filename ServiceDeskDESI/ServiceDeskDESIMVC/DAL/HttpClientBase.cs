using Newtonsoft.Json;
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
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using (var st = new StreamReader(await httpResponseMessage.Content.ReadAsStreamAsync()))
                    {
                        return JsonConvert.DeserializeObject<T>(await st.ReadToEndAsync());
                    }
                }
                else
                {
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