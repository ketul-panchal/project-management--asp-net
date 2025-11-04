using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS.Data;
using PMS.Models;

namespace PMS.Controllers
{
    [Authorize(Roles = "ProjectManager")]
    public class ProjectManagerController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly IPasswordHasher<AppUser> _hasher;

        public ProjectManagerController(AppDbContext ctx, IPasswordHasher<AppUser> hasher)
        {
            _ctx = ctx;
            _hasher = hasher;
        }
        public IActionResult Index()
        {
            ViewBag.Message = TempData["msg"];
            return View();

            ViewData["HideNavbar"] = true; // Hides navbar
            return View();
        }
        
        
    }
}
