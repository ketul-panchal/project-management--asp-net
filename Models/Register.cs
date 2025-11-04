using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PMS.Models
{
    public class Register
    {
        [Key]
         public int Id { get; set; }

        [Required, StringLength(80)]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        public Role Role { get; set; }
    }

}
