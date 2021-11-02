using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteChecker.Models
{
    public class Role
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<Person> Persons { get; set; }

        public Role()
        {
            Persons = new List<Person>();
        }
    }
}
