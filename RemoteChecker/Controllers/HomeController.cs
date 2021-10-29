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
            return Redirect("Person");
        }

    }
}
