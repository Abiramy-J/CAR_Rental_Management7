using Car_Rental_Management.Data;
using Car_Rental_Management.Models;
using Microsoft.AspNetCore.Mvc;

namespace Car_Rental_Management.Controllers
{
    public class CarModelController : Controller
    {
        private readonly ApplicationDbContext _dbcontext;

        public CarModelController(ApplicationDbContext context)
        {
            _dbcontext = context;
        }

        // GET: CarModel
        public IActionResult Index()
        {
            var models = _dbcontext.CarModels.ToList();
            return View(models);
        }

        // GET: CarModel/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CarModel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CarModel model)
        {
            if (ModelState.IsValid)
            {
                _dbcontext.CarModels.Add(model);
                _dbcontext.SaveChanges();
                TempData["SuccessMessage"] = "✅ Car model created successfully!";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "❌ Failed to create car model.";
            return View(model);
        }

        // GET: CarModel/Edit/5
        public IActionResult Edit(int id)
        {
            var model = _dbcontext.CarModels.Find(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CarModel model)
        {
            if (ModelState.IsValid)
            {
                _dbcontext.CarModels.Update(model);
                _dbcontext.SaveChanges();
                TempData["SuccessMessage"] = "✅ Car model updated successfully!";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "❌ Failed to update car model.";
            return View(model);
        }

        // GET: CarModel/Delete/5
        public IActionResult Delete(int id)
        {
            var model = _dbcontext.CarModels.Find(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var model = _dbcontext.CarModels.Find(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "❌ Car model not found!";
                return RedirectToAction("Index");
            }

            _dbcontext.CarModels.Remove(model);
            _dbcontext.SaveChanges();
            TempData["SuccessMessage"] = "✅ Car model deleted successfully!";
            return RedirectToAction("Index");
        }

    }
}
