using Car_Rental_Management.Data;
using Car_Rental_Management.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Car_Rental_Management.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }
        //public async Task<IActionResult> BrowseCars(
        //int? SelectedCarModelID, decimal? MinRate, decimal? MaxRate,
        //string? Status, string? Keyword, DateTime? FromDate, DateTime? ToDate)
        //{
        //    var carsQuery = _context.Cars
        //        .Include(c => c.CarModel)
        //        .Include(c => c.Bookings)
        //        .AsQueryable();

        //    if (SelectedCarModelID.HasValue)
        //        carsQuery = carsQuery.Where(c => c.CarModelID == SelectedCarModelID.Value);

        //    if (MinRate.HasValue)
        //        carsQuery = carsQuery.Where(c => c.DailyRate >= MinRate.Value);

        //    if (MaxRate.HasValue)
        //        carsQuery = carsQuery.Where(c => c.DailyRate <= MaxRate.Value);

        //    // Only cars with status Available
        //    carsQuery = carsQuery.Where(c => c.Status == "Available");

        //    if (!string.IsNullOrWhiteSpace(Keyword))
        //        carsQuery = carsQuery.Where(c => c.Description.Contains(Keyword));

        //    // ❗️ Hide cars that have bookings overlapping given date range (or today if none given)
        //    var start = FromDate ?? DateTime.Today;
        //    var end = ToDate ?? DateTime.Today;

        //    carsQuery = carsQuery.Where(c =>
        //        !c.Bookings.Any(b =>
        //            b.StartDate <= end && b.EndDate >= start));

        //    var vm = new CustomerBrowseCarVM
        //    {
        //        Cars = await carsQuery.ToListAsync(),
        //        CarModelList = await _context.CarModels
        //            .Select(cm => new SelectListItem
        //            {
        //                Value = cm.CarModelID.ToString(),
        //                Text = cm.ModelName
        //            }).ToListAsync(),
        //        StatusList = new List<SelectListItem>
        //{
        //    new() { Value = "Available", Text = "Available" },
        //    new() { Value = "Booked", Text = "Booked" }
        //},
        //        SelectedCarModelID = SelectedCarModelID,
        //        MinRate = MinRate,
        //        MaxRate = MaxRate,
        //        Status = Status,
        //        Keyword = Keyword
        //    };

        //    return View(vm);
        //}

        public async Task<IActionResult> BrowseCars(int? SelectedCarModelID, decimal? MinRate, decimal? MaxRate, string? Status, string? Keyword)
        {
            var carsQuery = _context.Cars.Include(c => c.CarModel).AsQueryable();

            if (SelectedCarModelID.HasValue)
                carsQuery = carsQuery.Where(c => c.CarModelID == SelectedCarModelID.Value);

            if (MinRate.HasValue)
                carsQuery = carsQuery.Where(c => c.DailyRate >= MinRate.Value);

            if (MaxRate.HasValue)
                carsQuery = carsQuery.Where(c => c.DailyRate <= MaxRate.Value);

            // ✅ Only show "Available" cars by default
            if (!string.IsNullOrEmpty(Status))
            {
                carsQuery = carsQuery.Where(c => c.Status == Status);
            }
            else
            {
                // Default filter: Only available cars
                carsQuery = carsQuery.Where(c => c.Status == "Available");
            }

            if (!string.IsNullOrEmpty(Keyword))
                carsQuery = carsQuery.Where(c => c.Description.Contains(Keyword));

            var vm = new CustomerBrowseCarVM
            {
                Cars = await carsQuery.ToListAsync(),
                CarModelList = await _context.CarModels
                    .Select(cm => new SelectListItem { Value = cm.CarModelID.ToString(), Text = cm.ModelName })
                    .ToListAsync(),
                StatusList = new List<SelectListItem>
        {
            new SelectListItem { Value = "Available", Text = "Available" },
            new SelectListItem { Value = "Booked", Text = "Booked" }
        },
                SelectedCarModelID = SelectedCarModelID,
                MinRate = MinRate,
                MaxRate = MaxRate,
                Status = Status,
                Keyword = Keyword
            };

            return View(vm);
        }


        // GET: MyBookings
        public IActionResult MyBookings()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var bookings = _context.Bookings
            .Include(b => b.Car).ThenInclude(c => c.CarModel)
            .Include(b => b.Location)
            .Include(b => b.Driver)
            .Where(b => b.CustomerID == userId.Value)
            .ToList();

            return View(bookings);
        }

        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            return View(); // Your Customer dashboard view
            
        }
        

    }
}
