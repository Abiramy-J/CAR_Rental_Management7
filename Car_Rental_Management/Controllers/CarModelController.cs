using Car_Rental_Management.Data;
using Car_Rental_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Index()
        {
            var models = await _dbcontext.CarModels.ToListAsync();
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
        public async Task<IActionResult> Create(CarModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _dbcontext.CarModels.AddAsync(model);
                    await _dbcontext.SaveChangesAsync();
                    TempData["SuccessMessage"] = "✅ Car model created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    TempData["ErrorMessage"] = "❌ Error occurred while creating car model.";
                }
            }
            return View(model);
        }

        // GET: CarModel/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _dbcontext.CarModels.FindAsync(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "❌ Car model not found!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: CarModel/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CarModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _dbcontext.CarModels.Update(model);
                    await _dbcontext.SaveChangesAsync();
                    TempData["SuccessMessage"] = "✅ Car model updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["ErrorMessage"] = "❌ Car model update failed due to concurrency issue.";
                }
                catch
                {
                    TempData["ErrorMessage"] = "❌ Error occurred while updating car model.";
                }
            }
            return View(model);
        }

        // GET: CarModel/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _dbcontext.CarModels.FindAsync(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "❌ Car model not found!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: CarModel/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _dbcontext.CarModels.FindAsync(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "❌ Car model not found!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _dbcontext.CarModels.Remove(model);
                await _dbcontext.SaveChangesAsync();
                TempData["SuccessMessage"] = "✅ Car model deleted successfully!";
            }
            catch
            {
                TempData["ErrorMessage"] = "❌ Error occurred while deleting car model.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
