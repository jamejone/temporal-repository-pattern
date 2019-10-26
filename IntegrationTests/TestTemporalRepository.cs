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

        public async Task ClearCollectionAsync()
        {
            IMongoCollection<T> collection = GetMongoCollection();
            await collection.DeleteManyAsync(_ => true);
        }

        internal async Task Healthcheck()
        {
            var client = GetMongoClient();

            var result = await client.ListDatabaseNamesAsync();

            result.ToList();
        }

        internal async Task<IEnumerable<T>> GetAllIncludingAllVersionsAsync()
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
    }
}
