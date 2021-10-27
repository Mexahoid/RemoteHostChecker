using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteChecker.TestData
{
    public static class TestFields
    {
        public static void Initialize(Models.CheckContext context)
        {
            if (!context.Persons.Any())
            {
                context.Persons.AddRange(
                    new Models.Person
                    {
                        Login = "admin",
                        Password = "admin",
                        Role = 1
                    },
                    new Models.Person
                    {
                        Login = "user1",
                        Password = "pass1",
                        Role = 0
                    },
                    new Models.Person
                    {
                        Login = "user2",
                        Password = "pass2",
                        Role = 0
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
