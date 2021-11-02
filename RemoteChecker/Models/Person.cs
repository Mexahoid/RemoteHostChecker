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
            CheckRequests = new List<CheckRequest>();
        }
        public int ID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int RoleID { get; set; }

        public Role Role { get; set; }
        public List<CheckRequest> CheckRequests { get; set; }

    }
}
