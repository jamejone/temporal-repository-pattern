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

        public async Task<MongoResponse<MongoItem>> GetAllAsync()
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();

            var response = new MongoResponse<MongoItem>();

            string mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");

            var settings = MongoClientSettings
                .FromUrl(MongoUrl.Create(mongoUri));

            Tag tag = new Tag("secondary", "1");
            TagSet tagSet = new TagSet(new List<Tag>() { tag });
            settings.ReadPreference = new ReadPreference(ReadPreferenceMode.Secondary, new List<TagSet>() { tagSet });
            settings.ReadConcern = ReadConcern.Local;

            var client = new MongoClient(settings);

            var adminDatabase = client.GetDatabase("admin");

            BsonDocument statsDocument = adminDatabase.RunCommand<BsonDocument>(new BsonDocument("isMaster", 1), ReadPreference.Secondary);

            BsonValue optime = statsDocument["lastWrite"]["opTime"]["ts"];
            response.LastOperationTime = new DateTime(1970, 1, 1).AddSeconds(optime.AsBsonTimestamp.Timestamp);

            BsonBoolean isSecondary = statsDocument["secondary"] as BsonBoolean;
            if (!isSecondary.AsBoolean)
                throw new ApplicationException("Reads are only supposed to be against secondary nodes.");

            var database = client.GetDatabase("SampleDatabase");

            IMongoCollection<MongoItem> collection = database.GetCollection<MongoItem>("SampleCollection");

            var builder = Builders<MongoItem>.Filter;

            var filter = new FilterDefinitionBuilder<MongoItem>().Empty;

            var arrayResult = new List<MongoItem>();

            var findQuery = collection
                .Find(filter)
                .Sort(new SortDefinitionBuilder<MongoItem>() { }.Descending(i => i.Id))
                .Project<MongoItem>(Builders<MongoItem>.Projection.Exclude(i => i.Payload));

            await findQuery.ForEachAsync(
                item =>
                    {
                        arrayResult.Add(item);
                    }
                );

            timer.Stop();
            Console.WriteLine($"Mongo GetAll query took: {timer.ElapsedMilliseconds} ms.");

            response.Result = arrayResult;

            return response;
        }
    }
}
