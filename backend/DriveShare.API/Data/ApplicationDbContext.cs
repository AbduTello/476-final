using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DriveShare.API.Models; // Update this namespace according to your project structure

namespace DriveShare.API.Data
{
    // Ensure ApplicationUser is defined in DriveShare.API.Models (or adjust the namespace)
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // Define additional DbSets as needed.
        public DbSet<CarListing> CarListings { get; set; }
    }
}
