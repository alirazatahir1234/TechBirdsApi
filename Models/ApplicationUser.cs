using Microsoft.AspNetCore.Identity;

namespace TechBirdsWebAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Basic Information
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        
        // Profile & Media
        public string? Avatar { get; set; }
        public string? Website { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Specialization { get; set; }
        
  
        public int PostsCount { get; set; } = 0;
        public int TotalViews { get; set; } = 0;
        public DateTime? LastActive { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public virtual ICollection<Post>? Posts { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
    }
}