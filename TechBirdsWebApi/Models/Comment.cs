namespace TechBirdsWebAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int PostId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public bool IsApproved { get; set; }
    }
}