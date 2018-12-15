using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;


namespace WebApplication
{
    public class TemporalRepository
    {
        public void Create()
        {
            string mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");

            var settings = MongoClientSettings
                .FromUrl(MongoUrl.Create(mongoUri));

            var client = new MongoClient(settings);

            var database = client.GetDatabase("SampleDatabase");

            IMongoCollection<MongoItem> collection = database.GetCollection<MongoItem>("SampleCollection");
            
            for (int i = 0; i < 25; i++)
            {
                var newBusinessObject = new MongoItem()
                {
                    Id = ObjectId.GenerateNewId(),
                    Name = "Test business object 1",
                    Payload = new string('*', 28800)
                };

                collection.InsertOne(newBusinessObject);
            }
        }

        public IEnumerable<MongoItem> GetAll()
        {
            string mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");

            var settings = MongoClientSettings
                .FromUrl(MongoUrl.Create(mongoUri));
            settings.ReadPreference = ReadPreference.Secondary;

            var client = new MongoClient(settings);

            var database = client.GetDatabase("SampleDatabase");

            IMongoCollection<MongoItem> collection = database.GetCollection<MongoItem>("SampleCollection");

            var builder = Builders<MongoItem>.Filter;
            var filter = builder.Where(i => i.Name == "Test business object 1");

            var result = collection.Find(filter).Sort(new SortDefinitionBuilder<MongoItem>() { }.Descending(i => i.Id));

            return result.ToList();
        }
    }
}
