using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RemoteChecker.Models;
using RemoteChecker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RemoteChecker.Controllers
{
    public class AccountController : Controller
    {
        private readonly CheckContext db;
        public AccountController(CheckContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Login,Password")] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Login))
            {
                ModelState.AddModelError("Login", "Логин не может быть пустым");
            }
            else if (!(from p in db.Persons where p.Login == model.Login select p).Any())
            {
                ModelState.AddModelError("Login", "Такой логин не существует");
            }
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Пароль не может быть пустым");
            }
            if (!ModelState.IsValid)
                return View(model);

            string pwd = Security.PasswordManager.HashPassword(model.Login, model.Password);

            Person person = await db.Persons
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(
                        u => u.Login == model.Login && 
                        Security.PasswordManager.ValidatePassword(model.Login, model.Password, pwd)
                );

            if (person == null)
            {
                ModelState.AddModelError("Login", "Неверный пароль");
                return View(model);
            }

            await Authenticate(person); // аутентификация
            return RedirectToAction("Index", "");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Login,Password,ConfirmPassword")] RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.Login))
            {
                ModelState.AddModelError("Login", "Логин не может быть пустым");
            }
            else if ((from p in db.Persons where p.Login == model.Login select p).Any())
            {
                ModelState.AddModelError("Login", "Такой логин уже существует");
            }
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Пароль не должен быть пустым");
            }
            if (model.ConfirmPassword != model.Password)
            {
                ModelState.AddModelError("ConfirmPassword", "Пароли не совпадают");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string pwd = Security.PasswordManager.HashPassword(model.Login, model.Password);
            Person pers = new() { Login = model.Login, Password = pwd };
            Role r = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Пользователь");
            if (r != null)
                pers.Role = r;

            db.Persons.Add(pers);
            await db.SaveChangesAsync();
            await Authenticate(pers); // аутентификация
            return RedirectToAction("Index", "");
        }

        private async Task Authenticate(Person person)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, person.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role?.Name)
            }; 
            ClaimsIdentity id = new(claims, "ApplicationCookie", 
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "");
        }
    }
}
