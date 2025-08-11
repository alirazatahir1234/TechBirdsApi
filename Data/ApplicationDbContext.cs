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

        // DbSets for all entities
        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        
        // 🔐 Logging and Exception Tracking Tables
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<Models.SystemException> SystemExceptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure table names to match existing database
          
 
            builder.Entity<Post>().ToTable("posts");
            builder.Entity<Category>().ToTable("categories");
            builder.Entity<Comment>().ToTable("comments");
            
            // Configure ApplicationUser entity
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Bio).HasMaxLength(1000);
                entity.Property(e => e.Avatar).HasMaxLength(500);
                entity.Property(e => e.Website).HasMaxLength(200);
                entity.Property(e => e.Twitter).HasMaxLength(100);
                entity.Property(e => e.LinkedIn).HasMaxLength(200);
                entity.Property(e => e.Specialization).HasMaxLength(200);
            });
            
            
            // Configure Post entity
            builder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Title).HasColumnName("title");
                entity.Property(e => e.Content).HasColumnName("content");
                entity.Property(e => e.Summary).HasColumnName("summary");
                entity.Property(e => e.ImageUrl).HasColumnName("imageurl");
                entity.Property(e => e.UserId).HasColumnName("userid").IsRequired();
                entity.Property(e => e.CategoryId).HasColumnName("categoryid");
                entity.Property(e => e.Tags).HasColumnName("tags");
                entity.Property(e => e.PublishedAt).HasColumnName("publishedat");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
                entity.Property(e => e.Type).HasColumnName("type");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.Featured).HasColumnName("featured");
                entity.Property(e => e.AllowComments).HasColumnName("allowcomments");
                entity.Property(e => e.ViewCount).HasColumnName("viewcount");
                entity.Property(e => e.LikeCount).HasColumnName("likecount");
                entity.Property(e => e.ShareCount).HasColumnName("sharecount");
                entity.Property(e => e.ExternalUrl).HasColumnName("externalurl");
                entity.Property(e => e.ExternalSource).HasColumnName("externalsource");
                
                // Foreign key relationships
                entity.HasOne(p => p.User)
                      .WithMany(u => u.Posts)
                      .HasForeignKey(p => p.UserId);
                      
                entity.HasOne(p => p.Category)
                      .WithMany()
                      .HasForeignKey(p => p.CategoryId);
            });
            
            // Configure Comment entity
            builder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Content).HasMaxLength(2000);
                
                // Foreign key relationships
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.UserId);
                      
              
                      
                entity.HasOne(c => c.Post)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(c => c.PostId);
            });
            
            // 🔐 Configure UserActivity entity
            builder.Entity<UserActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.RequestPath).HasMaxLength(200);
                entity.Property(e => e.HttpMethod).HasMaxLength(10);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                
                // Foreign key relationship
                entity.HasOne(ua => ua.User)
                      .WithMany()
                      .HasForeignKey(ua => ua.UserId);
                      
                // Index for performance
                entity.HasIndex(ua => ua.UserId);
                entity.HasIndex(ua => ua.CreatedAt);
                entity.HasIndex(ua => ua.Action);
            });
            
            // 🔐 Configure SystemException entity
            builder.Entity<Models.SystemException>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExceptionType).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(2000).IsRequired();
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.RequestPath).HasMaxLength(200);
                entity.Property(e => e.HttpMethod).HasMaxLength(10);
                entity.Property(e => e.Severity).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Source).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.ResolvedBy).HasMaxLength(100);
                
                // Foreign key relationship (optional)
                entity.HasOne(se => se.User)
                      .WithMany()
                      .HasForeignKey(se => se.UserId);
                      
                // Indexes for performance
                entity.HasIndex(se => se.UserId);
                entity.HasIndex(se => se.CreatedAt);
                entity.HasIndex(se => se.Severity);
                entity.HasIndex(se => se.IsResolved);
            });
            
            // Seed roles - Updated role system
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole("Subscriber") { Id = "1", NormalizedName = "SUBSCRIBER" },
                new ApplicationRole("Contributor") { Id = "2", NormalizedName = "CONTRIBUTOR" },
                new ApplicationRole("Author") { Id = "3", NormalizedName = "AUTHOR" },
                new ApplicationRole("Admin") { Id = "4", NormalizedName = "ADMIN" },
                new ApplicationRole("Editor") { Id = "5", NormalizedName = "EDITOR" },
                new ApplicationRole("SuperAdmin") { Id = "6", NormalizedName = "SUPERADMIN" }
            );
        }
    }
}