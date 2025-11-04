using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace PMS.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ReportType { get; set; }
        public string UploadedBy { get; set; }
        public int ActiveProjects { get; set; }
        public int OverdueTasks { get; set; }
        public int TeamMembers { get; set; }
        public int CompletionRate { get; set; }
        public DateTime UploadDate { get; set; }

        // actual saved path of file
        public string? FilePath { get; set; }

        // not mapped to DB — just used for file upload form
        [NotMapped]
        public IFormFile? File { get; set; }

        // for report listing page
        [NotMapped]
        public List<Report>? ReportList { get; set; }
    }
}
