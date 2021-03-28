using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESTest.Model
{
    public class ESConnection
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int Limit { get; set; }
    }
}
