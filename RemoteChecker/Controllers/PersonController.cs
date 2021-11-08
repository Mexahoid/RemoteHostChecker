using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RemoteChecker.Models;

namespace RemoteChecker.Controllers
{
    [Authorize(Roles="Администратор")]
    public class PersonController : Controller
    {
        private readonly CheckContext _context;

        public PersonController(CheckContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            var a = await _context.Persons
                    .Include(u => u.Role)
                    .Include(u => u.CheckRequests)
                    .ToListAsync();

            var b = a[0].Role;

            return View(a);
        }


        public async Task<IActionResult> ActiveChange(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkRequest = await _context.CheckRequests
                .Include(c => c.Person)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (checkRequest == null)
            {
                return NotFound();
            }

            checkRequest.Active = !checkRequest.Active;
            _context.Update(checkRequest);
            await _context.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"].ToString());
        }


        // GET: Person/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            Person p = Security.AdminIdentifier.CheckIfAdmin(User, _context);
            ViewData["admin"] = p != null && p.Role.Name == "Администратор";

            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Persons
                .Include(u => u.Role)
                .Include(u => u.CheckRequests)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // GET: Person/Create
        public IActionResult Create()
        {
            return View();
        }


        // GET: Person/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Persons
                .Include(p => p.CheckRequests)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: Person/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var person = await _context.Persons.FindAsync(id);

            var c = _context.CheckRequests.Where(x => x.Person == person).ToList();
            foreach (var cr in c)
            {
                var l = _context.CheckHistories.Where(x => x.CheckRequest == cr).ToList();
                foreach (var item in l)
                {
                    _context.Remove(item);
                }
                _context.Remove(cr);
            }
            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(int id)
        {
            return _context.Persons.Any(e => e.ID == id);
        }
    }
}
