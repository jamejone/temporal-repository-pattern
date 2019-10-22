using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared;

namespace DataAccess
{
    public class TemporalRepository<T> where T : TemporalEntityBase
    {
        private readonly ConfigurationModel _config;

        public TemporalRepository(ConfigurationModel config)
        {
            _config = config;
        }

        private IMongoCollection<T> GetCollection()
        {
            var mongoEntitySettings = typeof(T).GetCustomAttributes(typeof(MongoEntitySettings), true);
            if (!mongoEntitySettings.Any()) throw new InvalidOperationException("Entity must have a MongoEntitySettings attribute.");
            var mongoEntitySettingsCast = mongoEntitySettings.First() as MongoEntitySettings;

            var settings = MongoClientSettings
                .FromUrl(MongoUrl.Create(_config.MongoUri));
            settings.WriteConcern = WriteConcern.Acknowledged;

            var client = new MongoClient(settings);

            var database = client.GetDatabase(mongoEntitySettingsCast.Database);

            IMongoCollection<T> collection = database.GetCollection<T>(mongoEntitySettingsCast.Collection);

            return collection;
        }

        public void Create(T newEntity)
        {
            IMongoCollection<T> collection = GetCollection();

            collection.InsertOne(newEntity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();

            IMongoCollection<T> collection = GetCollection();

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
