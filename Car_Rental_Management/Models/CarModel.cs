using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.Models
{
    public class CarModel
    {
        [Key]
        public int CarModelID { get; set; }

        
        [Required(ErrorMessage = "Brand/Manufacturer is required")]
        [MaxLength(100)]
        public string Brand { get; set; }

        
        [Required(ErrorMessage = "Model Name is required")]
        [MaxLength(100)]
        public string ModelName { get; set; }

       
        [Range(1950, 2100, ErrorMessage = "Enter a valid year")]
        public int Year { get; set; }

        
        [Required(ErrorMessage = "Body type is required")]
        [MaxLength(50)]
        public string BodyType { get; set; }

        
        [Required(ErrorMessage = "Fuel type is required")]
        [MaxLength(50)]
        public string FuelType { get; set; }

        
        [Required(ErrorMessage = "Transmission type is required")]
        [MaxLength(50)]
        public string Transmission { get; set; }

       
        [Range(1, 20, ErrorMessage = "Seating capacity must be between 1 and 20")]
        public int SeatingCapacity { get; set; }

        
        [MaxLength(50)]
        public string EngineCapacity { get; set; }


        [Display(Name = "Car Logo")]
        public string? LogoUrl { get; set; } // will store the logo path (URL or uploaded file)

    }
}
