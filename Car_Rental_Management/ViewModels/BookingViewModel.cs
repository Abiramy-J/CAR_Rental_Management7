namespace Car_Rental_Management.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; }
        public string DriverName { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string CarModel { get; set; }
        public string CarBrand { get; set; }
        public string Status { get; set; }
    }
}