using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Rental_Management.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        [Required]
        public int CarID { get; set; }
        [ForeignKey("CarID")]
        public Car? Car { get; set; }

        [Required]
        public int CustomerID { get; set; }
        [ForeignKey("CustomerID")]
        public User? Customer { get; set; }

        [Required]
        public int LocationID { get; set; }
        [ForeignKey("LocationID")]
        public Location? Location { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        // Driver options
        public bool NeedDriver { get; set; }       // if customer wants company driver
        public int? DriverId { get; set; }         // assigned company driver
        public Driver? Driver { get; set; }

        // Customer’s own driver info
        public string? AltDriverName { get; set; }
        public string? AltDriverIC { get; set; }
        public string? AltDriverLicenseNo { get; set; }

        // Payment & Status
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string? Status { get; set; } // PendingPayment, Paid, Confirmed, Cancelled

        public string? PaymentMethod { get; set; } // "Card" or "Cash"

        public bool RefundIssued { get; set; } // true if refund processed

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RefundAmount { get; set; } // refund amount (nullable)

        // Navigation
        public ICollection<DriverBooking> DriverBookings { get; set; } = new List<DriverBooking>();
    }
}
