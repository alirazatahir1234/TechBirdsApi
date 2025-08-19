using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechBirdsWebAPI.Models;

namespace TechBirdsWebAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TechBirdsWebAPI.Models.SystemException> SystemExceptions { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Seed roles
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole("SuperAdmin") { Id = "1", NormalizedName = "SUPERADMIN" },
                new ApplicationRole("Admin") { Id = "2", NormalizedName = "ADMIN" },
                new ApplicationRole("User") { Id = "3", NormalizedName = "USER" }
            );
        }
    }
}