using Elasticsearch.Net;
using EzAspDotNet.Settings;
using Microsoft.Extensions.Configuration;
using Nest;
using System;

namespace EzAspDotNet.Services
{
    public class QueryService
    {
        private readonly ElasticClient _elasticClient;

        public ElasticClient Client => _elasticClient;

        public QueryService(IConfiguration configuration)
        {
            var nodes = new Uri[]
            {
                new Uri(configuration.ElasticSearch().Host)
            };

            var pool = new StaticConnectionPool(nodes);
            var settings = new ConnectionSettings(pool);
            _elasticClient = new ElasticClient(settings);
        }
    }
}
