using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    public class MongoItemRepository : TemporalRepository<MongoItem>
    {
        public void CreateMany()
        {
            for (int i = 0; i < 25; i++)
            {
                var newBusinessObject = new MongoItem()
                {
                    Id = ObjectId.GenerateNewId(),
                    Payload = new string('*', 16)
                };

                this.Create(newBusinessObject);
            }
        }
    }
}
