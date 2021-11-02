using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteChecker.Models
{
    public class Person
    {
        public int ID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        [ForeignKey("Role")]
        public int RoleID { get; set; }
        
        public Role Role { get; set; }
        public ICollection<CheckRequest> CheckRequests { get; set; }

    }
}
