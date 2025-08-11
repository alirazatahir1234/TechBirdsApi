namespace TechBirdsWebAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int? ArticleId { get; set; } // For article comments
        public int? PostId { get; set; } // For post comments
        public string UserId { get; set; } = string.Empty; // Reference to ApplicationUser
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsApproved { get; set; } = true;
        
        // Navigation Properties
        public virtual ApplicationUser? User { get; set; }
    
        public virtual Post? Post { get; set; }
    }
}