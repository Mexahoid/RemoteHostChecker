using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteChecker.Models
{
    public class CheckRequest
    {
        public CheckRequest()
        {
            CheckHistories = new List<CheckHistory>();
        }
        public int ID { get; set; }
        public string HostAddress { get; set; }
        public string Cron { get; set; }
        public bool Active { get; set; }

        public int PersonID { get; set; }
        public Person Person { get; set; }
        public List<CheckHistory> CheckHistories { get; set; }
    }
}
