using Microsoft.AspNetCore.Mvc;
using PMS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace PMS.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public ReportsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        private static List<Report> _reports = new List<Report>
        {
            new Report
            {
                Id = 1,
                Title = "Q1 2024 Project Overview",
                ReportType = "Overview",
                UploadedBy = "Admin",
                ActiveProjects = 3,
                OverdueTasks = 1,
                TeamMembers = 5,
                CompletionRate = 80,
                UploadDate = DateTime.Now.AddDays(-10),
                FilePath = "/uploads/reports/sample-report.pdf"
            },
            new Report
            {
                Id = 2,
                Title = "Development Progress Report",
                ReportType = "Progress",
                UploadedBy = "Manager",
                ActiveProjects = 2,
                OverdueTasks = 0,
                TeamMembers = 3,
                CompletionRate = 95,
                UploadDate = DateTime.Now.AddDays(-5),
                FilePath = "/uploads/reports/sample-report.pdf"
            }
        };

        // Allowed file extensions
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".txt", ".pptx", ".ppt" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        // GET: Reports Index
        public IActionResult Index()
        {
            return View(_reports);
        }

        // GET: Upload
        public IActionResult Upload()
        {
            return View();
        }

        // POST: Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Report model)
        {
            if (model.File != null && model.File.Length > 0)
            {
                // Validate file extension
                var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("File", $"Invalid file type. Allowed types: {string.Join(", ", _allowedExtensions)}");
                    return View(model);
                }

                // Validate file size
                if (model.File.Length > _maxFileSize)
                {
                    ModelState.AddModelError("File", "File size cannot exceed 10MB");
                    return View(model);
                }

                try
                {
                    var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "reports");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    string fileName = $"{Guid.NewGuid()}{extension}";
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.File.CopyToAsync(stream);
                    }

                    model.FilePath = "/uploads/reports/" + fileName;
                    model.UploadDate = DateTime.Now;
                    model.Id = _reports.Any() ? _reports.Max(r => r.Id) + 1 : 1;
                    model.UploadedBy = User.Identity?.Name ?? "Unknown User";

                    _reports.Add(model);

                    TempData["SuccessMessage"] = "Report uploaded successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading file: " + ex.Message);
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("File", "Please select a file to upload");
                return View(model);
            }
        }

        // GET: Edit (ONLY ADMIN CAN ACCESS)
        public IActionResult Edit(int id)
        {
            // Only Admin can edit
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Only administrators can edit reports.";
                return RedirectToAction("Index");
            }

            var report = _reports.FirstOrDefault(r => r.Id == id);

            if (report == null)
                return NotFound();

            return View(report);
        }

        // POST: Edit (ONLY ADMIN CAN ACCESS)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Report model)
        {
            // Only Admin can edit
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Only administrators can edit reports.";
                return RedirectToAction("Index");
            }

            var existing = _reports.FirstOrDefault(r => r.Id == model.Id);

            if (existing == null)
                return NotFound();

            existing.Title = model.Title;
            existing.ReportType = model.ReportType;
            existing.ActiveProjects = model.ActiveProjects;
            existing.OverdueTasks = model.OverdueTasks;
            existing.TeamMembers = model.TeamMembers;
            existing.CompletionRate = model.CompletionRate;

            // If a new file is uploaded
            if (model.File != null && model.File.Length > 0)
            {
                var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("File", "Invalid file type");
                    return View(model);
                }

                if (model.File.Length > _maxFileSize)
                {
                    ModelState.AddModelError("File", "File size cannot exceed 10MB");
                    return View(model);
                }

                try
                {
                    var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "reports");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    string fileName = $"{Guid.NewGuid()}{extension}";
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.File.CopyToAsync(stream);
                    }

                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(existing.FilePath))
                    {
                        var oldFilePath = Path.Combine(_env.WebRootPath, existing.FilePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    existing.FilePath = "/uploads/reports/" + fileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading file: " + ex.Message);
                    return View(model);
                }
            }

            TempData["SuccessMessage"] = "Report updated successfully!";
            return RedirectToAction("Index");
        }

        // GET: Delete (ONLY ADMIN CAN ACCESS)
        public IActionResult Delete(int id)
        {
            // Only Admin can delete
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Only administrators can delete reports.";
                return RedirectToAction("Index");
            }

            var report = _reports.FirstOrDefault(r => r.Id == id);

            if (report == null)
                return NotFound();

            // Delete physical file
            if (!string.IsNullOrEmpty(report.FilePath))
            {
                var filePath = Path.Combine(_env.WebRootPath, report.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _reports.Remove(report);
            TempData["SuccessMessage"] = "Report deleted successfully!";
            return RedirectToAction("Index");
        }
        // Download/View File
        public IActionResult ViewReport(int id)
        {
            var report = _reports.FirstOrDefault(r => r.Id == id);
            if (report == null) return NotFound();

            string fullPath = Path.Combine(_env.WebRootPath, report.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            var extension = Path.GetExtension(fullPath).ToLowerInvariant();

            string contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };

            return File(fileBytes, contentType);
        }

        // Download File
        public IActionResult DownloadReport(int id)
        {
            var report = _reports.FirstOrDefault(r => r.Id == id);
            if (report == null) return NotFound();

            string fullPath = Path.Combine(_env.WebRootPath, report.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            var fileName = report.Title + Path.GetExtension(fullPath);

            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}