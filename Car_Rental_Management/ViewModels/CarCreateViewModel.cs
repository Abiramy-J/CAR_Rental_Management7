using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class CarCreateViewModel
    {
        [Required]
        public int CarModelID { get; set; }

        [Required]
        public string Color { get; set; }

        [Required]
        public int Mileage { get; set; }

        [Required]
        public decimal DailyRate { get; set; }

        // Either paste a URL or upload a file
        [Display(Name = "Main Image URL (optional)")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Upload Image (optional)")]
        public IFormFile? ImageFile { get; set; }

        //public string? VideoUrl { get; set; }
        public string? Description { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
