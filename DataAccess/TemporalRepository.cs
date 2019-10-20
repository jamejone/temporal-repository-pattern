using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccess
{
    public class TemporalRepository<T> where T : TemporalEntityBase
    {
        private string MongoUri = Environment.GetEnvironmentVariable("MONGO_URI");

        public void Create(T newEntity)
        {
            var settings = MongoClientSettings
                .FromUrl(MongoUrl.Create(MongoUri));
            settings.WriteConcern = WriteConcern.Acknowledged;

            var client = new MongoClient(settings);

            var mongoEntitySettings = typeof(T).GetCustomAttributes(typeof(MongoEntitySettings), true);
            if (!mongoEntitySettings.Any()) throw new InvalidOperationException("Entity must have a MongoEntitySettings attribute.");
            var mongoEntitySettingsCast = mongoEntitySettings.First() as MongoEntitySettings;

            var database = client.GetDatabase(mongoEntitySettingsCast.Database);

            IMongoCollection<T> collection = database.GetCollection<T>(mongoEntitySettingsCast.Collection);

            collection.InsertOne(newEntity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();

            var settings = MongoClientSettings
                .FromUrl(MongoUrl.Create(MongoUri));

            var client = new MongoClient(settings);

            var mongoEntitySettings = typeof(T).GetCustomAttributes(typeof(MongoEntitySettings), true);
            if (!mongoEntitySettings.Any()) throw new InvalidOperationException("Entity must have a MongoEntitySettings attribute.");
            var mongoEntitySettingsCast = mongoEntitySettings.First() as MongoEntitySettings;

            var database = client.GetDatabase(mongoEntitySettingsCast.Database);

            IMongoCollection<T> collection = database.GetCollection<T>(mongoEntitySettingsCast.Collection);

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
