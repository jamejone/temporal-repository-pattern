using System;
using System.Collections.Generic;

namespace DataAccess
{
    public class MongoResponse<T>
    {
        public DateTime LastOperationTime { get; set; }

        public List<T> Result { get; set; }
    }
}
