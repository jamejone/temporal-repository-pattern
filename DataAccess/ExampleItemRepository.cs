using MongoDB.Bson;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    public class ExampleItemRepository : TemporalRepository<ExampleItem>
    {
        public ExampleItemRepository(ConfigurationModel config) : base(config) { }

        public void CreateMany()
        {
            string payload = new string('*', 1024);
            int payloads = 5 * 1024;
            int numPartitionKeys = 6;

            for (int i = 0; i < payloads; i++)
            {
                var newBusinessObject = new ExampleItem()
                {
                    Id = ObjectId.GenerateNewId(),
                    ShardKey = i % numPartitionKeys,
                    Payload = payload
                };

                this.Save(newBusinessObject);
            }
        }
    }
}
