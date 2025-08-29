using System;

namespace TechBirdsApi.Dto
{
    public class AdminLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AdminRegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? Avatar { get; set; }
        public string? Website { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Specialization { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
    }

    public class AdminUserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? Website { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Specialization { get; set; }
        public int ArticleCount { get; set; }
        public int PostsCount { get; set; }
        public int TotalViews { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LastActive { get; set; }
    }

    public class AuthResponse
    {
        public AdminUserResponse User { get; set; } = new();
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
