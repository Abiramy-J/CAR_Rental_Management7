using Microsoft.AspNetCore.Mvc;
using CAR.Data;
using CAR.Models;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Security.AccessControl;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335; // Ensure this namespace matches where ApplicationDbContext is defined

namespace CAR.Controllers
{
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BrandController(ApplicationDbContext dbcontext, IWebHostEnvironment webHostEnvironment)
        {
            _dbcontext = dbcontext;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public IActionResult Index()
        {
            List<Models.Brand> brands = _dbcontext.Brands.ToList();
            return View(brands);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Brand brand)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;// Get the wwwroot path for file uploads
            var file = HttpContext.Request.Form.Files; // Get the uploaded file
            if (file.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString() ; // Generate a unique file name
                var upload = Path.Combine(wwwRootPath, "images/brand"); // Define the path to save the file
                var extensions = Path.GetExtension(file[0].FileName); // Get the file extension
                using(var fileStream = new FileStream(Path.Combine(upload , newFileName + extensions), FileMode.Create)) // Create a new file stream to save the file
                {
                    file[0].CopyTo(fileStream); // Save the uploaded file
                }
                brand.brandLogo = @"images\brand\" + newFileName + extensions; // Set the brand logo property to the saved file name
            }
           
            if (ModelState.IsValid)
            {
                _dbcontext.Brands.Add(brand);
                _dbcontext.SaveChanges();
                return RedirectToAction(nameof(Index));

            }
            return View();

        }
        [HttpGet]
        public IActionResult Details (Guid id)
        {
            Brand brand = _dbcontext.Brands.FirstOrDefault(x => x.Id == id);
            return View(brand);
        }
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            Brand brand = _dbcontext.Brands.FirstOrDefault(x => x.Id == id);
            return View(brand);
        }
        [HttpPost]
        public IActionResult Edit(Brand brand)
        {
            string webRootPath = _webHostEnvironment.WebRootPath; // Get the wwwroot path for file uploads
            var file = HttpContext.Request.Form.Files; // Get the uploaded file
            if (file.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString(); // Generate a unique file name
                var upload = Path.Combine(webRootPath, "images/brand"); // Define the path to save the file
                var extensions = Path.GetExtension(file[0].FileName); // Get the file extension

                //Delete old image 
                var objfromDb= _dbcontext.Brands.AsNoTracking().FirstOrDefault(x=>x.Id ==brand.Id);
                if (objfromDb.brandLogo != null)
                {
                    var oldImagePath = Path.Combine(webRootPath, objfromDb.brandLogo.Trim('\\'));

                    if (System.IO.File.Exists(oldImagePath))

                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }


                using (var fileStream = new FileStream(Path.Combine(upload, newFileName + extensions), FileMode.Create)) // Create a new file stream to save the file
                {
                    file[0].CopyTo(fileStream); // Save the uploaded file
                }
                brand.brandLogo = @"images\brand\" + newFileName + extensions; // Set the brand logo property to the saved file name
            }
            
            if (ModelState.IsValid)
            {
                var objfromDb = _dbcontext.Brands.AsNoTracking().FirstOrDefault(x => x.Id == brand.Id);
                objfromDb.Name = brand.Name;
                objfromDb.EstablishYear = brand.EstablishYear;

                if (brand.brandLogo != null) 
                {
                    objfromDb.brandLogo = brand.brandLogo;
                }

                _dbcontext.Brands.Update(objfromDb );
                _dbcontext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            Brand brand = _dbcontext.Brands.FirstOrDefault(x => x.Id == id);
            return View(brand);
        }
        [HttpPost]
        public IActionResult Delete(Brand brand)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;

            if (!string.IsNullOrEmpty(brand.brandLogo))
            {
                var objfromDb = _dbcontext.Brands.AsNoTracking().FirstOrDefault(x => x.Id == brand.Id);
                if (objfromDb.brandLogo != null)
                {
                    var fileName = Path.GetFileName(objfromDb.brandLogo);
                    var oldImagePath = Path.Combine(webRootPath, "images", "brand", fileName);


                    if (System.IO.File.Exists(oldImagePath))

                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
            }
            _dbcontext.Brands.Remove(brand); 
            _dbcontext.SaveChanges(true);

            return RedirectToAction(nameof(Index));
        }
        

    }
}
 