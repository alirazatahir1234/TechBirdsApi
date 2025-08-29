using System;
using System.Collections.Generic;

namespace TechBirdsApi.Dto
{
    public class UserQueryParams
    {
        private int _page = 1;
        private int _limit = 10;
        public int Page { get => _page; set => _page = value > 0 ? value : 1; }
        public int Limit { get => _limit; set => _limit = value > 0 && value <= 100 ? value : 10; }
        private string? _search;
        public string? Search { get => _search; set => _search = value?.Length > 100 ? value.Substring(0, 100) : value; }
        public string? Role { get; set; }
        public string? Specialization { get; set; }
        public string? SortBy { get; set; } = "joined";
        public string? SortOrder { get; set; } = "desc";
    }

    public class UserPublicProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? Website { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Specialization { get; set; }
        public int ArticleCount { get; set; }
        public int PostsCount { get; set; }
        public int TotalViews { get; set; }
        public string JoinedAt { get; set; } = string.Empty;
        public string? LastActive { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Email { get; set; }
    }

    public class UserDetailedProfile : UserPublicProfile
    {
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public List<UserArticleSummary> RecentArticles { get; set; } = new();
    }

    public class UserPublicDetailedProfile : UserPublicProfile
    {
        public List<UserArticleSummary> RecentArticles { get; set; } = new();
    }

    public class UserArticleSummary
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string PublishedAt { get; set; } = string.Empty;
        public int ViewCount { get; set; }
    }

    public class UpdateUserProfileRequest
    {
        private string _name = string.Empty;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        public string Name { get => _name; set => _name = value?.Length > 100 ? value.Substring(0, 100) : value ?? string.Empty; }
        public string FirstName { get => _firstName; set => _firstName = value?.Length > 50 ? value.Substring(0, 50) : value ?? string.Empty; }
        public string LastName { get => _lastName; set => _lastName = value?.Length > 50 ? value.Substring(0, 50) : value ?? string.Empty; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Bio { get; set; }
        public string? Avatar { get; set; }
        public string? Website { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Specialization { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UserListResponse
    {
        public List<UserPublicProfile> Users { get; set; } = new();
        public PaginationMeta Pagination { get; set; } = new();
    }

    public class ResolveExceptionRequest
    {
        public string? Resolution { get; set; }
    }

    public class CreateUserRequest
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
        public bool? IsActive { get; set; } = true;
    }

    public class PaginationMeta
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }
    }
}
