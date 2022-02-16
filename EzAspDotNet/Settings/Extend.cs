using Microsoft.Extensions.Configuration;

namespace EzAspDotNet.Settings
{
    public static class Extend
    {
        public static DatabaseSettings GetDatabaseSettings(this IConfiguration configuration)
        {
            var databaseSection = configuration.GetSection("Database");
            return databaseSection.Get<DatabaseSettings>();
        }

        public static GatewaySettings GetGatewaySettings(this IConfiguration configuration)
        {
            var databaseSection = configuration.GetSection("Gateway");
            return databaseSection.Get<GatewaySettings>();
        }

        public static RabbitMqSettings GetRabbitMqSettings(this IConfiguration configuration)
        {
            var serviceSection = configuration.GetSection("RabbitMq");
            return serviceSection.Get<RabbitMqSettings>();
        }

        public static MongoDbSettings GetMongoDbSettings(this IConfiguration configuration)
        {
            var databaseSection = configuration.GetSection("MongoDb");
            return databaseSection.Get<MongoDbSettings>();
        }

        public static ElasticSearch ElasticSearch(this IConfiguration configuration)
        {
            return configuration.GetSection("ElasticSearch").Get<ElasticSearch>();
        }
    }
}
