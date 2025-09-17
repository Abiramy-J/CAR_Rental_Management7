using Car_Rental_Management.Data;
using Car_Rental_Management.Models;
using Car_Rental_Management.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Http;


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
            if (userId == null) return RedirectToAction("Register", "Account", new { returnUrl = $"/CustomerBooking/BookCar/{id}" });

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
                                  .Select(l => new SelectListItem { Value = l.LocationID.ToString(), Text = l.Address })
                                  .ToList(),
                AltDriverName = "",
                AltDriverIC = "",
                AltDriverLicenseNo = "",
                AvailableDrivers = new List<SelectListItem>() // initially empty, will be loaded via JS if needed

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
                vm.LocationList = GetLocations();
                return View(vm);
            }

            var car = _db.Cars.Find(vm.CarID);
            if (car == null) return NotFound();

            int days = Math.Max(1, (vm.ReturnDate - vm.PickupDate).Days); // at least 1 day
            decimal total = days * car.DailyRate;
            if (vm.NeedDriver) total += days * 2000;
            var booking = new Booking
            {
                CarID = vm.CarID,
                CustomerID = userId.Value,
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

            // ✅ Mark the car as Booked
            car.Status = "Booked";
            _db.Cars.Update(car);

            _db.SaveChanges(); // Save both booking and car status


            // Assign company driver if selected
            if (vm.NeedDriver && vm.SelectedDriverId.HasValue)
            {
                var driver = _db.Drivers.Find(vm.SelectedDriverId.Value);
                if (driver == null)
                {
                    ModelState.AddModelError("", "Selected driver not found.");
                    vm.LocationList = GetLocations();
                    return View(vm);
                }

                bool isDriverAvailable = !_db.DriverBookings
                    .Any(dbk => dbk.DriverId == driver.DriverId &&
                                (vm.PickupDate < dbk.ReturnDateTime && vm.ReturnDate > dbk.PickupDateTime));

                if (!isDriverAvailable)
                {
                    ModelState.AddModelError("", "Selected driver is already booked for this time slot.");
                    vm.LocationList = GetLocations();
                    return View(vm);
                }

                var driverBooking = new DriverBooking
                {
                    DriverId = driver.DriverId,
                    CustomerId = userId.Value,
                    CarId = vm.CarID,
                    BookingId = booking.BookingID,
                    PickupDateTime = vm.PickupDate,
                    ReturnDateTime = vm.ReturnDate
                };

                _db.DriverBookings.Add(driverBooking);
                _db.SaveChanges();
            }

            return RedirectToAction("Method", "Payment", new { id = booking.BookingID });
        }

        [HttpGet]
        public IActionResult AvailableDrivers(DateTime pickup, DateTime returnDate)
        {
            if (pickup >= returnDate)
                return BadRequest("Return date must be after pickup date.");

            var drivers = GetAvailableDrivers(pickup, returnDate);

            if (drivers == null || !drivers.Any())
                return Json(new List<object>()); // empty list if no drivers

            var result = drivers.Select(d => new
            {
                d.DriverId,
                d.FullName,
                d.PhoneNo,
                d.LicenseNo
            }).ToList();

            return Json(result);
        }



        // Helper: Get available drivers
        private List<Driver> GetAvailableDrivers(DateTime pickup, DateTime returnDate)
        {
            var allDrivers = _db.Drivers.ToList();
            var overlappingBookings = _db.DriverBookings
                .Where(db => pickup < db.ReturnDateTime && returnDate > db.PickupDateTime)
                .Select(db => db.DriverId)
                .ToList();

            return allDrivers.Where(d => !overlappingBookings.Contains(d.DriverId)).ToList();
        }

        // Helper: Get locations
        private List<SelectListItem> GetLocations() =>
            _db.Locations.Select(l => new SelectListItem { Value = l.LocationID.ToString(), Text = l.Address }).ToList();
    }
}
