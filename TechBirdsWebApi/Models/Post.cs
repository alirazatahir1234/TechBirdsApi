namespace TechBirdsWebAPI.Models
{
    public class Post
    {
        // Core Properties
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; } // Brief summary for quick posts
        
        // Media & Visual
        public string? ImageUrl { get; set; }
        
        // Content Organization
        public string UserId { get; set; } = string.Empty; // Reference to ApplicationUser
        public int? CategoryId { get; set; } // Optional for posts
        public string? Tags { get; set; } // JSON array as string
        
        // Publishing & Status
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Post Type & Features
        public string Type { get; set; } = "update"; // update, announcement, quick, social
        public string Status { get; set; } = "draft"; // draft, published, archived
        public bool Featured { get; set; } = false;
        public bool AllowComments { get; set; } = true;
        
        // Engagement Metrics
        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;
        public int ShareCount { get; set; } = 0;
        
        // External Links (for sharing articles or external content)
        public string? ExternalUrl { get; set; }
        public string? ExternalSource { get; set; }
        
        // Navigation Properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Category? Category { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
    }
}
