using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

namespace DataAccess
{
    public interface ITemporalEntity<T>
    {
        ObjectId Id { get; set; }

        IEnumerable<CreateIndexModel<T>> GetIndexes();
    }
}