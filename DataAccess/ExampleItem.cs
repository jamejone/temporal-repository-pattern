using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    [MongoEntitySettings(Database = "ExampleDatabase", Collection = "ExampleCollection")]
    public class ExampleItem : ITemporalEntity<ExampleItem>
    {
        public ObjectId Id { get; set; }

        public int PartitionKey { get; set; }

        public string Payload { get; set; }

        public IEnumerable<CreateIndexModel<ExampleItem>> GetIndexes()
        {
            var index1 = Builders<ExampleItem>.IndexKeys.Combine(
                Builders<ExampleItem>.IndexKeys.Ascending(_ => _.PartitionKey),
                Builders<ExampleItem>.IndexKeys.Ascending(_ => _.Id));

            var index2 = Builders<ExampleItem>.IndexKeys.Hashed(_ => _.PartitionKey);

            yield return new CreateIndexModel<ExampleItem>(index1);
            yield return new CreateIndexModel<ExampleItem>(index2);
        }
    }
}
