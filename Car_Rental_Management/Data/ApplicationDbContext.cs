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


    }
}
