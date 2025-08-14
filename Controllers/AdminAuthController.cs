using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBirdsWebAPI.Models;
using TechBirdsWebAPI.Extension;
using TechBirdsWebAPI.Utils;
using TechBirdsWebAPI.Services;

namespace TechBirdsWebAPI.Controllers
{
    [Route("api/admin/auth")]
    [ApiController]
    public class AdminAuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly IUserActivityService _userActivityService;
        private readonly IExceptionLoggerService _exceptionLogger;

        public AdminAuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration config,
            IUserActivityService userActivityService,
            IExceptionLoggerService exceptionLogger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
            _userActivityService = userActivityService;
            _exceptionLogger = exceptionLogger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Invalid email or password" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Invalid email or password" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                

                var jwtSecret = _config["Jwt:Secret"];
                if (string.IsNullOrEmpty(jwtSecret))
                {
                    return StatusCode(500, new { message = "JWT configuration error" });
                }

                var token = JwtHelper.GenerateTokenForIdentityUser(user, roles.ToList(), jwtSecret);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                var adminUser = new AdminUserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Name = user.Name,
                    Email = user.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "Admin",
                    Bio = user.Bio,
                    Avatar = user.Avatar.ToBase64String(),
                    Website = user.Website,
                    Twitter = user.Twitter,
                    LinkedIn = user.LinkedIn,
                    Specialization = user.Specialization,
                    PostsCount = user.PostsCount,
                    TotalViews = user.TotalViews,
                    
                    // Timestamps
                    CreatedAt = user.CreatedAt,
                    JoinedAt = user.JoinedAt,
                    LastActive = user.LastActive
                };

                // Log successful admin login
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                await _userActivityService.LogLoginAsync(user.Id, ipAddress, userAgent, true, "Admin login successful");

                return Ok(new AuthResponse
                {
                    User = adminUser,
                    Token = token,
                    ExpiresAt = expiresAt
                });
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Admin login failed");
                return StatusCode(500, new { message = "Login failed", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AdminRegisterRequest request)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User with this email already exists" });
                }

                var user = new ApplicationUser
                {
                    // Identity fields
                    UserName = request.Email,
                    Email = request.Email,
                    
                    // Basic Information
                    Name = $"{request.FirstName} {request.LastName}",
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Bio = request.Bio ?? "Administrator",
                    Avatar = string.IsNullOrWhiteSpace(request.Avatar) ? null : request.Avatar.ToByteArray(),
                    Website = request.Website,
                    Twitter = request.Twitter,
                    LinkedIn = request.LinkedIn,
                    Specialization = request.Specialization ?? "System Administration",
                    PostsCount = 0,
                    TotalViews = 0,
                    
                    // Timestamps
                    CreatedAt = DateTime.UtcNow,
                    JoinedAt = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "User creation failed", errors = result.Errors });
                }

                // Assign role (default to Admin if not specified or invalid)
                var validRoles = new[] { "Subscriber", "Contributor", "Author", "Admin", "Editor", "SuperAdmin" };
                var roleToAssign = !string.IsNullOrEmpty(request.Role) && validRoles.Contains(request.Role) 
                    ? request.Role 
                    : "Admin";
                
                await _userManager.AddToRoleAsync(user, roleToAssign);

                // Log admin registration
                await _userActivityService.LogActivityAsync(
                    user.Id,
                    "UserRegistration",
                    $"New {roleToAssign} account created",
                    new { firstName = request.FirstName, lastName = request.LastName, email = request.Email, role = roleToAssign }
                );

                return Ok(new { message = $"{roleToAssign} user created successfully" });
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Admin registration failed");
                return StatusCode(500, new { message = "Registration failed", error = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCurrentAdmin()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized();
                }

                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                
                var adminUser = new AdminUserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Name = user.Name,
                    Email = user.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "Admin",
                    Bio = user.Bio,
                    Avatar = user.Avatar.ToBase64String(),
                    Website = user.Website,
                    Twitter = user.Twitter,
                    LinkedIn = user.LinkedIn,
                    Specialization = user.Specialization,
                    PostsCount = user.PostsCount,
                    TotalViews = user.TotalViews,
                    
                    // Timestamps
                    CreatedAt = user.CreatedAt,
                    JoinedAt = user.JoinedAt,
                    LastActive = user.LastActive
                };

                return Ok(adminUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user info", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                    await _userActivityService.LogLogoutAsync(userId, ipAddress, userAgent);
                }

                // For JWT tokens, logout is typically handled client-side by removing the token
                // You could implement token blacklisting here if needed
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Admin logout error");
                return Ok(new { message = "Logged out successfully" }); // Still return success for logout
            }
        }
    }

    // Request/Response DTOs
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
        
        // Optional Profile Fields
        public string? Bio { get; set; }
        public string? Avatar { get; set; } // base64 string from frontend
        public string? Website { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Specialization { get; set; }
        
        // Role Assignment (defaults to Admin if not specified)
        public string? Role { get; set; }
        public bool? IsActive { get; set; } // Additional field from frontend
    }

    public class AdminUserResponse
    {
        public string Id { get; set; } = string.Empty; // Keep as string for GUID
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        
        // Profile & Media
        public string? Avatar { get; set; } // base64 string for frontend
        public string? Website { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Specialization { get; set; }
        
        // Content Creator Stats
        public int ArticleCount { get; set; }
        public int PostsCount { get; set; }
        public int TotalViews { get; set; }
        
        // Timestamps
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
