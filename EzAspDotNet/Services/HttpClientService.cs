using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace EzAspDotNet.Services
{
    public class HttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IConfiguration _configuration;

        public HttpClientService(IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public IHttpClientFactory Factory => _httpClientFactory;

        public IConfiguration Configuration => _configuration;
    }
}
