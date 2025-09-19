using Car_Rental_Management.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Rental_Management.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class BookingManagementController : Controller
    {
        private readonly ApplicationDbContext _db;
        public BookingManagementController(ApplicationDbContext db) => _db = db;

        // Show all bookings
        public IActionResult Index()
        {
            var bookings = _db.Bookings
                .Include(b => b.Car).ThenInclude(c => c.CarModel)
                .Include(b => b.Customer)
                .ToList();

            return View(bookings);
        }

        // Show one booking details
        public IActionResult Details(int id)
        {
            var booking = _db.Bookings
                .Include(b => b.Car).ThenInclude(c => c.CarModel)
                .Include(b => b.Customer)
                .Include(b => b.DriverBookings).ThenInclude(dbk => dbk.Driver)
                .FirstOrDefault(b => b.BookingID == id);

            if (booking == null) return NotFound();

            return View(booking);
        }
    }
}
