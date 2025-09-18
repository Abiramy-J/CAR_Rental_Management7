// ViewModels/CreateUserViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class CreateUserViewModel
    {

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
