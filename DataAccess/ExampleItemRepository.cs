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
            for (int i = 0; i < 25; i++)
            {
                var newBusinessObject = new ExampleItem()
                {
                    Id = ObjectId.GenerateNewId(),
                    Payload = new string('*', 16)
                };

                this.Create(newBusinessObject);
            }
        }
    }
}
