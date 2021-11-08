using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RemoteChecker.CheckerLogics;
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
            //await r.CallCheckRequestTasks(_context);


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
            Person p = Security.AdminIdentifier.CheckIfAdmin(User, _context);
            ViewData["admin"] = p != null && p.Role.Name == "Администратор";

            if (id == null)
            {
                return NotFound();
            }

            var checkRequest = await _context.CheckRequests
                .Include(c => c.Person)
                .Include(c => c.CheckHistories)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (checkRequest == null)
            {
                return NotFound();
            }

            if (checkRequest.PersonID == p.ID || p.Role.Name == "Администратор")
                return View(checkRequest);
            else
                return RedirectToAction(nameof(Unauthorized), new { message = "Попытка просмотра истории чужого чекера" });
        }


        public IActionResult Unauthorized(string message)
        {
            ViewData["err"] = message;
            return View();
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


            Person p = Security.AdminIdentifier.CheckIfAdmin(User, _context);
            if (checkRequest.PersonID != p.ID && p.Role.Name != "Администратор")
                return RedirectToAction(nameof(Unauthorized), new { message = "Попытка смены состояния активности чужого чекера" });

            checkRequest.Active = !checkRequest.Active;
            _context.Update(checkRequest);
            await _context.SaveChangesAsync();


            Worker r = Worker.GetInstance();
            await r.ChangeCheckRequestActivity(checkRequest);


            return Redirect(Request.Headers["Referer"].ToString());
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

                Worker r = Worker.GetInstance();
                await r.AddCheckRequest(checkRequest);

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

            Person p = Security.AdminIdentifier.CheckIfAdmin(User, _context);
            if (checkRequest.PersonID != p.ID && p.Role.Name != "Администратор")
                return RedirectToAction(nameof(Unauthorized), new { message = "Попытка изменения чужого чекера" });
            
            ViewData["PersonID"] = checkRequest.PersonID;
            return View(checkRequest);
        }

        // GET: CheckRequests/Update/5
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkRequest = await _context.CheckRequests
                .Include(c => c.CheckHistories)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (checkRequest == null)
            {
                return NotFound();
            }

            Person p = Security.AdminIdentifier.CheckIfAdmin(User, _context);
            if (checkRequest.PersonID != p.ID && p.Role.Name != "Администратор")
                return RedirectToAction(nameof(Unauthorized), new { message = "Попытка запуска чужого чекера" });


            Worker r = Worker.GetInstance();
            int res = await r.ForceCheckRequest(checkRequest);

            CheckHistory ch = new()
            {
                CheckID = checkRequest.ID,
                Moment = DateTime.Now,
                Result = res,
                CheckRequest = checkRequest
            };
            _context.Add(ch);
            await _context.SaveChangesAsync();


            return Redirect(Request.Headers["Referer"].ToString());

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


            Person p = Security.AdminIdentifier.CheckIfAdmin(User, _context);
            if (checkRequest.PersonID != p.ID && p.Role.Name != "Администратор")
                return RedirectToAction(nameof(Unauthorized), new { message = "Попытка изменения чужого чекера" });


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(checkRequest);
                    await _context.SaveChangesAsync();

                    Worker r = Worker.GetInstance();
                    await r.EditCheckRequest(checkRequest);
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


            Person p = Security.AdminIdentifier.CheckIfAdmin(User, _context);
            if (checkRequest.PersonID != p.ID && p.Role.Name != "Администратор")
                return RedirectToAction(nameof(Unauthorized), new { message = "Попытка удаления чужого чекера" });

            return View(checkRequest);
        }

        // POST: CheckRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var checkRequest = await _context.CheckRequests.FindAsync(id);

            var l = _context.CheckHistories.Where(x => x.CheckRequest == checkRequest).ToList();
            foreach (var item in l)
            {
                _context.Remove(item);
            }

            _context.CheckRequests.Remove(checkRequest);
            await _context.SaveChangesAsync();

            Worker r = Worker.GetInstance();
            await r.RemoveCheckRequest(checkRequest);
            return RedirectToAction(nameof(Index));
        }

        private bool CheckRequestExists(int id)
        {
            return _context.CheckRequests.Any(e => e.ID == id);
        }



    }
}
