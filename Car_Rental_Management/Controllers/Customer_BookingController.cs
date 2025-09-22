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
using System.Threading.Tasks;

namespace Car_Rental_Management.Controllers
{
    [Route("Customer/[action]/{id?}")]
    public class CustomerBookingController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CustomerBookingController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: BookCar
        public async Task<IActionResult> BookCar(int id, DateTime? pickupDate, DateTime? returnDate)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Register", "Account", new { returnUrl = $"/CustomerBooking/BookCar/{id}" });

            var userExists = await _db.Users.AnyAsync(u => u.UserId == userId.Value);
            if (!userExists)
                return RedirectToAction("Login", "Account");

            var car = await _db.Cars.Include(c => c.CarModel).FirstOrDefaultAsync(c => c.CarID == id);
            if (car == null) return NotFound();

            // Use provided dates or default to today and tomorrow
            var pickup = pickupDate ?? DateTime.Now;
            var ret = returnDate ?? DateTime.Now.AddDays(1);

            // Get available drivers for the selected dates
            var availableDrivers = await GetAvailableDriversAsync(pickup, ret);

            var vm = new BookingVM
            {
                CarID = car.CarID,
                Car = car,
                PickupDate = pickup,
                ReturnDate = ret,
                LocationList = await GetLocationsAsync(),
                AltDriverName = "",
                AltDriverIC = "",
                AltDriverLicenseNo = "",
                AvailableDrivers = availableDrivers.Select(d => new SelectListItem
                {
                    Value = d.DriverId.ToString(),
                    Text = d.FullName
                }).ToList()
            };

            // Existing booked dates
            var bookedDates = await _db.Bookings
                .Where(b => b.CarID == id)
                .Select(b => new { b.PickupDate, b.ReturnDate })
                .ToListAsync();
            ViewBag.BookedDates = bookedDates;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookCar(BookingVM vm)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var userExists = await _db.Users.AnyAsync(u => u.UserId == userId.Value);
            if (!userExists) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                vm.LocationList = await GetLocationsAsync();
                vm.AvailableDrivers = (await GetAvailableDriversAsync(vm.PickupDate, vm.ReturnDate))
                    .Select(d => new SelectListItem { Value = d.DriverId.ToString(), Text = d.FullName })
                    .ToList();
                return View(vm);
            }

            var car = await _db.Cars.FindAsync(vm.CarID);
            if (car == null) return NotFound();

            // Prevent overlapping car bookings
            bool isCarBooked = await _db.Bookings.AnyAsync(b =>
                b.CarID == vm.CarID &&
                b.ReturnDate > vm.PickupDate &&
                b.PickupDate < vm.ReturnDate
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
                Status = "PendingPayment",
                PaymentMethod = null,
                RefundIssued = false
            };

            await _db.Bookings.AddAsync(booking);
            car.Status = "Booked";
            _db.Cars.Update(car);
            await _db.SaveChangesAsync();

            // ---- Robust driver validation ----
            if (vm.NeedDriver && vm.SelectedDriverId.HasValue)
            {
                var driver = await _db.Drivers.FindAsync(vm.SelectedDriverId.Value);
                if (driver == null)
                {
                    ModelState.AddModelError("", "Selected driver not found.");
                }
                else
                {
                    bool isDriverAvailable = !await _db.DriverBookings.AnyAsync(dbk =>
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

                    await _db.DriverBookings.AddAsync(driverBooking);
                    await _db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Method", "Payment", new { id = booking.BookingID });
        }

        [HttpGet]
        public async Task<IActionResult> AvailableDrivers(DateTime pickup, DateTime returnDate)
        {
            if (pickup >= returnDate)
                return Json(new List<object>()); // Return empty if invalid dates

            // Get all drivers
            var allDrivers = await _db.Drivers.ToListAsync();

            // Find driver IDs that have overlapping bookings
            var overlappingDriverIds = await _db.DriverBookings
                .Where(db => db.PickupDateTime < returnDate && db.ReturnDateTime > pickup)
                .Select(db => db.DriverId)
                .Distinct()
                .ToListAsync();

            // Filter out booked drivers
            var availableDrivers = allDrivers
                .Where(d => !overlappingDriverIds.Contains(d.DriverId))
                .Select(d => new
                {
                    driverId = d.DriverId,
                    fullName = d.FullName,
                    phoneNo = d.PhoneNo,
                    licenseNo = d.LicenseNo
                })
                .ToList();

            return Json(availableDrivers);
        }


        // Helper: Filter available drivers
        //private List<Driver> GetAvailableDrivers(DateTime pickup, DateTime returnDate)
        //{
        //    var allDrivers = _db.Drivers.ToList();
        //    var overlappingBookings = _db.DriverBookings
        //        .Where(db => pickup < db.ReturnDateTime && returnDate > db.PickupDateTime)
        //        .Select(db => db.DriverId)
        //        .ToList();

        //    return allDrivers.Where(d => !overlappingBookings.Contains(d.DriverId)).ToList();
        //}

        // Add this async method to the controller
        private async Task<List<Driver>> GetAvailableDriversAsync(DateTime pickup, DateTime returnDate)
        {
            var allDrivers = await _db.Drivers.ToListAsync();
            var overlappingBookings = await _db.DriverBookings
                .Where(db => pickup < db.ReturnDateTime && returnDate > db.PickupDateTime)
                .Select(db => db.DriverId)
                .ToListAsync();

            return allDrivers.Where(d => !overlappingBookings.Contains(d.DriverId)).ToList();
        }

        // Cancel Booking with refund logic
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var booking = await _db.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.CustomerID == userId.Value);

            if (booking == null) return NotFound();

            if (booking.Status == "Cancelled")
            {
                TempData["Error"] = "Booking already cancelled.";
                return RedirectToAction("MyBookings");
            }

            //// Allow cancel only within 24 hours of pickup
            //if ((booking.PickupDate - DateTime.Now).TotalHours > 24)
            //{
            //    TempData["Error"] = "Cannot cancel booking after 24 hours of pickup.";
            //    return RedirectToAction("MyBookings");
            //}

            booking.Status = "Cancelled";

            // Refund Logic
            if (booking.PaymentMethod == "Card")
            {
                booking.RefundIssued = true;
                booking.RefundAmount = booking.TotalAmount;  // Full refund
            }
            else
            {
                booking.RefundIssued = false;
                booking.RefundAmount = 0; // Cash – refund not auto issued
            }

            _db.Bookings.Update(booking);

            if (booking.Car != null)
            {
                booking.Car.Status = "Available";
                _db.Cars.Update(booking.Car);
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Booking cancelled successfully.";
            return RedirectToAction("MyBookings");
        }



        // Helper: Get Locations
        private async Task<List<SelectListItem>> GetLocationsAsync()
        {
            return await _db.Locations.Select(l => new SelectListItem
            {
                Value = l.LocationID.ToString(),
                Text = l.Address
            }).ToListAsync();
        }

        // Customer MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var bookings = await _db.Bookings
                .Include(b => b.Car).ThenInclude(c => c.CarModel)
                .Include(b => b.Location)
                .Where(b => b.CustomerID == userId.Value)
                .OrderByDescending(b => b.PickupDate)
                .ToListAsync();

            // Specify the correct view path
            return View("~/Views/Customer/MyBookings.cshtml", bookings);
        }
    }
}
