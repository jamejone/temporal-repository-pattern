using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class MongoItem
    {
        public ObjectId Id { get; set; }

        public string Payload { get; set; }
    }
}
