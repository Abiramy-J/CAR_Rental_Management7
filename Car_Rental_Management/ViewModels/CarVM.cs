using Car_Rental_Management.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class CarVM
    {
        public Car Car { get; set; }

        [BindNever]
        public IEnumerable<SelectListItem> CarModelList { get; set; }

        [BindNever]
        public List<SelectListItem> StatusList { get; set; }

        // File uploads
        [Display(Name = "Upload Main Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Upload Image 2 (optional)")]
        public IFormFile? ImageFile2 { get; set; }

        [Display(Name = "Upload Image 3 (optional)")]
        public IFormFile? ImageFile3 { get; set; }

        [Display(Name = "Upload Image 4 (optional)")]
        public IFormFile? ImageFile4 { get; set; }
    }
}
