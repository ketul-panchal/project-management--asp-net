using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;

// ✅ Fix the enum clash with System.Threading.Tasks.TaskStatus
using TaskStatusEnum = PMS.Models.TaskStatus;

namespace PMS.Controllers
{
    public class TasksController : Controller
    {
        private readonly AppDbContext _db;

        public TasksController(AppDbContext db)
        {
            _db = db;
        }

        // A lightweight VM for list/views (no dependency on Project.Name)
        public sealed class TaskListItemVm
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string? Description { get; set; }
            public int ProjectId { get; set; }
            public string ProjectDisplay { get; set; } = "";
            public TaskStatusEnum Status { get; set; }
            public TaskPriority Priority { get; set; }
            public DateTime? DueDate { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        // Utility: dropdown items for Projects
        private async Task PopulateProjectDropdownAsync(int? selectedId = null)
        {
            var items = await _db.Projects
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = "Project #" + p.Id
                })
                .ToListAsync();

            ViewBag.ProjectItems = new SelectList(items, "Value", "Text", selectedId?.ToString());
        }


        // GET: /Tasks
        // GET: /Tasks
        public async Task<IActionResult> Index(string search, string status, string priority, int? projectId)
        {
            // Load projects for filter dropdown
            ViewBag.Projects = await _db.Projects.ToListAsync();

            var query = _db.Tasks.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.Contains(search) || (t.Description != null && t.Description.Contains(search)));
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TaskStatusEnum>(status, out var statusEnum))
            {
                query = query.Where(t => t.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(priority) && Enum.TryParse<TaskPriority>(priority, out var priorityEnum))
            {
                query = query.Where(t => t.Priority == priorityEnum);
            }

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            var list = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TaskListItemVm
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    ProjectId = t.ProjectId ?? 0,
                    ProjectDisplay = "Project #" + t.ProjectId,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return View(list);
        }

        // Add this method to your existing TasksController

        // GET: /Tasks/Kanban
        public async Task<IActionResult> Kanban()
        {
            var list = await _db.Tasks
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .Select(t => new TaskListItemVm
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    ProjectId = t.ProjectId ?? 0,
                    ProjectDisplay = "Project #" + t.ProjectId,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            // Add anti-forgery token for AJAX calls
            ViewData["AntiForgeryToken"] = HttpContext.RequestServices
                .GetService<IAntiforgery>()?.GetAndStoreTokens(HttpContext).RequestToken;

            return View(list);
        }

        // POST: /Tasks/QuickCreate
        [HttpPost]
        public async Task<IActionResult> QuickCreate(TaskItem model)
        {
            try
            {
                model.CreatedAt = DateTime.UtcNow;
                _db.Tasks.Add(model);
                await _db.SaveChangesAsync();
                return Json(new { success = true, taskId = model.Id });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        // GET: /Tasks/Create
        public async Task<IActionResult> Create()
        {
            await PopulateProjectDropdownAsync();
            return View();
        }

        // POST: /Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateProjectDropdownAsync(model.ProjectId);
                return View(model);
            }

            // Defaults & safety
            if (!Enum.IsDefined(typeof(TaskStatusEnum), model.Status))
                model.Status = TaskStatusEnum.Todo;

            model.CreatedAt = DateTime.UtcNow;

            _db.Tasks.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Tasks/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            await PopulateProjectDropdownAsync(task.ProjectId);
            return View(task);
        }

        // POST: /Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateProjectDropdownAsync(model.ProjectId);
                return View(model);
            }

            // Attach & update safe fields
            var existing = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null) return NotFound();

            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.ProjectId = model.ProjectId;
            existing.Status = model.Status;              // ✅ uses TaskStatusEnum alias
            existing.Priority = model.Priority;
            existing.DueDate = model.DueDate;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: /Tasks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task != null)
            {
                _db.Tasks.Remove(task);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Tasks/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, TaskStatusEnum status)
        {
            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            task.Status = status; // ✅ no ambiguity
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
