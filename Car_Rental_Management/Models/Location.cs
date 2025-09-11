using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.Models
{
    public class Location
    {
        public int LocationID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }  // e.g., "Colombo Airport"

        [StringLength(200)]
        public string Address { get; set; } // Optional
    }
}
