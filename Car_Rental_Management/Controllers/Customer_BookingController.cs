using Car_Rental_Management.Data;
using Car_Rental_Management.Models;
using Car_Rental_Management.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Register", "Account", new { returnUrl = $"/CustomerBooking/BookCar/{id}" });

            if (!_db.Users.Any(u => u.UserId == userId.Value))
                return RedirectToAction("Login", "Account");

            var car = _db.Cars.Include(c => c.CarModel).FirstOrDefault(c => c.CarID == id);
            if (car == null) return NotFound();

            var vm = new BookingVM
            {
                CarID = car.CarID,
                Car = car,
                LocationList = GetLocations(),
                AltDriverName = "",
                AltDriverIC = "",
                AltDriverLicenseNo = "",
                AvailableDrivers = new List<SelectListItem>()
            };

            // Optional: send existing booked dates to view for info
            var bookedDates = _db.Bookings
                .Where(b => b.CarID == id)
                .Select(b => new { b.PickupDate, b.ReturnDate })
                .ToList();
            ViewBag.BookedDates = bookedDates;

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookCar(BookingVM vm)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!_db.Users.Any(u => u.UserId == userId.Value))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                vm.LocationList = GetLocations();
                return View(vm);
            }

            var car = _db.Cars.Find(vm.CarID);
            if (car == null) return NotFound();

            // Prevent double booking for overlapping dates
            bool isCarBooked = _db.Bookings.Any(b =>
                b.CarID == vm.CarID &&
                b.ReturnDate >= vm.PickupDate &&
                b.PickupDate <= vm.ReturnDate
            );
            if (isCarBooked)
            {
                TempData["Error"] = "This car is already booked for the selected dates!";
                return RedirectToAction("BrowseCars", "Customer");
            }

            int days = Math.Max(1, (vm.ReturnDate - vm.PickupDate).Days);
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

            // Update Car Status to "Booked"
            car.Status = "Booked";  // இங்கு Car Status-ஐ மாற்றுகிறோம்
            _db.Cars.Update(car);   // Car table-இல் update செய்யப்படுகிறது

            _db.SaveChanges();  // Booking மற்றும் Car Status ஒரே save-இல் commit

            // Assign driver if selected
            if (vm.NeedDriver && vm.SelectedDriverId.HasValue)
            {
                var driver = _db.Drivers.Find(vm.SelectedDriverId.Value);
                if (driver == null)
                {
                    ModelState.AddModelError("", "Selected driver not found.");
                    vm.LocationList = GetLocations();
                    return View(vm);
                }

                bool isDriverAvailable = !_db.DriverBookings.Any(dbk =>
                    dbk.DriverId == driver.DriverId &&
                    vm.PickupDate < dbk.ReturnDateTime &&
                    vm.ReturnDate > dbk.PickupDateTime
                );

                if (!isDriverAvailable)
                {
                    TempData["Error"] = "Selected driver is already booked for this time slot.";
                    return RedirectToAction("BrowseCars", "Customer");
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

            // Redirect to Payment page
            return RedirectToAction("Method", "Payment", new { id = booking.BookingID });
        }

        // API: Get available drivers for a date range
        [HttpGet]
        public IActionResult AvailableDrivers(DateTime pickup, DateTime returnDate)
        {
            var drivers = GetAvailableDrivers(pickup, returnDate)
                .Select(d => new { d.DriverId, d.FullName, d.PhoneNo, d.LicenseNo })
                .ToList();
            return Json(drivers);
        }

        // Helper: Filter available drivers
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
