using System;
using System.ComponentModel.DataAnnotations;

namespace PMS.Models
{
    public class TeamMember
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Developer";

        [Phone]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? Avatar { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "active";

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}