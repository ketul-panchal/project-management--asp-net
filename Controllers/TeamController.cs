using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;

namespace PMS.Controllers
{
    public class TeamController : Controller
    {
        private readonly AppDbContext _db;

        public TeamController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Team
        public async Task<IActionResult> Index(string search, string role)
        {
            var query = _db.TeamMembers.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => 
                    m.Name.Contains(search) || 
                    m.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(m => m.Role == role);
            }

            var members = await query
                .OrderBy(m => m.Name)
                .ToListAsync();

            // Pass stats to view
            ViewBag.TotalMembers = await _db.TeamMembers.CountAsync();
            ViewBag.ActiveMembers = await _db.TeamMembers.CountAsync(m => m.Status == "active");
            ViewBag.Developers = await _db.TeamMembers.CountAsync(m => m.Role == "Developer");
            ViewBag.Designers = await _db.TeamMembers.CountAsync(m => m.Role == "Designer");

            return View(members);
        }

        // GET: /Team/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Team/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamMember member)
        {
            if (!ModelState.IsValid)
            {
                return View(member);
            }

            // Generate avatar URL
            member.Avatar = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(member.Name)}&background=3b82f6&color=fff";
            member.JoinDate = DateTime.UtcNow;
            member.CreatedAt = DateTime.UtcNow;

            _db.TeamMembers.Add(member);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Team member added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Team/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var member = await _db.TeamMembers.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }
    }
}