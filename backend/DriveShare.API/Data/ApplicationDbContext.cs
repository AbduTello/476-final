using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DriveShare.API.Models;

namespace DriveShare.API.Data
{
    // This context inherits from IdentityDbContext with your custom ApplicationUser.
    // It will automatically include the Identity tables (e.g., AspNetUsers, AspNetRoles, etc.).
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // The constructor accepts DbContextOptions and passes them to the base class.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // Define additional DbSets here.
        // For example, a DbSet for CarListing entities.
        public DbSet<CarListing> CarListings { get; set; }

        // Optionally, override OnModelCreating to configure your model.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure additional mappings or constraints here.
            // For example, renaming Identity tables if needed:
            // builder.Entity<ApplicationUser>(entity => { entity.ToTable("MyUsers"); });
        }
    }
}
