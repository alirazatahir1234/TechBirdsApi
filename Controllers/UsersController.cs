using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechBirdsWebAPI.Extension;
using TechBirdsWebAPI.Data;
using TechBirdsWebAPI.Models;
using TechBirdsWebAPI.Services;

namespace TechBirdsWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserActivityService _userActivityService;
        private readonly IExceptionLoggerService _exceptionLogger;

        public UsersController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IUserActivityService userActivityService,
            IExceptionLoggerService exceptionLogger)
        {
            _context = context;
            _userManager = userManager;
            _userActivityService = userActivityService;
            _exceptionLogger = exceptionLogger;
        }
    // ...existing code...

        // GET: api/users - List all users (public profiles)
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserQueryParams parameters)
        {
            try
            {
                var query = _context.Users.AsQueryable();
                
                // Apply filters
                if (!string.IsNullOrEmpty(parameters.Search))
                {
                    query = query.Where(u => 
                        u.Name.Contains(parameters.Search) ||
                        u.FirstName.Contains(parameters.Search) ||
                        u.LastName.Contains(parameters.Search) ||
                        (u.Email != null && u.Email.Contains(parameters.Search)) ||
                        (u.Bio != null && u.Bio.Contains(parameters.Search)) ||
                        (u.Specialization != null && u.Specialization.Contains(parameters.Search)));
                }

                if (!string.IsNullOrEmpty(parameters.Role))
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(parameters.Role);
                    var userIds = usersInRole.Select(u => u.Id).ToList();
                    query = query.Where(u => userIds.Contains(u.Id));
                }

                if (!string.IsNullOrEmpty(parameters.Specialization))
                {
                    query = query.Where(u => u.Specialization != null && u.Specialization.Contains(parameters.Specialization));
                }

                // Apply sorting
                switch (parameters.SortBy?.ToLower())
                {
                    case "name":
                        query = parameters.SortOrder?.ToLower() == "asc"
                            ? query.OrderBy(u => u.Name)
                            : query.OrderByDescending(u => u.Name);
                        break;
                    case "posts":
                        query = parameters.SortOrder?.ToLower() == "asc"
                            ? query.OrderBy(u => u.PostsCount)
                            : query.OrderByDescending(u => u.PostsCount);
                        break;
                    case "views":
                        query = parameters.SortOrder?.ToLower() == "asc"
                            ? query.OrderBy(u => u.TotalViews)
                            : query.OrderByDescending(u => u.TotalViews);
                        break;
                    default: // joined date
                        query = parameters.SortOrder?.ToLower() == "asc"
                            ? query.OrderBy(u => u.JoinedAt)
                            : query.OrderByDescending(u => u.JoinedAt);
                        break;
                }

                var totalCount = await query.CountAsync();
                var users = await query
                    .Skip((parameters.Page - 1) * parameters.Limit)
                    .Take(parameters.Limit)
                    .ToListAsync();

                var userResponses = new List<UserPublicProfile>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userResponses.Add(new UserPublicProfile
                    {
                        Id = user.Id,
                        Name = user.Name,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Bio = user.Bio,
                        Avatar = user.Avatar.ToBase64String(), // Convert byte[] to base64 string
                        Website = user.Website,
                        Twitter = user.Twitter,
                        LinkedIn = user.LinkedIn,
                        Specialization = user.Specialization,
                        PostsCount = user.PostsCount,
                        TotalViews = user.TotalViews,
                        JoinedAt = user.JoinedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        LastActive = user.LastActive?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        Role = roles.FirstOrDefault() ?? "Subscriber",
                        Username = user.UserName, // Added
                        Email = user.Email // Added
                    });
                }

                var response = new UserListResponse
                {
                    Users = userResponses,
                    Pagination = new PaginationMeta
                    {
                        Page = parameters.Page,
                        Limit = parameters.Limit,
                        Total = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / parameters.Limit),
                        HasNext = parameters.Page < (int)Math.Ceiling((double)totalCount / parameters.Limit),
                        HasPrev = parameters.Page > 1
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // 🔐 Log exception with context
                await _exceptionLogger.LogExceptionAsync(ex, HttpContext, "UsersController.GetUsers", null, "UserManagement");
                
                // 🔒 SECURITY: Don't expose internal error details
                return StatusCode(500, new { message = "Error retrieving users" });
            }
        }

        // POST: api/users - Create new user (Admin only - redirects to admin auth)
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Editor")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                // � DEBUG: Log the incoming request
                Console.WriteLine($"[DEBUG] CreateUser called with: {System.Text.Json.JsonSerializer.Serialize(request)}");
                
                // �🔒 SECURITY: Only admins can create users
                var currentUserId = _userManager.GetUserId(User);
                if (currentUserId == null)
                {
                    Console.WriteLine($"[DEBUG] Unauthorized - currentUserId is null");
                    return Unauthorized();
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(request.FirstName))
                {
                    return BadRequest(new { message = "First name is required" });
                }

                if (string.IsNullOrWhiteSpace(request.LastName))
                {
                    return BadRequest(new { message = "Last name is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { message = "Email is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { message = "Password is required" });
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User with this email already exists" });
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    Name = $"{request.FirstName} {request.LastName}",
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Bio = request.Bio ?? "",
                    Avatar = string.IsNullOrWhiteSpace(request.Avatar) ? null : request.Avatar.ToByteArray(), // Convert base64 to byte[]
                    Website = request.Website,
                    Twitter = request.Twitter,
                    LinkedIn = request.LinkedIn,
                    Specialization = request.Specialization,
                    PostsCount = 0,
                    TotalViews = 0,
                    CreatedAt = DateTime.UtcNow,
                    JoinedAt = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "User creation failed", errors = result.Errors });
                }

                // Assign role (default to Subscriber if not specified or invalid)
                var validRoles = new[] { "Subscriber", "Contributor", "Author", "Admin", "Editor", "SuperAdmin" };
                var roleToAssign = !string.IsNullOrEmpty(request.Role) && validRoles.Contains(request.Role) 
                    ? request.Role 
                    : "Subscriber";
                
                await _userManager.AddToRoleAsync(user, roleToAssign);

                // Log user creation
                await _userActivityService.LogActivityAsync(
                    currentUserId,
                    "USER_CREATED",
                    $"Created new {roleToAssign} user: {request.Email}",
                    new { 
                        CreatedUserId = user.Id,
                        Email = request.Email, 
                        Role = roleToAssign,
                        FirstName = request.FirstName,
                        LastName = request.LastName
                    }
                );

                return Ok(new { 
                    message = $"{roleToAssign} user created successfully",
                    userId = user.Id,
                    email = user.Email,
                    role = roleToAssign,
                    avatar = user.Avatar // ✅ ADDED: Include avatar in response
                });
            }
            catch (Exception ex)
            {
                var currentUserId = _userManager.GetUserId(User);
                await _exceptionLogger.LogExceptionAsync(ex, HttpContext, "UsersController.CreateUser", currentUserId, "UserManagement");
                
                return StatusCode(500, new { message = "Error creating user" });
            }
        }

        // GET: api/users/{id} - Get user profile by ID (PUBLIC - no email)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Posts)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                
                // 🔒 SECURITY: Return public profile without sensitive data
                var userProfile = new UserPublicDetailedProfile
                {
                    Id = user.Id,
                    Name = user.Name,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Bio = user.Bio,
                    Avatar = user.Avatar.ToBase64String(),
                    Website = user.Website,
                    Twitter = user.Twitter,
                    LinkedIn = user.LinkedIn,
                    Specialization = user.Specialization,
                    PostsCount = user.PostsCount,
                    TotalViews = user.TotalViews,
                    JoinedAt = user.JoinedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    LastActive = user.LastActive?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Role = roles.FirstOrDefault() ?? "Subscriber",
                    Username = user.UserName, // Added
                    Email = user.Email // Added
                };

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                // 🔐 Log exception with context
                await _exceptionLogger.LogExceptionAsync(ex, HttpContext, "UsersController.GetUser", null, "UserManagement");
                
                // 🔒 SECURITY: Don't expose internal error details
                return StatusCode(500, new { message = "Error retrieving user" });
            }
        }

        // GET: api/users/profile - Get current user's profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var user = await _context.Users
                    .Include(u => u.Posts)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new { message = "User profile not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                
                var userProfile = new UserDetailedProfile
                {
                    Id = user.Id,
                    Name = user.Name,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Bio = user.Bio,
                    Avatar = user.Avatar.ToBase64String(),
                    Website = user.Website,
                    Twitter = user.Twitter,
                    LinkedIn = user.LinkedIn,
                    Specialization = user.Specialization,
                    PostsCount = user.PostsCount,
                    TotalViews = user.TotalViews,
                    JoinedAt = user.JoinedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    LastActive = user.LastActive?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Role = roles.FirstOrDefault() ?? "Subscriber",
                    Roles = roles.ToList(), // ✅ ADDED: Complete list of user roles
                    IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow, // ✅ ADDED: Account active status
                    
                };

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                // 🔐 Log exception with user context
                var userId = _userManager.GetUserId(User);
                await _exceptionLogger.LogExceptionAsync(ex, HttpContext, "UsersController.GetCurrentUserProfile", userId, "UserManagement");
                
                // 🔒 SECURITY: Don't expose internal error details
                return StatusCode(500, new { message = "Error retrieving profile" });
            }
        }

        // PUT: api/users/profile - Update current user's profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(UpdateUserProfileRequest request)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(request.FirstName))
                    return BadRequest(new { message = "First name is required" });
                if (string.IsNullOrWhiteSpace(request.LastName))
                    return BadRequest(new { message = "Last name is required" });

                var userId = _userManager.GetUserId(User);
                if (userId == null)
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                // Update profile fields
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Name = string.IsNullOrEmpty(request.Name) ? $"{request.FirstName} {request.LastName}" : request.Name;
                user.Bio = request.Bio ?? "";
                user.Avatar = string.IsNullOrWhiteSpace(request.Avatar) ? null : request.Avatar.ToByteArray();
                user.Website = request.Website;
                user.Twitter = request.Twitter;
                user.LinkedIn = request.LinkedIn;
                user.Specialization = request.Specialization;
                user.UpdatedAt = DateTime.UtcNow;
                user.LastActive = DateTime.UtcNow;

                // Update email if requested
                if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(request.Email);
                    if (existingUser != null && existingUser.Id != userId)
                        return BadRequest(new { message = "Email already exists" });
                    user.Email = request.Email;
                    user.UserName = request.Email;
                }

                // Update isActive status if provided
                if (request.IsActive.HasValue)
                {
                    var currentUserRoles = await _userManager.GetRolesAsync(user);
                    bool isAdmin = currentUserRoles.Any(r => new[] { "Admin", "SuperAdmin" }.Contains(r));
                    if (isAdmin)
                    {
                        user.LockoutEnabled = !request.IsActive.Value;
                        user.LockoutEnd = request.IsActive.Value ? null : DateTimeOffset.MaxValue;
                    }
                    else
                    {
                        return Forbid("Only administrators can change user active status");
                    }
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(new { message = "Profile update failed", errors = result.Errors });

                // Handle password update if requested
                if (!string.IsNullOrEmpty(request.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.Password);
                    if (!passwordResult.Succeeded)
                        return BadRequest(new { message = "Password update failed", errors = passwordResult.Errors });
                }

                // Handle role update if requested
                if (!string.IsNullOrEmpty(request.Role))
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (!currentRoles.Contains(request.Role))
                    {
                        if (currentRoles.Count > 0)
                        {
                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                            if (!removeResult.Succeeded)
                                return BadRequest(new { message = "Failed to remove current roles", errors = removeResult.Errors });
                        }
                        var addResult = await _userManager.AddToRoleAsync(user, request.Role);
                        if (!addResult.Succeeded)
                            return BadRequest(new { message = "Failed to assign new role", errors = addResult.Errors });
                    }
                }

                // Log profile update
                await _userActivityService.LogProfileUpdateAsync(
                    userId,
                    Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    Request.Headers.UserAgent.ToString(),
                    new { UpdatedFields = new { request.FirstName, request.LastName, request.Bio, request.Specialization } }
                );

                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                var userId = _userManager.GetUserId(User);
                await _exceptionLogger.LogExceptionAsync(ex, HttpContext, "UsersController.UpdateUserProfile", userId, "UserManagement");
                return StatusCode(500, new { message = "Error updating profile" });
            }
        }

        // 🔐 GET: api/users/activities - Get current user's activities
        [HttpGet("activities")]
        [Authorize]
        public async Task<IActionResult> GetUserActivities([FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var activities = await _userActivityService.GetUserActivitiesAsync(userId, page, limit);
                
                return Ok(new { activities, pagination = new { page, limit, total = activities.Count } });
            }
            catch (Exception ex)
            {
                var userId = _userManager.GetUserId(User);
                await _exceptionLogger.LogExceptionAsync(ex, HttpContext, "UsersController.GetUserActivities", userId, "UserManagement");
                
                return StatusCode(500, new { message = "Error retrieving activities" });
            }
        }

        // 🔐 GET: api/users/admin/activities - Get all user activities (Admin only)
        [HttpGet("admin/activities")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllUserActivities([FromQuery] string? userId = null, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var activities = await _context.UserActivities
                    .Where(ua => userId == null || ua.UserId == userId)
                    .OrderByDescending(ua => ua.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();
                
                var totalCount = await _context.UserActivities
                    .Where(ua => userId == null || ua.UserId == userId)
                    .CountAsync();

                return Ok(new { 
                    activities, 
                    pagination = new { 
                        page, 
                        limit, 
                        total = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / limit)
                    } 
                });
            }
            catch (Exception ex)
            {
                var currentUserId = _userManager.GetUserId(User);
                await _exceptionLogger.LogExceptionAsync(ex, HttpContext, "UsersController.GetAllUserActivities", currentUserId, "AdminOperations");
                
                return StatusCode(500, new { message = "Error retrieving activities" });
            }
        }

        // 🔐 GET: api/users/admin/exceptions - Get system exceptions (Admin only)
        [HttpGet("admin/exceptions")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetSystemExceptions([FromQuery] string? userId = null, [FromQuery] bool unResolvedOnly = false, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                List<Models.SystemException> exceptions;
                
                if (unResolvedOnly)
                {
                    exceptions = await _exceptionLogger.GetUnresolvedExceptionsAsync(page, limit);
                }
                else
                {
                    exceptions = await _exceptionLogger.GetExceptionsAsync(userId, page, limit);
                }

                return Ok(new { 
                    exceptions, 
                    pagination = new { 
                        page, 
                        limit, 
                        total = exceptions.Count 
                    } 
                });
            }
            catch (Exception ex)
            {
                var currentUserId = _userManager.GetUserId(User);
                await _exceptionLogger.LogExceptionAsync(ex, HttpContext, "UsersController.GetSystemExceptions", currentUserId, "AdminOperations");
                
                return StatusCode(500, new { message = "Error retrieving exceptions" });
            }
        }

        // 🔐 PUT: api/users/admin/exceptions/{id}/resolve - Mark exception as resolved (Admin only)
        [HttpPut("admin/exceptions/{id}/resolve")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ResolveException(int id, [FromBody] ResolveExceptionRequest request)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var currentUser = await _userManager.FindByIdAsync(currentUserId!);
                
                await _exceptionLogger.MarkAsResolvedAsync(id, currentUser?.Name ?? "Admin", request.Resolution);
                
                return Ok(new { message = "Exception marked as resolved" });
            }
            catch (Exception ex)
            {
                var currentUserId = _userManager.GetUserId(User);
                await _exceptionLogger.LogExceptionAsync(ex, HttpContext, "UsersController.ResolveException", currentUserId, "AdminOperations");
                
                return StatusCode(500, new { message = "Error resolving exception" });
            }
        }

        // DELETE: api/users/{id} - Delete user (SuperAdmin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "User deletion failed", errors = result.Errors });
                }

                // Log user deletion
                var currentUserId = _userManager.GetUserId(User);
                await _userActivityService.LogActivityAsync(
                    currentUserId,
                    "USER_DELETED",
                    $"Deleted user: {user.Email}",
                    new { DeletedUserId = user.Id, Email = user.Email }
                );

                return Ok(new { message = "User deleted successfully", userId = user.Id });
            }
            catch (Exception ex)
            {
                var currentUserId = _userManager.GetUserId(User);
                await _exceptionLogger.LogExceptionAsync(ex, HttpContext, "UsersController.DeleteUser", currentUserId, "UserManagement");
                return StatusCode(500, new { message = "Error deleting user" });
            }
        }

        // Helper method to check if role change is a valid self-downgrade
        private static bool IsSelfDowngrade(IList<string> currentRoles, string newRole)
        {
            // Define role hierarchy (higher number = higher privilege)
            var roleHierarchy = new Dictionary<string, int>
            {
                { "Subscriber", 1 },
                { "Contributor", 2 },
                { "Author", 3 },
                { "Editor", 4 },
                { "Admin", 5 },
                { "SuperAdmin", 6 }
            };

            var currentHighestRole = currentRoles
                .Where(r => roleHierarchy.ContainsKey(r))
                .Select(r => roleHierarchy[r])
                .DefaultIfEmpty(1)
                .Max();

            var newRoleLevel = roleHierarchy.GetValueOrDefault(newRole, 1);

            // Allow self-downgrade (going to a lower privilege role)
            return newRoleLevel <= currentHighestRole && newRoleLevel < currentHighestRole;
        }
    }

    // DTOs for Users API
    public class UserQueryParams
    {
        private int _page = 1;
        private int _limit = 10;
        
        public int Page 
        { 
            get => _page; 
            set => _page = value > 0 ? value : 1; 
        }
        
        public int Limit 
        { 
            get => _limit; 
            set => _limit = value > 0 && value <= 100 ? value : 10; // Max 100 per page
        }
        
        private string? _search;
        public string? Search 
        { 
            get => _search; 
            set => _search = value?.Length > 100 ? value.Substring(0, 100) : value; // Max 100 chars
        }
        
        public string? Role { get; set; }
        public string? Specialization { get; set; }
        public string? SortBy { get; set; } = "joined"; // joined, name, articles, posts, views
        public string? SortOrder { get; set; } = "desc"; // asc, desc
    }

    public class UserPublicProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
    public string? Avatar { get; set; } // base64 string for frontend
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
        public string? Username { get; set; } // Added
        public string? Email { get; set; } // Added
    }

    public class UserDetailedProfile : UserPublicProfile
    {
        public List<string> Roles { get; set; } = new(); // ✅ ADDED: All user roles
        public bool IsActive { get; set; } = true; // ✅ ADDED: Account active status
        public List<UserArticleSummary> RecentArticles { get; set; } = new();
    }
    
    public class UserPublicDetailedProfile : UserPublicProfile
    {
        // 🔒 SECURITY: Public detailed profile without sensitive data (no email)
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
        
        public string Name 
        { 
            get => _name; 
            set => _name = value?.Length > 100 ? value.Substring(0, 100) : value ?? string.Empty;
        }
        
        public string FirstName 
        { 
            get => _firstName; 
            set => _firstName = value?.Length > 50 ? value.Substring(0, 50) : value ?? string.Empty;
        }
        
        public string LastName 
        { 
            get => _lastName; 
            set => _lastName = value?.Length > 50 ? value.Substring(0, 50) : value ?? string.Empty;
        }
        
        public string? Email { get; set; } // ✅ ADDED: Email update capability
        public string? Password { get; set; } // ✅ ADDED: Password update capability
        public string? Bio { get; set; }
        public string? Avatar { get; set; }
        public string? Website { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Specialization { get; set; }
        public string? Role { get; set; } // ✅ ADDED: Role update capability
        public bool? IsActive { get; set; } // ✅ ADDED: Active status update capability
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
