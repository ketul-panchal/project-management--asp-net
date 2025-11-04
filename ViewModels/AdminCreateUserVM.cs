using System.ComponentModel.DataAnnotations;
using PMS.Models;

namespace PMS.ViewModels
{
    public class AdminCreateUserVM
    {
        [Required, StringLength(80)]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password), MinLength(8)]
        public string Password { get; set; } = "";

        [Required]
        public Role Role { get; set; }

        public string? Gender { get; set; }
        public string? Address { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be 10 digits")]
        public string? ContactNo { get; set; }
    }
}
