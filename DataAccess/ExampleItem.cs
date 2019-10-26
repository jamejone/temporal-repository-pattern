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
        /// <summary>
        /// The MongoDB object ID. This field uniquely identifies the record and 
        /// contains information about when the object was created. In a temporal
        /// data store you can think of this as a version field.
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// The MongoDB shard key. Documents with matching shard keys will be
        /// colocated on the same replica set.
        /// </summary>
        public int ShardKey { get; set; }

        /// <summary>
        /// The primary identifier of the object.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Increases the size of the document to facilitate sharding.
        /// </summary>
        public string Payload { get; set; }

        public IEnumerable<CreateIndexModel<ExampleItem>> GetIndexes()
        {
            var index1 = Builders<ExampleItem>.IndexKeys.Combine(
                Builders<ExampleItem>.IndexKeys.Ascending(_ => _.Identifier),
                Builders<ExampleItem>.IndexKeys.Ascending(_ => _.Id));

            var index2 = Builders<ExampleItem>.IndexKeys.Hashed(_ => _.ShardKey);

            yield return new CreateIndexModel<ExampleItem>(index1);
            yield return new CreateIndexModel<ExampleItem>(index2);
        }
    }
}
