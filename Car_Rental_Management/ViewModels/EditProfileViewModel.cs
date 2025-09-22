using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class EditProfileViewModel
    {
        public int UserId { get; set; }

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

        public string? ProfileImageUrl { get; set; }

        // For file upload
        public IFormFile? ProfileImage { get; set; }
    }
}
