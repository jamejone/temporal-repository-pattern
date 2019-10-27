using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared;
using Polly;

namespace DataAccess
{
    public class TemporalRepository<T> where T : ITemporalEntity<T>, new()
    {
        private readonly ConfigurationModel _config;
        private readonly AsyncPolicy _retryPolicy;

        public TemporalRepository(ConfigurationModel config)
        {
            _config = config;
            
            _retryPolicy = Policy
                .Handle<MongoWaitQueueFullException>()
                .WaitAndRetryAsync(config.NumTransientFaultRetries, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Log(retryAttempt))
                 );
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

        /// <summary>
        /// Saves the entity to the database. In a temporal data store, 
        /// all records are immutable. So every save results in a new record
        /// saved to the database.
        /// </summary>
        public async Task SaveAsync(T entity)
        {
            IMongoCollection<T> collection = GetMongoCollection();

            await _retryPolicy.ExecuteAsync(() => collection.InsertOneAsync(entity));
        }

        /// <summary>
        /// Retrieves the latest version of a given entity.
        /// </summary>
        public async Task<T> GetAsync(string identifier)
        {
            IMongoCollection<T> collection = GetMongoCollection();

            var filter = new FilterDefinitionBuilder<T>()
                .Eq(_ => _.Identifier, identifier);

            var arrayResult = new List<T>();

            var findQuery = collection
                .Find(filter)
                .Limit(1)
                .Sort(new SortDefinitionBuilder<T>() { }.Descending(i => i.Id));

            return await _retryPolicy.ExecuteAsync(() => findQuery.FirstOrDefaultAsync());
        }

        /// <summary>
        /// Retrieves the version of a given entity as of the time specified.
        /// </summary>
        public async Task<T> GetAsync(string identifier, DateTime asOf)
        {
            IMongoCollection<T> collection = GetMongoCollection();

            var filter = new FilterDefinitionBuilder<T>().And(
                Builders<T>.Filter.Eq(_ => _.Identifier, identifier),
                Builders<T>.Filter.Lt(_ => _.Id, ObjectId.GenerateNewId(asOf)));

            var arrayResult = new List<T>();

            var findQuery = collection
                .Find(filter)
                .Limit(1)
                .Sort(new SortDefinitionBuilder<T>() { }.Descending(i => i.Id));

            return await _retryPolicy.ExecuteAsync(() => findQuery.FirstOrDefaultAsync());
        }

        /// <summary>
        /// Retrieves the historical states of an entity.
        /// </summary>
        public async IAsyncEnumerable<T> GetHistoryAsync(string identifier)
        {
            IMongoCollection<T> collection = GetMongoCollection();

            var filter = new FilterDefinitionBuilder<T>()
                .Eq(_ => _.Identifier, identifier);

            var arrayResult = new List<T>();

            var findQuery = collection
                .Find(filter)
                .Sort(new SortDefinitionBuilder<T>() { }.Descending(i => i.Id));

            var cursor = await _retryPolicy.ExecuteAsync(() => findQuery.ToCursorAsync());

            foreach (var item in cursor.ToEnumerable())
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<string> GetAllIdentifiersAsync()
        {
            IMongoCollection<T> collection = GetMongoCollection();

            var emptyFilter = new FilterDefinitionBuilder<T>().Empty;

            var distinctQuery = collection.DistinctAsync(_ => _.Identifier, emptyFilter);

            var asyncCursor = await _retryPolicy.ExecuteAsync(() => distinctQuery);

            foreach (var item in asyncCursor.ToEnumerable())
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<string> GetAllIdentifiersAsync(DateTime asOf)
        {
            IMongoCollection<T> collection = GetMongoCollection();

            var emptyFilter = new FilterDefinitionBuilder<T>()
                .Lt(_ => _.Id, ObjectId.GenerateNewId(asOf));

            var distinctQuery = collection.DistinctAsync(_ => _.Identifier, emptyFilter);

            var asyncCursor = await _retryPolicy.ExecuteAsync(() => distinctQuery);

            foreach (var item in asyncCursor.ToEnumerable())
            {
                yield return item;
            }
        }

        /// <summary>
        /// Retrieves the latest version of all the entities from the database.
        /// </summary>
        public async IAsyncEnumerable<T> GetAllAsync()
        {
            IMongoCollection<T> collection = GetMongoCollection();

            await foreach (string identifier in GetAllIdentifiersAsync())
            {
                var filter = new FilterDefinitionBuilder<T>()
                    .Eq(_ => _.Identifier, identifier);

                var findQuery = collection
                    .Find(filter)
                    .Limit(1)
                    .Sort(new SortDefinitionBuilder<T>() { }.Descending(i => i.Id));

                var result = await _retryPolicy.ExecuteAsync(() => findQuery.FirstOrDefaultAsync());

                yield return result;
            }
        }

        /// <summary>
        /// Fetches all entites from the database as of a particular point in time.
        /// </summary>
        public async IAsyncEnumerable<T> GetAllAsync(DateTime asOf)
        {
            IMongoCollection<T> collection = GetMongoCollection();

            await foreach (string identifier in GetAllIdentifiersAsync(asOf))
            {
                var filter = new FilterDefinitionBuilder<T>()
                    .And(
                        Builders<T>.Filter.Eq(_ => _.Identifier, identifier),
                        Builders<T>.Filter.Lt(_ => _.Id, ObjectId.GenerateNewId(asOf)));

                var findQuery = collection
                    .Find(filter)
                    .Limit(1)
                    .Sort(new SortDefinitionBuilder<T>() { }.Descending(i => i.Id));

                var result = await _retryPolicy.ExecuteAsync(() => findQuery.FirstOrDefaultAsync());

                yield return result;
            }
        }

        /// <summary>
        /// Purges historical versions from the database, keeping only the most recent versions.
        /// This has a permanent side effect on the database and you will no longer be able to
        /// reliably retrieve records beyond this date ever again, for which there are no safeguards.
        /// </summary>
        public async Task PurgeHistoricalVersionsAsync(DateTime howFarBackToPurge, int minVersionsToKeep = 1)
        {
            IMongoCollection<T> collection = GetMongoCollection();

            await foreach (string identifier in GetAllIdentifiersAsync(howFarBackToPurge))
            {
                var filter = new FilterDefinitionBuilder<T>()
                    .And(
                        Builders<T>.Filter.Eq(_ => _.Identifier, identifier),
                        Builders<T>.Filter.Lt(_ => _.Id, ObjectId.GenerateNewId(howFarBackToPurge)));

                var findQuery = collection
                    .Find(filter)
                    .Skip(minVersionsToKeep - 1)
                    .Limit(1)
                    .Sort(new SortDefinitionBuilder<T>() { }.Descending(i => i.Id));

                var result = await _retryPolicy.ExecuteAsync(() => findQuery.FirstOrDefaultAsync());

                if (result != null)
                {
                    var deleteFilter = new FilterDefinitionBuilder<T>()
                        .And(
                            Builders<T>.Filter.Eq(_ => _.Identifier, identifier),
                            Builders<T>.Filter.Lt(_ => _.Id, result.Id));

                    await _retryPolicy.ExecuteAsync(() => collection.DeleteManyAsync(deleteFilter));
                }
            }
        }
    }
}
