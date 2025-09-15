using Car_Rental_Management.Data;
using Car_Rental_Management.Models;
using Car_Rental_Management.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Car_Rental_Management.Controllers
{
    public class CustomerBookingController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CustomerBookingController(ApplicationDbContext db) => _db = db;

        // GET: BookCar
        public IActionResult BookCar(int id)
        {
            // Ensure the user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Ensure user exists in Users table
            var userExists = _db.Users.Any(u => u.UserId == userId.Value);
            if (!userExists) return RedirectToAction("Login", "Account");

            var car = _db.Cars.Include(c => c.CarModel).FirstOrDefault(c => c.CarID == id);
            if (car == null) return NotFound();

            var vm = new BookingVM
            {
                CarID = car.CarID,
                Car = car,
                LocationList = _db.Locations
                                  .Select(l => new SelectListItem { Value = l.LocationID.ToString(), Text = l.Name })
                                  .ToList(),
                AltDriverName = "",
                AltDriverIC = "",
                AltDriverLicenseNo = ""
            };

            return View(vm);
        }

        // POST: BookCar
        [HttpPost]
        public IActionResult BookCar(BookingVM vm)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Validate user exists in Users table
            var userExists = _db.Users.Any(u => u.UserId == userId.Value);
            if (!userExists) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                vm.LocationList = _db.Locations
                                     .Select(l => new SelectListItem { Value = l.LocationID.ToString(), Text = l.Name })
                                     .ToList();
                return View(vm);
            }

            var car = _db.Cars.Find(vm.CarID);
            if (car == null) return NotFound();

            int days = (vm.ReturnDate - vm.PickupDate).Days;
            decimal total = days * car.DailyRate;
            if (vm.NeedDriver) total += days * 2000; // driver fee

            var booking = new Booking
            {
                CarID = vm.CarID,
                CustomerID = userId.Value, // safe now, FK guaranteed
                LocationID = vm.LocationID,
                PickupDate = vm.PickupDate,
                ReturnDate = vm.ReturnDate,
                NeedDriver = vm.NeedDriver,
                AltDriverName = vm.AltDriverName,
                AltDriverIC = vm.AltDriverIC,
                AltDriverLicenseNo = vm.AltDriverLicenseNo,
                TotalAmount = total,
                Status = "PendingPayment"
            };

            _db.Bookings.Add(booking);
            _db.SaveChanges();

            return RedirectToAction("Method", "Payment", new { id = booking.BookingID });
        }
    }
}
