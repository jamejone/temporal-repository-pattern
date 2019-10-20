using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    public abstract class TemporalEntityBase : ITemporalEntity
    {
        public ObjectId Id { get; set; }
    }
}
