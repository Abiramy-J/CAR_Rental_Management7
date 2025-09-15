using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.Models
{
    public class Driver
    {
        [Key]
        public int DriverId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string PhoneNo { get; set; }

        [Required]
        public string NIC { get; set; }

        [Required]
        public string LicenseNo { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
