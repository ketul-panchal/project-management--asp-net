using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using PMS.Data;
using PMS.Models;

namespace PMS.Models
{
    public enum Role
    {
        Admin = 1,
        ProjectManager = 2
    }

    public class AppUser
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = string.Empty;

        // Store a hash, never plain text
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public Role Role { get; set; }

        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? ContactNo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
