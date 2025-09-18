using Microsoft.AspNetCore.Mvc;

namespace Car_Rental_Management.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
