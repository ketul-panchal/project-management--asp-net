using System.ComponentModel.DataAnnotations;
using PMS.Models;

namespace PMS.ViewModels
{
    public class RegisterVM
    {
        [Required, StringLength(80)]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password), MaxLength(6)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password), Compare(nameof(Password)),MaxLength(6)]
        public string ConfirmPassword { get; set; } = "";

        [MaxLength(10)]
        public string PhoneNo {get;set;}="";

        [MaxLength(6)]
        public string Gender {get;set;}="";

        [Required]
        public Role Role { get; set; }
    }
}
