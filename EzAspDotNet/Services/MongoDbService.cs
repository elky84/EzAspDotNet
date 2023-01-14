using EzAspDotNet.Settings;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;

namespace EzAspDotNet.Services
{
    public class MongoDbService
    {
        public IMongoDatabase Database { get; private set; }

        public bool IsInitialized { get; set; } = false;

        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = configuration.GetMongoDbSettings().ConnectionString;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MONGODB_CONNECTION")))
            {
                connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION");
            }

            var database = configuration.GetMongoDbSettings().DatabaseName;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MONGODB_DATABASE")))
            {
                database = Environment.GetEnvironmentVariable("MONGODB_DATABASE");
            }

            var client = new MongoClient(connectionString);
            Database = client.GetDatabase(database);
        }
    }
}
