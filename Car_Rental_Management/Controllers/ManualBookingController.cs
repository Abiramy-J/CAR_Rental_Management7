using Car_Rental_Management.Data;
using Car_Rental_Management.Models;
using Car_Rental_Management.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Car_Rental_Management.Controllers
{
    public class ManualBookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManualBookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------- GET: Select Car ----------------
        public async Task<IActionResult> SelectCar()
        {
            var cars = await _context.Cars
                .Include(c => c.CarModel)
                .ToListAsync();
            return View(cars);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int carId, int? customerId = null)
        {
            var car = await _context.Cars.Include(c => c.CarModel).FirstOrDefaultAsync(c => c.CarID == carId);
            if (car == null) return NotFound();

            var pickup = DateTime.Now;
            var ret = pickup.AddDays(1);

            var vm = new ManualBookingVM
            {
                CarId = car.CarID,
                CarModelName = car.CarModel?.ModelName ?? "",
                CarDailyRate = car.DailyRate,
                DriverDailyRate = 50m, // default driver/day (adjust or fetch per-driver)
                PickupDate = pickup,
                ReturnDate = ret,
                Total = car.DailyRate,
                PaymentDate = DateTime.Now,
                PaymentMethod = "Manual",
                IsPaid = true,
                CustomerId = customerId,
                LocationList = await GetLocationsAsync(),
                AvailableDrivers = await GetAvailableDriversAsync(pickup, ret)
            };

            if (customerId.HasValue)
            {
                var customer = await _context.Users.FindAsync(customerId.Value);
                if (customer != null)
                {
                    vm.FullName = customer.FullName ?? vm.FullName;
                    vm.Username = customer.Username ?? vm.Username;
                    vm.Email = customer.Email;
                    vm.PhoneNumber = customer.PhoneNumber;
                }
            }

            return View(vm);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ManualBookingVM vm)
        {
            // server-side validation: driver selection vs alt-driver fields
            if (vm.NeedDriver && !vm.SelectedDriverId.HasValue)
                ModelState.AddModelError(nameof(vm.SelectedDriverId), "Please select a driver when 'Need Driver' is Yes.");

            if (!vm.NeedDriver)
            {
                //optional: require alt-driver fields only if you want
                if (string.IsNullOrWhiteSpace(vm.AltDriverName)) ModelState.AddModelError(nameof(vm.AltDriverName), "Please enter alternative driver name.");
            }

            if (!ModelState.IsValid)
            {
                vm.LocationList = await GetLocationsAsync();
                vm.AvailableDrivers = await GetAvailableDriversAsync(vm.PickupDate, vm.ReturnDate);
                TempData["Errors"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(vm);
            }

            // Recalculate server-side total (authoritative)
            var car = await _context.Cars.FindAsync(vm.CarId);
            if (car == null) return NotFound();

            var days = Math.Ceiling((vm.ReturnDate - vm.PickupDate).TotalDays);
            if (days < 1) days = 1;
            decimal total = car.DailyRate * (decimal)days;
            var driverRate = vm.DriverDailyRate;
            if (vm.NeedDriver) total += driverRate * (decimal)days;
            vm.Total = total;

            // Begin a transaction to ensure atomicity
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Create or get customer (don't SaveChanges yet; EF can handle it)
                User customer;
                if (vm.CustomerId.HasValue)
                {
                    customer = await _context.Users.FindAsync(vm.CustomerId.Value)
                               ?? throw new Exception("Customer not found.");
                }
                else
                {
                    customer = new User
                    {
                        FullName = vm.FullName,
                        Username = vm.Username,
                        Password = vm.Password,
                        Email = vm.Email,
                        PhoneNumber = vm.PhoneNumber,
                        Role = "Customer"
                    };
                    _context.Users.Add(customer);
                }

                // 2) Create booking and link to customer entity (works whether new or existing)
                var booking = new Booking
                {
                    CarID = vm.CarId,
                    Customer = customer,                // set navigation property so EF will set FK
                    LocationID = vm.LocationId,
                    PickupDate = vm.PickupDate,
                    ReturnDate = vm.ReturnDate,
                    NeedDriver = vm.NeedDriver,
                    DriverId = vm.NeedDriver ? vm.SelectedDriverId : null,
                    AltDriverName = vm.NeedDriver ? null : vm.AltDriverName,
                    AltDriverIC = vm.NeedDriver ? null : vm.AltDriverIC,
                    AltDriverLicenseNo = vm.NeedDriver ? null : vm.AltDriverLicenseNo,
                    TotalAmount = vm.Total,
                    PaymentDate = vm.PaymentDate ?? DateTime.Now,
                    PaymentMethod = vm.PaymentMethod ?? "Manual",
                    Status = vm.IsPaid ? "Paid" : "Pending"
                };
                _context.Bookings.Add(booking);

                // Save so booking.CustomerId and booking.BookingID are assigned (and new user id, if any)
                await _context.SaveChangesAsync();

                // 3) If driver needed, create DriverBooking and set driver status
                if (vm.NeedDriver && vm.SelectedDriverId.HasValue)
                {
                    var driverBooking = new DriverBooking
                    {
                        DriverId = vm.SelectedDriverId.Value,
                        BookingId = booking.BookingID,
                        CustomerId = booking.CustomerID,
                        CarId = vm.CarId,
                        PickupDateTime = vm.PickupDate,
                        ReturnDateTime = vm.ReturnDate
                    };
                    _context.DriverBookings.Add(driverBooking);

                    var driver = await _context.Drivers.FindAsync(vm.SelectedDriverId.Value);
                    if (driver != null) driver.Status = "Booked";
                }

                // 4) Update car status
                var carToUpdate = await _context.Cars.FindAsync(vm.CarId);
                if (carToUpdate != null) carToUpdate.Status = "Booked";

                // 5) Save all remaining changes & commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Booking created successfully.";
                return RedirectToAction("BookingList");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // For debugging: pass message to view
                TempData["Errors"] = "Failed to create booking: " + ex.Message;
                vm.LocationList = await GetLocationsAsync();
                vm.AvailableDrivers = await GetAvailableDriversAsync(vm.PickupDate, vm.ReturnDate);
                return View(vm);
            }
        }

        // Helper: locations
        private async Task<List<SelectListItem>> GetLocationsAsync()
        {
            return await _context.Locations
                .Select(l => new SelectListItem { Value = l.LocationID.ToString(), Text = l.Address })
                .ToListAsync();
        }

        // Helper: available drivers (no overlapping bookings)
        private async Task<List<SelectListItem>> GetAvailableDriversAsync(DateTime pickup, DateTime returnDate)
        {
            var bookedDriverIds = await _context.DriverBookings
                .Where(db => db.PickupDateTime <= returnDate && db.ReturnDateTime >= pickup)
                .Select(db => db.DriverId)
                .ToListAsync();

            return await _context.Drivers
                .Where(d => !bookedDriverIds.Contains(d.DriverId))
                .Select(d => new SelectListItem { Value = d.DriverId.ToString(), Text = d.FullName })
                .ToListAsync();
        }

        private async Task UpdateCarStatusAsync(int carId, string status)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car != null)
            {
                car.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        private async Task UpdateDriverStatusAsync(int driverId, string status)
        {
            var driver = await _context.Drivers.FindAsync(driverId);
            if (driver != null)
            {
                driver.Status = status;
                await _context.SaveChangesAsync();
            }
        }
        // ---------------- GET: Booking List ----------------
        public async Task<IActionResult> BookingList()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .Include(b => b.DriverBookings).ThenInclude(db => db.Driver)
                .Include(b => b.Location)
                .ToListAsync();

            return View(bookings);
        }
        // ---------------- GET: Edit ----------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Car).ThenInclude(c => c.CarModel)
                .Include(b => b.Customer)
                .Include(b => b.DriverBookings).ThenInclude(db => db.Driver)
                .Include(b => b.Location)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();
            var availableDrivers = await GetAvailableDriversAsync(booking.PickupDate, booking.ReturnDate);
            var vm = new ManualBookingVM
            {
                BookingID = booking.BookingID,
                CarId = booking.CarID,
                CustomerId = booking.CustomerID,
                CarModelName = booking.Car.CarModel?.ModelName ?? "",
                CarDailyRate = booking.Car.DailyRate,
                DriverDailyRate = 500m, // replace with config or db value

                // Customer
                FullName = booking.Customer.FullName,
                Username = booking.Customer.Username,
                Password = booking.Customer.Password,
                Email = booking.Customer.Email,
                PhoneNumber = booking.Customer.PhoneNumber,
                LocationId = booking.LocationID,

                // Booking
                PickupDate = booking.PickupDate,
                ReturnDate = booking.ReturnDate,
                NeedDriver = booking.NeedDriver,
                SelectedDriverId = booking.DriverBookings.FirstOrDefault()?.DriverId,
                AltDriverName = booking.AltDriverName,
                AltDriverIC = booking.AltDriverIC,
                AltDriverLicenseNo = booking.AltDriverLicenseNo,
                Total = booking.TotalAmount,

                // Payment
                PaymentMethod = booking.PaymentMethod,
                PaymentDate = booking.PaymentDate,
                IsPaid = booking.Status == "Paid",

                // Dropdowns
                LocationList = await GetLocationsAsync(),
                AvailableDrivers = availableDrivers
            .Select(d => new SelectListItem
            {
                Value = d.Value,
                Text = d.Text,
                Selected = d.Value == booking.DriverBookings.FirstOrDefault()?.DriverId.ToString()
            })
            .ToList()
            };

            return View(vm);
        }
        // ---------------- POST: Edit ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ManualBookingVM vm)
        {
            if (id != vm.BookingID) return BadRequest();

            if (!ModelState.IsValid)
            {
                vm.LocationList = await GetLocationsAsync();
                vm.AvailableDrivers = await GetAvailableDriversAsync(vm.PickupDate, vm.ReturnDate);
                return View(vm);
            }

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.DriverBookings)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();

            // --- Recalculate total ---
            var car = await _context.Cars.FindAsync(vm.CarId);
            if (car == null) return NotFound();

            var days = Math.Ceiling((vm.ReturnDate - vm.PickupDate).TotalDays);
            if (days < 1) days = 1;

            decimal total = car.DailyRate * (decimal)days;
            if (vm.NeedDriver) total += vm.DriverDailyRate * (decimal)days;

            // --- Update Customer ---
            booking.Customer.FullName = vm.FullName;
            booking.Customer.Username = vm.Username;
            booking.Customer.Password = vm.Password; // ⚠️ hash in production
            booking.Customer.Email = vm.Email;
            booking.Customer.PhoneNumber = vm.PhoneNumber;

            // --- Update Booking ---
            booking.PickupDate = vm.PickupDate;
            booking.ReturnDate = vm.ReturnDate;
            booking.LocationID = vm.LocationId;
            booking.NeedDriver = vm.NeedDriver;
            booking.TotalAmount = total;
            booking.PaymentMethod = vm.PaymentMethod;
            booking.PaymentDate = vm.PaymentDate ?? DateTime.Now;
            booking.Status = vm.IsPaid ? "Paid" : "Pending";

            // --- Release old drivers ---
            foreach (var db in booking.DriverBookings)
            {
                var oldDriver = await _context.Drivers.FindAsync(db.DriverId);
                if (oldDriver != null)
                    oldDriver.Status = "Available"; // mark old driver available
            }

            // --- Clear old assignments ---
            booking.DriverBookings.Clear();

            // --- Assign new driver if needed ---
            if (vm.NeedDriver && vm.SelectedDriverId.HasValue)
            {
                booking.DriverBookings.Add(new DriverBooking
                {
                    DriverId = vm.SelectedDriverId.Value,
                    BookingId = booking.BookingID,
                    CustomerId = booking.CustomerID,
                    CarId = vm.CarId,
                    PickupDateTime = vm.PickupDate,
                    ReturnDateTime = vm.ReturnDate
                });

                var newDriver = await _context.Drivers.FindAsync(vm.SelectedDriverId.Value);
                if (newDriver != null) newDriver.Status = "Booked"; // mark new driver booked
            }
            else
            {
                // --- Alt driver info ---
                booking.AltDriverName = vm.AltDriverName;
                booking.AltDriverIC = vm.AltDriverIC;
                booking.AltDriverLicenseNo = vm.AltDriverLicenseNo;
            }

            try
            {
                await _context.SaveChangesAsync(); // save everything
                TempData["SuccessMessage"] = "Booking updated successfully.";
                return RedirectToAction("BookingList");
            }
            catch (Exception ex)
            {
                TempData["Errors"] = $"Failed to update booking: {ex.Message}";
                vm.LocationList = await GetLocationsAsync();
                vm.AvailableDrivers = await GetAvailableDriversAsync(vm.PickupDate, vm.ReturnDate);
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.DriverBookings)
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();

            booking.Status = "Cancelled";

            // Release car
            if (booking.Car != null) booking.Car.Status = "Available";

            // Release drivers
            foreach (var db in booking.DriverBookings)
            {
                var driver = await _context.Drivers.FindAsync(db.DriverId);
                if (driver != null) driver.Status = "Available";
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["Errors"] = "Failed to cancel booking: " + ex.Message;
            }

            return RedirectToAction("BookingList");
        }

    }
}
