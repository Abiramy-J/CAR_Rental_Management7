using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class DriverEditVM
    {
        public int DriverId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string PhoneNo { get; set; }

        [Required]
        public string NIC { get; set; }

        [Required]
        public string LicenseNo { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }
    }
}