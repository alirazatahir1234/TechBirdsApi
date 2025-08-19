using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechBirdsApi.Models;

namespace TechBirdsApi.Data
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
        public DbSet<MediaItem> MediaItems { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<PageRevision> PageRevisions { get; set; }
        
        // 🔐 Logging and Exception Tracking Tables
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<TechBirdsApi.Models.SystemException> SystemExceptions { get; set; }

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

            // Configure Page entity
            builder.Entity<Page>(entity =>
            {
                entity.ToTable("pages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Slug).HasMaxLength(200).IsRequired();
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Excerpt).HasMaxLength(1000);
                entity.Property(e => e.SeoTitle).HasMaxLength(200);
                entity.Property(e => e.SeoDescription).HasMaxLength(300);
                entity.Property(e => e.Template).HasMaxLength(100);
                entity.Property(e => e.MetaJson).HasColumnType("jsonb");

                entity.HasOne(e => e.Parent)
                      .WithMany(p => p.Children)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Author)
                      .WithMany()
                      .HasForeignKey(e => e.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.FeaturedMedia)
                      .WithMany()
                      .HasForeignKey(e => e.FeaturedMediaId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.UpdatedAt);
                entity.HasIndex(e => e.PublishedAt);
            });

            // Configure PageRevision entity
            builder.Entity<PageRevision>(entity =>
            {
                entity.ToTable("page_revisions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Version).IsRequired();
                entity.Property(e => e.ChangeSummary).HasMaxLength(500);
                entity.HasIndex(e => new { e.PageId, e.Version }).IsUnique();

                entity.HasOne(e => e.Page)
                      .WithMany(p => p.Revisions)
                      .HasForeignKey(e => e.PageId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure MediaItem entity
            builder.Entity<MediaItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.OriginalFileName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.MimeType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Url).HasMaxLength(500).IsRequired();
                entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
                entity.Property(e => e.StoragePath).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.ThumbnailPath).HasMaxLength(1000);
                entity.Property(e => e.Title).HasMaxLength(255);
                entity.Property(e => e.AltText).HasMaxLength(500);
                entity.Property(e => e.Caption).HasMaxLength(1000);
                entity.Property(e => e.Description).HasMaxLength(2000);

                entity.HasOne(e => e.UploadedBy)
                      .WithMany()
                      .HasForeignKey(e => e.UploadedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable("media_items");
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.UploadedByUserId);
                entity.HasIndex(e => e.IsDeleted);
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
            builder.Entity<TechBirdsApi.Models.SystemException>(entity =>
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