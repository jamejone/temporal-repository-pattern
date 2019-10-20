using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    [MongoEntitySettings(Database = "SampleDatabase", Collection = "SampleCollection")]
    public class MongoItem : TemporalEntityBase
    {
        public string DatabaseName { get { return "SampleDatabase"; } }

        public string CollectionName { get { return "SampleCollection"; } }

        public string Payload { get; set; }
    }
}
