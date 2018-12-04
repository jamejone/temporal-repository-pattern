using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");

            var settings = MongoClientSettings
                .FromUrl(MongoUrl.Create(mongoUri));

            var client = new MongoClient(settings);

            var database = client.GetDatabase("SampleDatabase");

            database.CreateCollection("SampleCollection");

            IMongoCollection<SampleBusinessObject> collection = database.GetCollection<SampleBusinessObject>("SampleCollection");

            var newBusinessObject = new SampleBusinessObject()
            {
                Id = ObjectId.GenerateNewId(),
                Name = "Test business object 1"
            };

            collection.InsertOne(newBusinessObject);

            //var findFilter = new FilterDefinition<SampleBusinessObject>();

            //var fetchedBusinessObject = collection.FindAsync();
        }
    }
}
