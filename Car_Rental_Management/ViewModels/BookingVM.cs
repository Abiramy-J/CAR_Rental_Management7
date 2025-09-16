using Car_Rental_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class BookingVM
    {
        public int CarID { get; set; }
        public Car? Car { get; set; }

        [Required]
        [Display(Name = "Pickup Location")]
        public int LocationID { get; set; }
        public List<SelectListItem>? LocationList { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        [Display(Name = "Need Company Driver?")]
        public bool NeedDriver { get; set; }

        // Alternate driver info
        public string? AltDriverName { get; set; }
        public string? AltDriverIC { get; set; }
        public string? AltDriverLicenseNo { get; set; }

        public int? SelectedDriverId { get; set; } // nullable in case customer uses own driver
        public List<SelectListItem>? AvailableDrivers { get; set; }

    }
}
