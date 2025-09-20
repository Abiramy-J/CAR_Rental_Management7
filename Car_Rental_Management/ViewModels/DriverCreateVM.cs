using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class DriverCreateVM
    {
        // User details
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        // Driver details
        [Required]
        public string NIC { get; set; }

        [Required]
        public string LicenseNo { get; set; }
    }
}
