﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RemoteChecker.Models;

namespace RemoteChecker.Controllers
{
    [Authorize]
    public class CheckRequestsController : Controller
    {
        private readonly CheckContext _context;

        public CheckRequestsController(CheckContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            Person p = Security.AdminIdentifier.CheckIfAdmin(User, _context);
            ViewData["admin"] = p != null && p.Role.Name == "Администратор";
            int id = p.RoleID;
            switch (id)
            {
                case 1:
                    var checkContext = _context.CheckRequests.Include(c => c.Person);
                    return View(await checkContext.ToListAsync());
                case 2:
                    var checkContext2 = _context.CheckRequests.Include(c => c.Person).Where(c => c.Person.ID == p.ID);
                    return View(await checkContext2.ToListAsync());
                default:
                    return Redirect("Error");
            }

        }

        // GET: CheckRequests/Details/5
        public async Task<IActionResult> Details(int? id)
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

            return View(checkRequest);
        }


        // GET: CheckRequests/ActiveChange/5
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

            //return View();
        }

        // GET: CheckRequests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CheckRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,HostAddress,Cron,Active,PersonID")] CheckRequest checkRequest)
        {

            string login = User.Identity.Name;
            int? id = (from r in _context.Persons where r.Login == login select r).FirstOrDefault()?.ID;
            checkRequest.PersonID = (int)id;

            if (ModelState.IsValid)
            {
                _context.Add(checkRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(checkRequest);
        }

        // GET: CheckRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
            ViewData["PersonID"] = checkRequest.PersonID;
            return View(checkRequest);
        }

        // POST: CheckRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,HostAddress,Cron,Active,PersonID")] CheckRequest checkRequest)
        {
            if (id != checkRequest.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(checkRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CheckRequestExists(checkRequest.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(checkRequest);
        }

        // GET: CheckRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(checkRequest);
        }

        // POST: CheckRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var checkRequest = await _context.CheckRequests.FindAsync(id);
            _context.CheckRequests.Remove(checkRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CheckRequestExists(int id)
        {
            return _context.CheckRequests.Any(e => e.ID == id);
        }
    }
}
