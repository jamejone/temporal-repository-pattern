using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    [System.AttributeUsage(System.AttributeTargets.Class |
                          System.AttributeTargets.Struct)]
    public class MongoEntitySettings : Attribute
    {
        public string Database;
        public string Collection;
    }
}
