using Car_Rental_Management.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Car_Rental_Management.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<CarModel> CarModels { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<DriverBooking> DriverBookings { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>()
                .Property(c => c.DailyRate)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // ===== Seed default admin user =====
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    FullName = "System Administrator",
                    Username = "admin",
                    Password = "admin123",          
                    Email = "admin@gmail.com",
                    PhoneNumber = "0000000000",
                    Role = "Admin",
                    ProfileImageUrl = "/images/default-profile.png"
                }
                );
        }

    }
}
