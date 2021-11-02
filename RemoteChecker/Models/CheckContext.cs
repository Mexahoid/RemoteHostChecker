using System;
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
        public DbSet<Role> Roles { get; set; }

        public CheckContext(DbContextOptions<CheckContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            string adminRoleName = "Администратор";
            string userRoleName = "Пользователь";
            string adminLogin = "admin";
            string adminPassword = "Pa$$w0rd";
            adminPassword = Security.PasswordManager.HashPassword(adminLogin, adminPassword);

            Role adminRole = new() { ID = 1, Name = adminRoleName };
            Role userRole = new() { ID = 2, Name = userRoleName };

            Person admin = new() { ID = 1, Login = adminLogin, Password = adminPassword, RoleID = adminRole.ID };

            modelBuilder.Entity<Role>().HasData(new Role[] { adminRole, userRole });
            modelBuilder.Entity<Person>().HasData(new Person[] { admin });

            base.OnModelCreating(modelBuilder);
        }
    }
}
