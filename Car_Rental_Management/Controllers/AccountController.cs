using Car_Rental_Management.Data;
using Car_Rental_Management.Models;
using Car_Rental_Management.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Car_Rental_Management.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var inputUsername = model.Username?.Trim().ToLower();
            var inputPassword = model.Password?.Trim();

            // Hardcoded Admin login
            if (inputUsername == "admin" && inputPassword == "admin123")
            {
                HttpContext.Session.SetInt32("UserId", 0); // 0 = no DB ID
                HttpContext.Session.SetString("Username", "admin");
                HttpContext.Session.SetString("Role", "Admin");

                return RedirectToAction("Dashboard", "Admin");
            }

            // Check in database
            var user = _db.Users.FirstOrDefault(u =>
                u.Username.ToLower() == inputUsername &&
                u.Password == inputPassword); // ⚠️ Plain text, hash later

            if (user != null)
            {
                // Save session
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                // Redirect to original page if provided
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                // Otherwise redirect to dashboard
                return user.Role switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin"),
                    "Staff" => RedirectToAction("Dashboard", "Staff"),
                    "Customer" => RedirectToAction("Dashboard", "Customer"),
                    _ => RedirectToAction("Login")
                };
            }

            // Login failed
            ModelState.AddModelError(string.Empty, "Invalid username or password");
            return View(model);
        }

        // GET: /Account/Register
        //public IActionResult Register()
        //{
        //    return View();
        //}
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        // POST: /Account/Register
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Register(RegisterViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    // Check if username already exists
        //    if (_db.Users.Any(u => u.Username.ToLower() == model.Username.Trim().ToLower()))
        //    {
        //        ModelState.AddModelError("Username", "Username already exists");
        //        return View(model);
        //    }

        //    // Create new user
        //    var user = new User
        //    {
        //        Username = model.Username.Trim(),
        //        Password = model.Password.Trim(), // ⚠️ Plain text, hash later
        //        Email = model.Email.Trim(),
        //        Role = "Customer"
        //    };

        //    _db.Users.Add(user);
        //    await _db.SaveChangesAsync();

        //    // Save session
        //    HttpContext.Session.SetInt32("UserId", user.UserId);
        //    HttpContext.Session.SetString("Username", user.Username);
        //    HttpContext.Session.SetString("Role", user.Role);

        //    // Redirect to customer dashboard
        //    return RedirectToAction("Login", "Account");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_db.Users.Any(u => u.Username.ToLower() == model.Username.Trim().ToLower()))
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username.Trim(),
                Password = model.Password.Trim(), // ⚠️ Plain text, hash later
                Email = model.Email.Trim(),
                Role = "Customer"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Save session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            // Redirect to original page if returnUrl is provided
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Dashboard", "Customer");
        }

        // GET: /Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _db.Users.FirstOrDefault(u =>
                u.Username.ToLower() == model.Username.Trim().ToLower() &&
                u.Email.ToLower() == model.Email.Trim().ToLower());

            if (user == null)
            {
                ViewBag.Error = "Invalid username or email.";
                return View(model);
            }

            // Update password (⚠️ hashing recommended)
            user.Password = model.NewPassword.Trim();
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            // Set session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            // Redirect based on role
            return user.Role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Staff" => RedirectToAction("Dashboard", "Staff"),
                "Customer" => RedirectToAction("Dashboard", "Customer"),
                _ => RedirectToAction("Login")
            };
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
