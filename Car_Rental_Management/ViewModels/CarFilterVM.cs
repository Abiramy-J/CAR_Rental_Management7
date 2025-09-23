using Car_Rental_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Car_Rental_Management.ViewModels
{
    public class CarFilterVM
    { 
        public int? SelectedCarModelID { get; set; }
        public decimal? MinRate { get; set; }
        public decimal? MaxRate { get; set; }
        public string Status { get; set; }
        public string Keyword { get; set; }

        
        public DateTime? PickupDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        
        public string SortOrder { get; set; }

      
        public List<SelectListItem> CarModelList { get; set; }
        public List<SelectListItem> StatusList { get; set; }
        public List<Car> CarList { get; set; }
    }
}
