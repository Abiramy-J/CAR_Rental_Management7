using Car_Rental_Management.Data;
using Car_Rental_Management.Models;
using Car_Rental_Management.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Car_Rental_Management.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // ------------------- LOGIN -------------------
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var inputUsername = model.Username?.Trim().ToLower();
            var inputPassword = model.Password?.Trim();

            // Hardcoded admin login
            if (inputUsername == "admin" && inputPassword == "admin123")
            {
                HttpContext.Session.SetInt32("UserId", 0);
                HttpContext.Session.SetString("Username", "admin");
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("ProfileImageUrl", "/images/default-profile.png");
                return RedirectToAction("Dashboard", "Admin");
            }

            var user = _db.Users.FirstOrDefault(u =>
                u.Username.ToLower() == inputUsername &&
                u.Password == inputPassword); // ⚠ plain text password

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("ProfileImageUrl", user.ProfileImageUrl ?? "/images/default-profile.png");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return user.Role switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin"),
                    "Staff" => RedirectToAction("Dashboard", "Staff"),
                    "Customer" => RedirectToAction("Dashboard", "Customer"),
                    _ => RedirectToAction("Login")
                };
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password");
            return View(model);
        }

        // ------------------- REGISTER -------------------
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            if (_db.Users.Any(u => u.Username.ToLower() == model.Username.Trim().ToLower()))
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Username = model.Username.Trim(),
                Password = model.Password.Trim(),
                Email = model.Email.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Role = "Customer",
                ProfileImageUrl = "/images/default-profile.png"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("ProfileImageUrl", user.ProfileImageUrl);

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

        // ------------------- PROFILE -------------------
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _db.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _db.Users.Find(userId.Value);
            if (user == null) return RedirectToAction("Login");

            var vm = new EditProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            // Update image if uploaded
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/uploads");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImage.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(stream);
                }

                user.ProfileImageUrl = "/images/uploads/" + fileName;
                HttpContext.Session.SetString("ProfileImageUrl", user.ProfileImageUrl);
            }

            // Update other fields
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            _db.Update(user);
            await _db.SaveChangesAsync();

            // ✅ Redirect to Profile after success
            return RedirectToAction("Profile");
        }

    }
}
