using System.Diagnostics;
using CAR.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAR.Controllers
{

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Book(string name, string email, DateTime date, string car)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(car))
            {
                ViewBag.Error = "Please fill all details correctly.";
                return View("Index");
            }

            // Inga booking save panna DB logic podalaam (future la)
            ViewBag.Message = "🎉 Booking Confirmed! We’ll contact you shortly.";
            return View("Index");
        }
    }
}
