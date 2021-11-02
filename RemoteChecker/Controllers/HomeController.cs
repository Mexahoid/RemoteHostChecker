using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RemoteChecker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RemoteChecker.Controllers
{
    public class HomeController : Controller
    {
        CheckContext db;

        public HomeController(CheckContext context)
        {
            db = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            string login = User.Identity.Name;
            int? id = (from r in db.Roles where r.Name == role select r).FirstOrDefault()?.ID;
            Person p = null;
            if (id != null)
            {
                p = (from pr in db.Persons where pr.Login == login select pr).FirstOrDefault();
            }
            switch (id)
            {
                case 1:
                    return Redirect("Person");
                case 2:
                    return View(p);
                default:
                    return Redirect("Error");
            }

        }

    }
}
