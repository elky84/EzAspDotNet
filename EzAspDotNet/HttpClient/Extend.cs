using EzAspDotNet.Constants;
using EzAspDotNet.Exception;
using EzAspDotNet.Protocols;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EzAspDotNet.HttpClient
{
    public static class Extend
    {
        public static void SetDefaultHeader(this System.Net.Http.HttpClient client, HttpRequestMessage request, string userId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add(HeaderKeys.InternalServer, System.Reflection.Assembly.GetEntryAssembly().GetName().Name);
            if (userId != null)
            {
                request.Headers.Add(HeaderKeys.AuthorizedUserId, userId);
            }
        }

        private static async Task CheckError(this HttpResponseMessage httpResponseMessage)
        {
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var responseHeader = JsonConvert.DeserializeObject<ResponseHeader>(await httpResponseMessage.Content.ReadAsStringAsync());
                if (responseHeader != null)
                {
                    throw new DeveloperException(responseHeader.ResultCode);
                }
                else
                {
                    throw new DeveloperException(Protocols.Code.ResultCode.HttpError, httpResponseMessage.StatusCode);
                }
            }
        }

        public static async Task<T> Request<T>(this IHttpClientFactory httpClientFactory,
            HttpMethod httpMethod,
            string url,
            string userId = null,
            Action<HttpRequestMessage> preAction = null) where T : ResponseHeader
        {
            using var client = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(httpMethod, url);
            client.SetDefaultHeader(request, userId);
            preAction?.Invoke(request);
            var response = await client.SendAsync(request);
            await CheckError(response);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public static async Task<T> RequestJson<T>(this IHttpClientFactory httpClientFactory,
                HttpMethod httpMethod,
                string url,
                string body,
                string userId = null) where T : ResponseHeader
        {
            using var client = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(httpMethod, url);
            client.SetDefaultHeader(request, userId);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            await CheckError(response);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }


        public static async Task<T> RequestJson<T>(this IHttpClientFactory httpClientFactory,
            HttpMethod httpMethod,
            string url,
            object body,
            string userId = null) where T : ResponseHeader
        {
            return await httpClientFactory.RequestJson<T>(httpMethod, url, JsonConvert.SerializeObject(body), userId);
        }


        public static async Task<T> RequestJson<T>(this IHttpClientFactory httpClientFactory,
            HttpMethod httpMethod,
            string url,
            object body)
        {
            var response = await httpClientFactory.RequestJson(httpMethod, url, JsonConvert.SerializeObject(body));
            return await response.ResponseDeserialize<T>();
        }

        public static async Task<HttpResponseMessage> RequestJson(this IHttpClientFactory httpClientFactory,
            HttpMethod httpMethod,
            string url,
            object body)
        {
            using var client = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(httpMethod, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };
            client.SetDefaultHeader(request, null);
            return await client.SendAsync(request);
        }


        public static async Task<T> Request<T>(this IHttpClientFactory httpClientFactory,
            HttpMethod httpMethod, string url)
        {
            using var client = httpClientFactory.CreateClient();
            return await client.Request<T>(httpMethod, url);
        }

        public static async Task<T> Request<T>(this System.Net.Http.HttpClient httpClient,
            HttpMethod httpMethod, string url)
        {
            var request = new HttpRequestMessage(httpMethod, url);
            var httpResponse = await httpClient.SendAsync(request);
            return await httpResponse.ResponseDeserialize<T>();
        }

        public static async Task<T> Request<T>(this System.Net.Http.HttpClient httpClient,
            HttpMethod httpMethod, string url, object body)
        {
            var request = new HttpRequestMessage(httpMethod, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            var httpResponse = await httpClient.SendAsync(request);
            return await httpResponse.ResponseDeserialize<T>();
        }

        private static async Task<T> ResponseDeserialize<T>(this HttpResponseMessage httpResponse)
        {
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
            {
                Log.Logger.Error($"ResponseSerialize Failed. <StatusCode:{httpResponse.StatusCode}> <Url:{httpResponse.RequestMessage.Method} {httpResponse.RequestMessage.RequestUri}> <Content:{httpResponse.RequestMessage.Content}>");
                return default;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (System.Exception ex)
            {
                Log.Logger.Error($"ResponseSerialize Failed. <Exception:{ex.Message}> <StatusCode:{httpResponse.StatusCode}> <Url:{httpResponse.RequestMessage.Method} {httpResponse.RequestMessage.RequestUri}> <Content:{httpResponse.RequestMessage.Content}>");
                ex.ExceptionLog();
                return default;
            }
        }
    }
}
