using Microsoft.AspNetCore.Mvc;
using PMS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace PMS.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IWebHostEnvironment _env;

        // Simulated role — replace later with your actual authentication system
        private string CurrentUserRole = "Admin"; // or "Manager"

        public ReportsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Temporary in-memory list
        private static List<Report> _reports = new List<Report>
        {
            new Report
            {
                Id = 1,
                Title = "Monthly Overview Report",
                ReportType = "Overview",
                UploadedBy = "Admin",
                ActiveProjects = 3,
                OverdueTasks = 1,
                TeamMembers = 5,
                CompletionRate = 80,
                UploadDate = DateTime.Now.AddDays(-10),
                FilePath = "/uploads/reports/sample-report.pdf"
            }
        };

        // GET: Reports (list page for both Admin & Manager)
        public IActionResult Index()
        {
            ViewBag.Role = CurrentUserRole;
            return View(_reports);
        }

        // GET: Upload (Project Manager uploads)
        public IActionResult Upload()
        {
            return View();
        }

        // POST: Upload
        [HttpPost]
        public IActionResult Upload(Report model)
        {
            if (model.FilePath != null)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "reports");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                string filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.File.CopyTo(stream); 
                }

                model.FilePath = "/uploads/reports/" + fileName;
                model.UploadDate = DateTime.Now;
                model.Id = _reports.Any() ? _reports.Max(r => r.Id) + 1 : 1;
                model.UploadedBy = CurrentUserRole;
                _reports.Add(model);
            }

            return RedirectToAction("Index");
        }

        // GET: Edit
        public IActionResult Edit(int id)
        {
            var report = _reports.FirstOrDefault(r => r.Id == id);
            if (report == null) return NotFound();
            return View(report);
        }

        // POST: Edit
        [HttpPost]
        public IActionResult Edit(Report model)
        {
            var existing = _reports.FirstOrDefault(r => r.Id == model.Id);
            if (existing == null) return NotFound();

            existing.Title = model.Title;
            existing.ReportType = model.ReportType;
            existing.UploadedBy = model.UploadedBy;

            if (model.FilePath != null)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "reports");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                string filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                   model.File.CopyTo(stream);

                }

                existing.FilePath = "/uploads/reports/" + fileName;
            }

            return RedirectToAction("Index");
        }

        // GET: Delete
        public IActionResult Delete(int id)
        {
            var report = _reports.FirstOrDefault(r => r.Id == id);
            if (report == null) return NotFound();

            _reports.Remove(report);
            return RedirectToAction("Index");
        }

        // View PDF
        public IActionResult ViewReport(int id)
        {
            var report = _reports.FirstOrDefault(r => r.Id == id);
            if (report == null) return NotFound();

            string fullPath = Path.Combine(_env.WebRootPath, report.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, "application/pdf");
        }
    }
}
