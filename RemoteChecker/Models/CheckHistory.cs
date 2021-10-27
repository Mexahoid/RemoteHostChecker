using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteChecker.Models
{
    public class CheckHistory
    {
        public int ID { get; set; }
        public DateTime Moment { get; set; }
        public int Result { get; set; }

        public int CheckID { get; set; }
        public CheckRequest CheckRequest { get; set; }
    }
}
