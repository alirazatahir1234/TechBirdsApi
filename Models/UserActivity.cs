namespace TechBirdsApi.Models
{
    public class UserActivity
    {
        public int Id { get; set; }
        
        // User Information
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        
        // Activity Details
        public string Action { get; set; } = string.Empty; // LOGIN, LOGOUT, PROFILE_UPDATE, etc.
        public string Description { get; set; } = string.Empty;
        public string? Details { get; set; } // JSON string for additional data
        
        // Request Information
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? RequestPath { get; set; }
        public string? HttpMethod { get; set; }
        
        // Timing
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public int? DurationMs { get; set; }
        
        // Status
        public bool IsSuccess { get; set; } = true;
        public string? ErrorMessage { get; set; }
        
        // Navigation Properties
        public virtual ApplicationUser? User { get; set; }
    }
}
