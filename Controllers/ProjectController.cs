using Microsoft.AspNetCore.Mvc;
using PMS.Data;
using PMS.Models;
using System.Linq;

public class ProjectsController : Controller
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext context)
    {
        _context = context;
    }

    // Show all projects
    public IActionResult Index()
    {
        var projects = _context.Projects.ToList();
        return View(projects);
    }

    // Load Create Project form (PartialView for Modal)
    public IActionResult Create()
    {
        return PartialView("_CreateProjectPartial");
    }

   // Save Project
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(Project project)
{
    if (ModelState.IsValid)
    {
        project.CreatedBy = User.Identity?.Name ?? "Admin"; // fallback if null
        _context.Projects.Add(project);
        _context.SaveChanges();

        return Json(new { success = true });
    }

    return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
}
 [HttpGet]
        public IActionResult Edit(int id)
        {
            var project = _context.Projects.FirstOrDefault(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }
            return PartialView("_EditProjectPartial", project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Project project)
        {
            if (ModelState.IsValid)
            {
                var existingProject = _context.Projects.FirstOrDefault(p => p.Id == project.Id);
                if (existingProject == null)
                {
                    return NotFound();
                }

                // update only editable fields
                existingProject.ProjectName = project.ProjectName;
                existingProject.Description = project.Description;
                existingProject.Status = project.Status;
                existingProject.Priority = project.Priority;
                existingProject.StartDate = project.StartDate;
                existingProject.EndDate = project.EndDate;
                existingProject.ProjectColor = project.ProjectColor;

                // keep CreatedBy intact
                _context.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // -------------------- DELETE --------------------
        [HttpPost]
public IActionResult Delete(int id)
{
    var project = _context.Projects.Find(id);
    if (project == null)
    {
        return Json(new { success = false });
    }

    _context.Projects.Remove(project);
    _context.SaveChanges();
    return Json(new { success = true });
}

    }