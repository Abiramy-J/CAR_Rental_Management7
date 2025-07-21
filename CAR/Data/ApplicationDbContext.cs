using CAR.Models;
using Microsoft.EntityFrameworkCore;

namespace CAR.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Brand> Brands { get; set; }

    }
   
   
}
