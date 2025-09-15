using Car_Rental_Management.Models;

namespace Car_Rental_Management.ViewModels
{
    public class CustomerBrowseCarVM
    {
        public List<Car> Cars { get; set; } = new();  // List of cars to display

        public List<CarModel> CarModels { get; set; } = new(); // Optional: dropdown filter
    }
}
