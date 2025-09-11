// ViewModels/CreateUserViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } // "Admin" or "Staff"
    }
}
