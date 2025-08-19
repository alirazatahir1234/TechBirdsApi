using Microsoft.AspNetCore.Identity;

namespace TechBirdsWebAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public byte[]? Avatar { get; set; }
        public string? Website { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Specialization { get; set; }
        public int PostsCount { get; set; }
        public int TotalViews { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}