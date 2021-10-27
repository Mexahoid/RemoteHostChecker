using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteChecker.Models
{
    public class Person
    {
        public Person()
        {
            Role = false;
        }

        public int ID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool Role { get; set; }

    }
}
