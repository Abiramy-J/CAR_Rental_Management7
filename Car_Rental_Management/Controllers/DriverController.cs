using Car_Rental_Management.Data;
using Car_Rental_Management.Models;
using Microsoft.AspNetCore.Mvc;

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

        // Add driver (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Add driver (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Driver driver)
        {
            if (!ModelState.IsValid) return View(driver);

            _db.Drivers.Add(driver);
            _db.SaveChanges();
            TempData["Success"] = "Driver added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Edit driver (GET)
        public IActionResult Edit(int id)
        {
            var driver = _db.Drivers.Find(id);
            if (driver == null) return NotFound();
            return View(driver);
        }

        // Edit driver (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Driver driver)
        {
            if (!ModelState.IsValid) return View(driver);

            _db.Drivers.Update(driver);
            _db.SaveChanges();
            TempData["Success"] = "Driver updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/DeleteDriver/5
        public IActionResult Delete(int id)
        {
            var driver = _db.Drivers.FirstOrDefault(d => d.DriverId == id);
            if (driver == null) return NotFound();

            return View(driver);
        }

        // POST: Admin/DeleteDriver/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var driver = _db.Drivers.FirstOrDefault(d => d.DriverId == id);
            if (driver == null) return NotFound();

            _db.Drivers.Remove(driver);
            _db.SaveChanges();

            TempData["Success"] = "Driver deleted successfully!";
            return RedirectToAction("Index");
        }

    }
}

