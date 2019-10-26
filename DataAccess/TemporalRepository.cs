﻿using System;
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

        /// <summary>
        /// Saves a document to the database. In a temporal data store, 
        /// all records are immutable. So every save results in a new record
        /// saved to the database.
        /// </summary>
        public void Save(T entity)
        {
            IMongoCollection<T> collection = GetMongoCollection();

            collection.InsertOne(entity);
        }

        /// <summary>
        /// Retrieves all the documents for this collection from the database.
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
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

            return arrayResult;
        }

        /// <summary>
        /// Fetches all records created as of a particular point in time.
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync(DateTime asOf)
        {
            IMongoCollection<T> collection = GetMongoCollection();

            var filter = new FilterDefinitionBuilder<T>()
                .Lt(_ => _.Id, ObjectId.GenerateNewId(asOf));

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

            return arrayResult;
        }

        public async Task PurgeHistoricalVersions(DateTime howFarBackToPurge, int minVersionsToKeep = 1)
        {
            IMongoCollection<T> collection = GetMongoCollection();

            var distinctFilter = new FilterDefinitionBuilder<T>()
                .Lt(_ => _.Id, ObjectId.GenerateNewId(howFarBackToPurge));

            var distinctQuery = collection.DistinctAsync(_ => _.Identifier, distinctFilter);

            var asyncCursor = await distinctQuery;

            var identifierList = new List<string>();
            await asyncCursor.ForEachAsync(identifier =>
            {
                identifierList.Add(identifier);
            });

            foreach (string identifier in identifierList)
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

                var result = await findQuery.FirstOrDefaultAsync();

                if (result != null)
                {
                    var deleteFilter = new FilterDefinitionBuilder<T>()
                        .And(
                            Builders<T>.Filter.Eq(_ => _.Identifier, identifier),
                            Builders<T>.Filter.Lt(_ => _.Id, result.Id));

                    await collection.DeleteManyAsync(deleteFilter);
                }
            }
        }
    }
}
