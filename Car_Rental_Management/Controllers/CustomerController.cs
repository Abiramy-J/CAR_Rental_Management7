using Car_Rental_Management.Data; // Assuming this is the namespace for your DbContext
using Car_Rental_Management.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Ensure this is included

namespace Car_Rental_Management.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -----------------------------
        // Browse available cars
        // -----------------------------
        public IActionResult BrowseCars()
        {
            var vm = new CustomerBrowseCarVM
            {
                Cars = _context.Cars
                               .Include(c => c.CarModel)
                               .Where(c => c.Status == "Available")
                               .ToList(),
                CarModels = _context.CarModels.ToList()
            };

            return View(vm);
        }


        public IActionResult Dashboard()
        {
            return View();
        }

        

        public IActionResult MyBookings()
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return RedirectToAction("Login", "Account");

                var bookings = _context.Bookings
                    .Include(b => b.Car).ThenInclude(c => c.CarModel)
                   // .Include(b => b.Payment)
                    .Where(b => b.CustomerID == userId.Value)
                    .ToList();

                return View(bookings);
            }

}
}
