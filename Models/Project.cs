using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using PMS.Data;
using PMS.Models;

namespace PMS.Models
{
    public class Project
    {
        public int Id { get; set; }
        public required string ProjectName { get; set; }
        public required string Description { get; set; }
        public required string Status { get; set; }
        public required string Priority { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ProjectColor { get; set; }
        public  string? CreatedBy { get; set; }  // Project Manager Name or Id
    }


}