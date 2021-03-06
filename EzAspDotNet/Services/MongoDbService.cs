using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using EzAspDotNet.Settings;

namespace EzAspDotNet.Services
{
    public class MongoDbService
    {
        public IMongoDatabase Database { get; private set; }

        public bool IsInitialized { get; set; } = false;

        public MongoDbService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetMongoDbSettings().ConnectionString);
            Database = client.GetDatabase(configuration.GetMongoDbSettings().DatabaseName);
        }
    }
}
