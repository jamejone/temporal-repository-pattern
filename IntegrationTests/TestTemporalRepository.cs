using DataAccess;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public class TestTemporalRepository<T> : TemporalRepository<T> where T : ITemporalEntity<T>, new()
    {
        public TestTemporalRepository(ConfigurationModel config) : base(config)
        {
        }

        public async Task DropDatabaseAsync()
        {
            var mongoEntitySettings = GetMongoEntitySettings();

            MongoClient client = GetMongoClient();

            await client.DropDatabaseAsync(mongoEntitySettings.Database);
        }

        public async Task DropCollectionAsync()
        {
            var mongoEntitySettings = GetMongoEntitySettings();

            IMongoDatabase database = GetMongoDatabase(mongoEntitySettings.Database);

            await database.DropCollectionAsync(mongoEntitySettings.Collection);
        }

        public async Task CreateCollectionAsync()
        {
            var mongoEntitySettings = GetMongoEntitySettings();

            IMongoDatabase database = GetMongoDatabase(mongoEntitySettings.Database);

            await database.CreateCollectionAsync(mongoEntitySettings.Collection);

            IMongoCollection<T> collection = GetMongoCollection();

            var instance = new T();

            IEnumerable<CreateIndexModel<T>> indexesToCreate = instance.GetIndexes();

            await collection.Indexes.CreateManyAsync(indexesToCreate);
        }

        internal async Task Healthcheck()
        {
            var client = GetMongoClient();

            var result = await client.ListDatabaseNamesAsync();

            result.ToList();
        }
    }
}
