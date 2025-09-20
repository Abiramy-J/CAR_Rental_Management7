using Car_Rental_Management.Data;
using Car_Rental_Management.Models;
using Car_Rental_Management.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Rental_Management.Controllers
{
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DriverController(ApplicationDbContext db)
        {
            _db = db;
        }

        // List all drivers
        public IActionResult Index()
        {
            var drivers = _db.Drivers.ToList();
            return View(drivers);
        }

        // GET: Driver/Create
        public IActionResult Create()
        {
            return View(new DriverCreateVM());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DriverCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username already exists
            var existingUser = await _db.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.Trim().ToLower());
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(model);
            }

            // Create new User
            var user = new User
            {
                Username = model.Username.Trim(),
                Password = model.Password.Trim(), 
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Role = "Driver",
                ProfileImageUrl = "/images/default-profile.png"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(); 

            // Create new Driver
            var driver = new Driver
            {
                FullName = model.FullName.Trim(),
                PhoneNo = model.PhoneNumber.Trim(),
                NIC = model.NIC.Trim(),
                LicenseNo = model.LicenseNo.Trim(),
                UserId = user.UserId 
            };

            _db.Drivers.Add(driver);
            await _db.SaveChangesAsync(); 

            TempData["Success"] = "Driver added successfully!";
            return RedirectToAction(nameof(Index));
        }
        // GET: Driver/Edit/5
        public IActionResult Edit(int id)
        {
            var driver = _db.Drivers.Include(d => d.User).FirstOrDefault(d => d.DriverId == id);
            if (driver == null) return NotFound();

            var vm = new DriverEditVM
            {
                DriverId = driver.DriverId,
                FullName = driver.FullName,
                PhoneNo = driver.PhoneNo,
                NIC = driver.NIC,
                LicenseNo = driver.LicenseNo,
                Username = driver.User.Username,
                Email = driver.User.Email
            };
            return View(vm);
        }

        // POST: Driver/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DriverEditVM model)
        {
            if (id != model.DriverId || !ModelState.IsValid) return View(model);

            var driver = await _db.Drivers.Include(d => d.User).FirstOrDefaultAsync(d => d.DriverId == id);
            if (driver == null) return NotFound();

            // Update Driver details
            driver.FullName = model.FullName.Trim();
            driver.PhoneNo = model.PhoneNo.Trim();
            driver.NIC = model.NIC.Trim();
            driver.LicenseNo = model.LicenseNo.Trim();

            // Update User details
            var user = driver.User;
            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNo.Trim();
            user.Email = model.Email.Trim();
            user.Username = model.Username.Trim(); // Note: Check for username uniqueness if needed

            _db.Drivers.Update(driver);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Driver updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Driver/Delete/5
        public IActionResult Delete(int id)
        {
            var driver = _db.Drivers.Include(d => d.User).FirstOrDefault(d => d.DriverId == id);
            if (driver == null) return NotFound();

            return View(driver);
        }

        // POST: Driver/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _db.Drivers.Include(d => d.User).FirstOrDefaultAsync(d => d.DriverId == id);
            if (driver == null) return NotFound();

            // Delete the associated User
            if (driver.User != null)
            {
                _db.Users.Remove(driver.User);
            }

            _db.Drivers.Remove(driver);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Driver deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Driver/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Verify user is a driver
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
            if (user == null || user.Role != "Driver")
                return Unauthorized("Access restricted to drivers.");

            // Get driver record
            var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.UserId == userId.Value);
            if (driver == null)
                return NotFound("Driver profile not found.");

            // Fetch bookings assigned to this driver
            var bookings = await _db.DriverBookings
                .Include(db => db.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(db => db.Booking)
                    .ThenInclude(b => b.Car)
                        .ThenInclude(c => c.CarModel)
                .Where(db => db.DriverId == driver.DriverId)
                .Select(db => new
                {
                    BookingId = db.Booking.BookingID,
                    CustomerName = db.Booking.Customer.FullName,
                    PickupDate = db.Booking.PickupDate,
                    Duration = (db.Booking.ReturnDate - db.Booking.PickupDate).Days + 1, // Include end date
                    Status = db.Booking.Status
                })
                .OrderByDescending(db => db.PickupDate)
                .ToListAsync();

            return View(bookings);
        }

    }
}

