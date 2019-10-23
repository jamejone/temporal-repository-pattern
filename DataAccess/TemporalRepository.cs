using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared;

namespace DataAccess
{
    public class TemporalRepository<T> where T : ITemporalEntity<T>, new()
    {
        private readonly ConfigurationModel _config;

        public TemporalRepository(ConfigurationModel config)
        {
            _config = config;
        }

        protected MongoEntitySettings GetMongoEntitySettings()
        {
            var mongoEntitySettings = typeof(T).GetCustomAttributes(typeof(MongoEntitySettings), true);
            if (!mongoEntitySettings.Any()) throw new InvalidOperationException("Entity must have a MongoEntitySettings attribute.");
            var mongoEntitySettingsCast = mongoEntitySettings.First() as MongoEntitySettings;

            return mongoEntitySettingsCast;
        }

        protected MongoClient GetMongoClient()
        {
            var settings = MongoClientSettings
                .FromUrl(MongoUrl.Create(_config.MongoUri));
            settings.WriteConcern = WriteConcern.Acknowledged;

            var client = new MongoClient(settings);

            return client;
        }

        protected IMongoDatabase GetMongoDatabase(string databaseName)
        {
            MongoClient client = GetMongoClient();

            IMongoDatabase database = client.GetDatabase(databaseName);

            return database;
        }

        protected IMongoCollection<T> GetMongoCollection()
        {
            var mongoEntitySettings = GetMongoEntitySettings();

            IMongoDatabase database = GetMongoDatabase(mongoEntitySettings.Database);

            IMongoCollection<T> collection = database.GetCollection<T>(mongoEntitySettings.Collection);

            return collection;
        }

        public void Create(T newEntity)
        {
            IMongoCollection<T> collection = GetMongoCollection();

            collection.InsertOne(newEntity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();

            IMongoCollection<T> collection = GetMongoCollection();

            var filter = new FilterDefinitionBuilder<T>().Empty;

            var arrayResult = new List<T>();

            var findQuery = collection
                .Find(filter)
                .Sort(new SortDefinitionBuilder<T>() { }.Descending(i => i.Id));

            await findQuery.ForEachAsync(
                item =>
                    {
                        arrayResult.Add(item);
                    }
                );

            timer.Stop();
            Console.WriteLine($"Mongo GetAll query took: {timer.ElapsedMilliseconds} ms.");

            return arrayResult;
        }
    }
}
