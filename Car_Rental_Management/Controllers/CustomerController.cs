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

        public IActionResult BrowseCars(int? SelectedCarModelID, decimal? MinRate, decimal? MaxRate, string? Status, string? Keyword)
        {
            // Base query
            var carsQuery = _context.Cars.Include(c => c.CarModel).AsQueryable();

            // Filters
            if (SelectedCarModelID.HasValue)
                carsQuery = carsQuery.Where(c => c.CarModelID == SelectedCarModelID.Value);

            if (MinRate.HasValue)
                carsQuery = carsQuery.Where(c => c.DailyRate >= MinRate.Value);

            if (MaxRate.HasValue)
                carsQuery = carsQuery.Where(c => c.DailyRate <= MaxRate.Value);

            if (!string.IsNullOrEmpty(Status))
                carsQuery = carsQuery.Where(c => c.Status == Status);

            if (!string.IsNullOrEmpty(Keyword))
                carsQuery = carsQuery.Where(c => c.Description.Contains(Keyword));

            // Prepare ViewModel
            var vm = new CustomerBrowseCarVM
            {
                Cars = carsQuery.ToList(),
                CarModelList = _context.CarModels
                                       .Select(cm => new SelectListItem { Value = cm.CarModelID.ToString(), Text = cm.ModelName })
                                       .ToList(),
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
