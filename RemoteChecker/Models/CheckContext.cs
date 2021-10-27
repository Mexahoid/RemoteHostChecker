﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RemoteChecker.Models
{
    public class CheckContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }
        public DbSet<CheckRequest> CheckRequests { get; set; }
        public DbSet<CheckHistory> CheckHistories { get; set; }

        public CheckContext(DbContextOptions<CheckContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
