using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Rental_Management.Models
{
    public class DriverBooking
    {
        [Key]
        public int DriverBookingId { get; set; }

        [Required]
        public int DriverId { get; set; }
        [ForeignKey("DriverId")]
        public Driver? Driver { get; set; }

        [Required]
        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        public DateTime PickupDateTime { get; set; }

        [Required]
        public DateTime ReturnDateTime { get; set; }
    }
}
