using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models
{
    public class IndexModel
    {
        public BusinessObjectItem[] BusinessObjectItems { get; set; }

        public int NumberOfItems { get; set; }

        public DateTime LastOperationTime { get; set; }
    }
}
