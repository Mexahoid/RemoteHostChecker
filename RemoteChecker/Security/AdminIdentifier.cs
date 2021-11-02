using RemoteChecker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RemoteChecker.Security
{
    public static class AdminIdentifier
    {
        public static Person CheckIfAdmin(ClaimsPrincipal user, CheckContext _context)
        {
            string role = user.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            string login = user.Identity.Name;
            int? id = (from r in _context.Roles where r.Name == role select r).FirstOrDefault()?.ID;
            Person p = null;
            if (id != null)
            {
                p = (from pr in _context.Persons where pr.Login == login && pr.RoleID == id select pr).FirstOrDefault();
            }
            return p;
        }
    }
}
