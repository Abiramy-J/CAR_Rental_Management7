using Car_Rental_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Car_Rental_Management.ViewModels
{
    public class CustomerBrowseCarVM
    {
        public List<Car> Cars { get; set; } = new();  // List of cars to display

        // Filter properties
        public int? SelectedCarModelID { get; set; }
        public IEnumerable<SelectListItem> CarModelList { get; set; } = new List<SelectListItem>();

        public decimal? MinRate { get; set; }
        public decimal? MaxRate { get; set; }

        public string? Status { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; } = new List<SelectListItem>();

        public string? Keyword { get; set; }
    }
}
