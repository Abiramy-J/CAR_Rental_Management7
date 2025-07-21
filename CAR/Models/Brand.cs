using System.ComponentModel.DataAnnotations;

namespace CAR.Models
{
    public class Brand
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Display (Name = "Established Year")]
        public int EstablishYear { get; set; }
        [Display(Name = "Brand Logo")]
        public string brandLogo { get; set; }


        
    }
}
