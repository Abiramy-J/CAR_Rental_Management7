using Car_Rental_Management.Data;
using Car_Rental_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Car_Rental_Management.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _db;
        public PaymentController(ApplicationDbContext db) => _db = db;

        // GET: Payment/Method
        public IActionResult Method(int id)
        {
            var booking = _db.Bookings
                             .Include(b => b.Car)
                             .ThenInclude(c => c.CarModel)
                             .FirstOrDefault(b => b.BookingID == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Payment/Confirm
        [HttpPost]
        public IActionResult Confirm(int bookingId, string paymentType)
        {
            var booking = _db.Bookings.Find(bookingId);
            if (booking == null) return NotFound();

            // Update booking
            booking.Status = "Paid";
            booking.PaymentDate = DateTime.Now;
            booking.PaymentMethod = paymentType; 

            _db.SaveChanges();

            return RedirectToAction("Receipt", new { id = booking.BookingID });
        }


          // Receipt.cshtml  show 
        public IActionResult Receipt(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var booking = _db.Bookings
                .Include(b => b.Car).ThenInclude(c => c.CarModel)
                .Include(b => b.Location)
                .Include(b => b.Driver)
                .FirstOrDefault(b => b.BookingID == id && b.CustomerID == userId.Value);

            if (booking == null) return NotFound();

            return View(booking); 
        }


        public IActionResult DownloadReceipt(int id)
        {
            var booking = _db.Bookings
                .Include(b => b.Car).ThenInclude(c => c.CarModel)
                .Include(b => b.Customer)
                .Include(b => b.Location)
                .Include(b => b.Driver)
                .FirstOrDefault(b => b.BookingID == id);

            if (booking == null) return NotFound();

            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                   
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().AlignLeft().Text("🚘 Car Rental Management")
                            .Bold().FontSize(18);
                        row.RelativeItem().AlignRight().Text($"Date: {DateTime.Now:dd/MM/yyyy}");
                    });

                    
                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Text("Booking Receipt").Bold().FontSize(16).Underline();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Text("Booking ID:").SemiBold();
                            table.Cell().Text(booking.BookingID.ToString());

                            

                            table.Cell().Text("Car:").SemiBold();
                            table.Cell().Text($"{booking.Car.CarModel.Brand} {booking.Car.CarModel.ModelName}");

                            table.Cell().Text("Pickup Date:").SemiBold();
                            table.Cell().Text($"{booking.PickupDate:dd/MM/yyyy}");

                            table.Cell().Text("Return Date:").SemiBold();
                            table.Cell().Text($"{booking.ReturnDate:dd/MM/yyyy}");

                            table.Cell().Text("Pickup Location:").SemiBold();
                            table.Cell().Text(booking.Location.Address);

                            table.Cell().Text("Payment Method:").SemiBold();
                            table.Cell().Text(booking.PaymentMethod ?? "Not specified");

                            table.Cell().Text("Driver Needed:").SemiBold();
                            table.Cell().Text(booking.NeedDriver ? "Yes" : "No");

                            if (booking.NeedDriver)
                            {
                                if (booking.Driver != null)
                                {
                                    table.Cell().Text("Driver:").SemiBold();
                                    table.Cell().Text(booking.Driver.FullName);
                                }
                                else if (!string.IsNullOrEmpty(booking.AltDriverName))
                                {
                                    table.Cell().Text("Alt Driver:").SemiBold();
                                    table.Cell().Text($"{booking.AltDriverName} (IC: {booking.AltDriverIC}, License: {booking.AltDriverLicenseNo})");
                                }
                            }

                            table.Cell().Text("Total Paid:").SemiBold();
                            table.Cell().Text($"Rs. {booking.TotalAmount:N2}");
                        });
                    });

                    
                    page.Footer().AlignCenter().Text("⚠️ No Refund Policy | Thank you for booking with us!")
                        .FontSize(10).Italic();
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"Receipt_{booking.BookingID}.pdf");
        }

    }
}
