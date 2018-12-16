using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;


namespace DataAccess
{
    public class TemporalRepository
    {
        public void Create()
        {
            string mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");

            var settings = MongoClientSettings
                .FromUrl(MongoUrl.Create(mongoUri));
            settings.WriteConcern = WriteConcern.Acknowledged;

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

        public MongoResponse<MongoItem> GetAll()
        {
            var response = new MongoResponse<MongoItem>();

            string mongoUri = Environment.GetEnvironmentVariable("MONGO_SECONDARY");

            var settings = MongoClientSettings
                .FromUrl(MongoUrl.Create(mongoUri));
            settings.ReadPreference = ReadPreference.Secondary;
            settings.ReadConcern = ReadConcern.Local;

            var client = new MongoClient(settings);

            var adminDatabase = client.GetDatabase("admin");

            BsonDocument statsDocument = adminDatabase.RunCommand<BsonDocument>(new BsonDocument("replSetGetStatus", 1));

            BsonValue optime = statsDocument["optimes"]["readConcernMajorityOpTime"]["ts"];
            response.LastOperationTime = new DateTime(1970, 1, 1).AddSeconds(optime.AsBsonTimestamp.Timestamp);

            var database = client.GetDatabase("SampleDatabase");

            IMongoCollection<MongoItem> collection = database.GetCollection<MongoItem>("SampleCollection");

            var builder = Builders<MongoItem>.Filter;
            var filter = builder.Where(i => i.Name == "Test business object 1");

            var result = collection.Find(filter).Sort(new SortDefinitionBuilder<MongoItem>() { }.Descending(i => i.Id));

            response.Result = result.ToList();

            return response;
        }
    }
}
