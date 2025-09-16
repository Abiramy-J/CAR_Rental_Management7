using Car_Rental_Management.Data;
using Car_Rental_Management.ViewModels; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Car_Rental_Management.Models;
using System.ComponentModel.DataAnnotations;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ========================
    // GET: /Admin/AddCar
    // ========================
    public IActionResult AddCar()
    {
        var vm = new CarVM
        {
            Car = new Car(),  // empty car object for the form to bind

            // Load dropdown for CarModel from DB
            CarModelList = _context.CarModels.Select(m => new SelectListItem
            {
                Value = m.CarModelID.ToString(),
                Text = m.ModelName
            }).ToList(),

            // Hardcoded status dropdown
            StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Available", Text = "Available" },
                new SelectListItem { Value = "Maintenance", Text = "Maintenance" }
            }
        };

        return View(vm);
    }

    // ========================
    // POST: /Admin/AddCar
    // ========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCar(CarVM vm)
    {
        // 🛠️ Remove them manually before validation
        ModelState.Remove("CarModelList");
        ModelState.Remove("StatusList");

        if (!ModelState.IsValid)
        {
            // Reload dropdowns
            vm.CarModelList = _context.CarModels.Select(m => new SelectListItem
            {
                Value = m.CarModelID.ToString(),
                Text = m.ModelName
            }).ToList();

            vm.StatusList = new List<SelectListItem>
        {
            new SelectListItem { Value = "Available", Text = "Available" },
            new SelectListItem { Value = "Maintenance", Text = "Maintenance" }
        };

            return View(vm);
        }

        // Folder to save images
    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cars");
    if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);

    // Handle uploaded images
    if (vm.ImageFile != null)
    {
        string fileName = Guid.NewGuid() + Path.GetExtension(vm.ImageFile.FileName);
        string filePath = Path.Combine(folder, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
            await vm.ImageFile.CopyToAsync(stream);

        vm.Car.ImageUrl = "/images/cars/" + fileName;
    }

    if (vm.ImageFile2 != null)
    {
        string fileName = Guid.NewGuid() + Path.GetExtension(vm.ImageFile2.FileName);
        string filePath = Path.Combine(folder, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
            await vm.ImageFile2.CopyToAsync(stream);

        vm.Car.ImageUrl2 = "/images/cars/" + fileName;
    }

    // Repeat for ImageFile3 and ImageFile4

    _context.Cars.Add(vm.Car);
    await _context.SaveChangesAsync();

    return RedirectToAction("CarList");
}

    // OPTIONAL: To show list of cars
    //public IActionResult CarList(CarFilterVM filter)
    //{
    //    if (filter == null)
    //        filter = new CarFilterVM();

    //    var cars = _context.Cars.Include(c => c.CarModel).AsQueryable();

    //    // Apply filtering
    //    if (filter.SelectedCarModelID.HasValue)
    //        cars = cars.Where(c => c.CarModelID == filter.SelectedCarModelID.Value);

    //    if (filter.MinRate.HasValue)
    //        cars = cars.Where(c => c.DailyRate >= filter.MinRate.Value);

    //    if (filter.MaxRate.HasValue)
    //        cars = cars.Where(c => c.DailyRate <= filter.MaxRate.Value);

    //    if (!string.IsNullOrEmpty(filter.Status))
    //        cars = cars.Where(c => c.Status == filter.Status);

    //    if (!string.IsNullOrEmpty(filter.Keyword))
    //        cars = cars.Where(c => c.Description.Contains(filter.Keyword));

    //    // Apply sorting
    //    switch (filter.SortOrder)
    //    {
    //        case "model_desc":
    //            cars = cars.OrderByDescending(c => c.CarModel.ModelName);
    //            break;
    //        case "rate_asc":
    //            cars = cars.OrderBy(c => c.DailyRate);
    //            break;
    //        case "rate_desc":
    //            cars = cars.OrderByDescending(c => c.DailyRate);
    //            break;
    //        default: // default sort by Model ascending
    //            cars = cars.OrderBy(c => c.CarModel.ModelName);
    //            break;
    //    }

    //    // Prepare ViewModel data for dropdowns and results
    //    filter.CarList = cars.ToList();

    //    filter.CarModelList = _context.CarModels.Select(m => new SelectListItem
    //    {
    //        Value = m.CarModelID.ToString(),
    //        Text = m.ModelName
    //    }).ToList();

    //    filter.StatusList = new List<SelectListItem>
    //{
    //    new SelectListItem { Value = "Available", Text = "Available" },
    //    new SelectListItem { Value = "Maintenance", Text = "Maintenance" }
    //};

    //    return View(filter);
    //}



    // GET: Admin/CarList
    public async Task<IActionResult> CarList(CarFilterVM filter)
    {
        if (filter == null)
            filter = new CarFilterVM();

        // Start query with Cars including their CarModel
        var carsQuery = _context.Cars.Include(c => c.CarModel).AsQueryable();

        // Apply filtering
        if (filter.SelectedCarModelID.HasValue)
            carsQuery = carsQuery.Where(c => c.CarModelID == filter.SelectedCarModelID.Value);

        if (filter.MinRate.HasValue)
            carsQuery = carsQuery.Where(c => c.DailyRate >= filter.MinRate.Value);

        if (filter.MaxRate.HasValue)
            carsQuery = carsQuery.Where(c => c.DailyRate <= filter.MaxRate.Value);

        if (!string.IsNullOrEmpty(filter.Status))
            carsQuery = carsQuery.Where(c => c.Status == filter.Status);

        if (!string.IsNullOrEmpty(filter.Keyword))
            carsQuery = carsQuery.Where(c => c.Description.Contains(filter.Keyword));

        // Apply sorting
        switch (filter.SortOrder)
        {
            case "model_desc":
                carsQuery = carsQuery.OrderByDescending(c => c.CarModel.ModelName);
                break;
            case "rate_asc":
                carsQuery = carsQuery.OrderBy(c => c.DailyRate);
                break;
            case "rate_desc":
                carsQuery = carsQuery.OrderByDescending(c => c.DailyRate);
                break;
            default: // default sort by Model ascending
                carsQuery = carsQuery.OrderBy(c => c.CarModel.ModelName);
                break;
        }

        // Execute query asynchronously
        filter.CarList = await carsQuery.ToListAsync();

        // Prepare dropdowns for filtering
        filter.CarModelList = await _context.CarModels
            .Select(m => new SelectListItem
            {
                Value = m.CarModelID.ToString(),
                Text = m.ModelName
            })
            .ToListAsync();

        filter.StatusList = new List<SelectListItem>
    {
        new SelectListItem { Value = "Available", Text = "Available" },
        new SelectListItem { Value = "Maintenance", Text = "Maintenance" }
    };

        return View(filter);
    }




    // GET: Admin/EditCar/5
    public IActionResult EditCar(int id)
    {
        var car = _context.Cars
            .Include(c => c.CarModel)
            .FirstOrDefault(c => c.CarID == id);

        if (car == null) return NotFound();

        var vm = new CarVM
        {
            Car = car,
            CarModelList = _context.CarModels.Select(m => new SelectListItem
            {
                Value = m.CarModelID.ToString(),
                Text = m.ModelName
            }).ToList(),
            StatusList = new List<SelectListItem>
        {
            new SelectListItem { Value = "Available", Text = "Available" },
            new SelectListItem { Value = "Maintenance", Text = "Maintenance" }
        }
        };

        return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCar(int id, CarVM vm)
    {
        if (id != vm.Car.CarID) return BadRequest();

        // Remove non-model fields from ModelState for validation
        ModelState.Remove("CarModelList");
        ModelState.Remove("StatusList");

        if (!ModelState.IsValid)
        {
            // Reload dropdowns if validation fails
            vm.CarModelList = _context.CarModels.Select(m => new SelectListItem
            {
                Value = m.CarModelID.ToString(),
                Text = m.ModelName
            }).ToList();

            vm.StatusList = new List<SelectListItem>
        {
            new SelectListItem { Value = "Available", Text = "Available" },
            new SelectListItem { Value = "Maintenance", Text = "Maintenance" }
        };

            return View(vm);
        }

        // Find existing car in DB
        var existingCar = await _context.Cars.FindAsync(id);
        if (existingCar == null) return NotFound();

        // Update basic fields
        existingCar.CarModelID = vm.Car.CarModelID;
        existingCar.Color = vm.Car.Color;
        existingCar.Mileage = vm.Car.Mileage;
        existingCar.DailyRate = vm.Car.DailyRate;
        existingCar.Description = vm.Car.Description;
        existingCar.Status = vm.Car.Status;
        existingCar.VideoUrl = vm.Car.VideoUrl;

        // Folder to save images
        string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cars");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        // Handle uploaded images
        if (vm.ImageFile != null)
        {
            string fileName = Guid.NewGuid() + Path.GetExtension(vm.ImageFile.FileName);
            string filePath = Path.Combine(folder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await vm.ImageFile.CopyToAsync(stream);

            existingCar.ImageUrl = "/images/cars/" + fileName;
        }

        if (vm.ImageFile2 != null)
        {
            string fileName2 = Guid.NewGuid() + Path.GetExtension(vm.ImageFile2.FileName);
            string filePath2 = Path.Combine(folder, fileName2);
            using (var stream = new FileStream(filePath2, FileMode.Create))
                await vm.ImageFile2.CopyToAsync(stream);

            existingCar.ImageUrl2 = "/images/cars/" + fileName2;
        }

        if (vm.ImageFile3 != null)
        {
            string fileName3 = Guid.NewGuid() + Path.GetExtension(vm.ImageFile3.FileName);
            string filePath3 = Path.Combine(folder, fileName3);
            using (var stream = new FileStream(filePath3, FileMode.Create))
                await vm.ImageFile3.CopyToAsync(stream);

            existingCar.ImageUrl3 = "/images/cars/" + fileName3;
        }

        if (vm.ImageFile4 != null)
        {
            string fileName4 = Guid.NewGuid() + Path.GetExtension(vm.ImageFile4.FileName);
            string filePath4 = Path.Combine(folder, fileName4);
            using (var stream = new FileStream(filePath4, FileMode.Create))
                await vm.ImageFile4.CopyToAsync(stream);

            existingCar.ImageUrl4 = "/images/cars/" + fileName4;
        }

        _context.Cars.Update(existingCar);
        await _context.SaveChangesAsync();

        return RedirectToAction("CarList");
    }

    // GET: Admin/DeleteCar/5
    public IActionResult DeleteCar(int id)
    {
        var car = _context.Cars.Include(c => c.CarModel).FirstOrDefault(c => c.CarID == id);
        if (car == null) return NotFound();

        return View(car);
    }
    [HttpPost, ActionName("DeleteCar")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteCarConfirmed(int id)
    {
        var car = _context.Cars.Find(id);
        if (car == null) return NotFound();

        _context.Cars.Remove(car);
        _context.SaveChanges();

        return RedirectToAction("CarList");
    }

    [Required]
    public string Description { get; set; }


    // GET: Admin/Dashboard
    public IActionResult Dashboard()
    {
        // Collect statistics for dashboard
        var totalCars = _context.Cars.Count();
        var availableCars = _context.Cars.Count(c => c.Status == "Available");
        var totalBookings = _context.Bookings.Count();
        var totalCustomers = _context.Users.Count(u => u.Role == "Customer");

        // Pass data to View via ViewBag
        ViewBag.TotalCars = totalCars;
        ViewBag.AvailableCars = availableCars;
        ViewBag.TotalBookings = totalBookings;
        ViewBag.TotalCustomers = totalCustomers;

        // (Optional) Recent Bookings - last 5
        //var recentBookings = _context.Bookings
        //    .OrderByDescending(b => b.BookingDate)
        //    .Take(5)
        //    .ToList();

        //ViewBag.RecentBookings = recentBookings;

        return View();
    }
    // ========================
    // GET: /Admin/User
    // ========================
    public async Task<IActionResult> User()
    {
        var customers = await _context.Users
                                      .Where(u => u.Role == "Customer")
                                      .ToListAsync();

        return View(customers);
    }

    // GET: Admin/CreateUser
    public IActionResult CreateUser()
    {
        ViewBag.Roles = new List<SelectListItem>
    {
        new SelectListItem { Value = "Admin", Text = "Admin" },
        new SelectListItem { Value = "Staff", Text = "Staff" }
    };

        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = new List<SelectListItem>
        {
            new SelectListItem { Value = "Admin", Text = "Admin" },
            new SelectListItem { Value = "Staff", Text = "Staff" }
        };
            return View(model);
        }

        // Check duplicate username
        if (_context.Users.Any(u => u.Username == model.Username))
        {
            ModelState.AddModelError("Username", "Username already exists");
            ViewBag.Roles = new List<SelectListItem>
        {
            new SelectListItem { Value = "Admin", Text = "Admin" },
            new SelectListItem { Value = "Staff", Text = "Staff" }
        };
            return View(model);
        }

        var user = new User
        {
            Username = model.Username.Trim(),
            Password = model.Password,   // ⚠️ later hash this
            Role = model.Role
            // If you have a Name column in User entity, save it too:
            // Name = model.Name
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "User created successfully!";
        return RedirectToAction("User"); // goes to list of users
    }


    [Route("Booking")]
    public IActionResult Booking()
    {
        // Optional: check if admin
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin") return RedirectToAction("Login", "Account");

        var bookings = _context.Bookings
        .Include(b => b.Car).ThenInclude(c => c.CarModel)
        .Include(b => b.Customer)
        .Include(b => b.Location)
        .Include(b => b.DriverBookings) // include driver bookings
            .ThenInclude(db => db.Driver) // get driver info
        .ToList();


        return View(bookings);
    }

}
