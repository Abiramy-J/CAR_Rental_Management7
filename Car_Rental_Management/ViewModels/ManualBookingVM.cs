using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class ManualBookingVM
    {
        public int BookingID { get; set; }

        [Required] public int CarId { get; set; }
        public string CarModelName { get; set; } = "";
        public decimal CarDailyRate { get; set; } = 0m;     
        public decimal DriverDailyRate { get; set; } = 0m;   

        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "Full name required")] public string FullName { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        [EmailAddress] public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Location required")] public int LocationId { get; set; }
        public List<SelectListItem> LocationList { get; set; } = new();

        [Required] public DateTime PickupDate { get; set; }
        [Required] public DateTime ReturnDate { get; set; }

        public decimal Total { get; set; }

        public bool NeedDriver { get; set; }
        public int? SelectedDriverId { get; set; }
        public List<SelectListItem> AvailableDrivers { get; set; } = new();

        public string? AltDriverName { get; set; }
        public string? AltDriverIC { get; set; }
        public string? AltDriverLicenseNo { get; set; }

        public DateTime? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }
        public bool IsPaid { get; set; } = false;
        // <-- Add this property for NeedDriver dropdown
        public List<SelectListItem> NeedDriverOptions { get; set; } = new List<SelectListItem>();

    }
}

