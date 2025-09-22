using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required, Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(07|21)\d{8}$", ErrorMessage = "Phone number must start with 07 or 21 and be 10 digits long")]
        public string PhoneNumber { get; set; }
    }
}
