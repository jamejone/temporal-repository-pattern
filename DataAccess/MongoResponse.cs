using System;
using System.Collections.Generic;

namespace DataAccess
{
    public class MongoResponse<T>
    {
        public List<T> Result { get; set; }
    }
}
