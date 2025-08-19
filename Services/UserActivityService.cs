using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TechBirdsApi.Data;
using TechBirdsApi.Models;

namespace TechBirdsApi.Services
{
    public interface IUserActivityService
    {
        Task LogActivityAsync(string userId, string action, string description, object? additionalData = null, bool isSuccess = true, string? errorMessage = null);
        Task LogLoginAsync(string userId, string ipAddress, string userAgent, bool isSuccess = true, string? errorMessage = null);
        Task LogLogoutAsync(string userId, string ipAddress, string userAgent);
        Task LogProfileUpdateAsync(string userId, string ipAddress, string userAgent, object? changes = null);
        Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int page = 1, int limit = 50);
    }

    public class UserActivityService : IUserActivityService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserActivityService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActivityAsync(string userId, string action, string description, object? additionalData = null, bool isSuccess = true, string? errorMessage = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var user = await _userManager.FindByIdAsync(userId);
                
                var activity = new UserActivity
                {
                    UserId = userId,
                    UserName = user?.Name,
                    UserEmail = user?.Email,
                    Action = action,
                    Description = description,
                    Details = additionalData != null ? JsonSerializer.Serialize(additionalData) : null,
                    IpAddress = GetClientIpAddress(httpContext),
                    UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
                    RequestPath = httpContext?.Request.Path,
                    HttpMethod = httpContext?.Request.Method,
                    CreatedAt = DateTime.UtcNow,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage
                };

                _context.UserActivities.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log to system exception table if activity logging fails
                await LogSystemExceptionAsync(ex, "UserActivityService.LogActivityAsync", userId);
            }
        }

        public async Task LogLoginAsync(string userId, string ipAddress, string userAgent, bool isSuccess = true, string? errorMessage = null)
        {
            await LogActivityAsync(
                userId, 
                "LOGIN", 
                isSuccess ? "User logged in successfully" : "Failed login attempt",
                new { IpAddress = ipAddress, UserAgent = userAgent },
                isSuccess,
                errorMessage
            );
        }

        public async Task LogLogoutAsync(string userId, string ipAddress, string userAgent)
        {
            await LogActivityAsync(
                userId,
                "LOGOUT",
                "User logged out",
                new { IpAddress = ipAddress, UserAgent = userAgent }
            );
        }

        public async Task LogProfileUpdateAsync(string userId, string ipAddress, string userAgent, object? changes = null)
        {
            await LogActivityAsync(
                userId,
                "PROFILE_UPDATE",
                "User updated profile",
                new { Changes = changes, IpAddress = ipAddress, UserAgent = userAgent }
            );
        }

        public async Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int page = 1, int limit = 50)
        {
            return await _context.UserActivities
                .Where(ua => ua.UserId == userId)
                .OrderByDescending(ua => ua.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
        }

        private string? GetClientIpAddress(HttpContext? httpContext)
        {
            if (httpContext == null) return null;

            // Check for IP in various headers (for reverse proxy scenarios)
            var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            }

            return ipAddress;
        }

        private async Task LogSystemExceptionAsync(Exception ex, string source, string? userId = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var systemException = new TechBirdsApi.Models.SystemException
                {
                    UserId = userId,
                    ExceptionType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message,
                    RequestPath = httpContext?.Request.Path,
                    HttpMethod = httpContext?.Request.Method,
                    IpAddress = GetClientIpAddress(httpContext),
                    UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
                    Severity = "Error",
                    Source = source,
                    Category = "Logging",
                    CreatedAt = DateTime.UtcNow
                };

                _context.SystemExceptions.Add(systemException);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // If we can't log to database, at least log to console/file
                Console.WriteLine($"Failed to log exception: {ex.Message}");
            }
        }
    }
}
