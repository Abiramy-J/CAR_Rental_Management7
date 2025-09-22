using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class DriverCreateVM
    { // User details
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full Name can only contain letters and spaces")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(07|21)\d{8}$", ErrorMessage = "Phone number must start with 07 or 21 and be 10 digits long")]
        public string PhoneNumber { get; set; }

        // Driver details
        [Required(ErrorMessage = "NIC is required")]
        [RegularExpression(@"^[0-9]{9}[VvXx]$|^[0-9]{12}$",
            ErrorMessage = "Enter a valid NIC number (e.g. 123456789V or 200012345678)")]
        public string NIC { get; set; }

        [Required(ErrorMessage = "License number is required")]
        [RegularExpression(@"^[A-Z0-9-]{6,15}$",
            ErrorMessage = "Enter a valid license number (6–15 characters, letters/numbers only)")]
        public string LicenseNo { get; set; }
    }
}
