using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class ForgotPasswordViewModel : IValidatableObject
    {
        public string Username { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "New Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // At least username or email must be provided
            if (string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(
                    "Please enter either Username or Email.",
                    new[] { nameof(Username), nameof(Email) });
            }
        }
    }
}
