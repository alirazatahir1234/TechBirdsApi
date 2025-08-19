using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TechBirdsWebAPI.Data;
using TechBirdsWebAPI.Models;

namespace TechBirdsWebAPI.Services
{
    public interface IExceptionLoggerService
    {
        Task LogExceptionAsync(Exception ex, string source, string? userId = null, string category = "General", string severity = "Error");
        Task LogExceptionAsync(Exception ex, HttpContext httpContext, string source, string? userId = null, string category = "General");
        Task<List<Models.SystemException>> GetExceptionsAsync(string? userId = null, int page = 1, int limit = 50);
        Task<List<Models.SystemException>> GetUnresolvedExceptionsAsync(int page = 1, int limit = 50);
        Task MarkAsResolvedAsync(int exceptionId, string resolvedBy, string? resolution = null);
    }

    public class ExceptionLoggerService : IExceptionLoggerService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExceptionLoggerService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogExceptionAsync(Exception ex, string source, string? userId = null, string category = "General", string severity = "Error")
        {
            var httpContext = _httpContextAccessor.HttpContext;
            await LogExceptionAsync(ex, httpContext!, source, userId, category);
        }

        public async Task LogExceptionAsync(Exception ex, HttpContext httpContext, string source, string? userId = null, string category = "General")
        {
            try
            {
                string? userName = null;
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    userName = user?.Name;
                }

                var severity = DetermineSeverity(ex);
                var requestBody = await GetRequestBodyAsync(httpContext);

                var systemException = new Models.SystemException
                {
                    UserId = userId,
                    UserName = userName,
                    ExceptionType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message,
                    RequestPath = httpContext?.Request.Path,
                    HttpMethod = httpContext?.Request.Method,
                    QueryString = httpContext?.Request.QueryString.ToString(),
                    RequestBody = requestBody,
                    IpAddress = GetClientIpAddress(httpContext),
                    UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
                    Severity = severity,
                    Source = source,
                    Category = category,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SystemExceptions.Add(systemException);
                await _context.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                // Fallback logging to console if database logging fails
                Console.WriteLine($"Failed to log exception to database: {logEx.Message}");
                Console.WriteLine($"Original exception: {ex.Message}");
            }
        }

        public async Task<List<Models.SystemException>> GetExceptionsAsync(string? userId = null, int page = 1, int limit = 50)
        {
            var query = _context.SystemExceptions.AsQueryable();
            
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(se => se.UserId == userId);
            }

            return await query
                .OrderByDescending(se => se.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Models.SystemException>> GetUnresolvedExceptionsAsync(int page = 1, int limit = 50)
        {
            return await _context.SystemExceptions
                .Where(se => !se.IsResolved)
                .OrderByDescending(se => se.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
        }

        public async Task MarkAsResolvedAsync(int exceptionId, string resolvedBy, string? resolution = null)
        {
            var exception = await _context.SystemExceptions.FindAsync(exceptionId);
            if (exception != null)
            {
                exception.IsResolved = true;
                exception.ResolvedBy = resolvedBy;
                exception.Resolution = resolution;
                exception.ResolvedAt = DateTime.UtcNow;
                exception.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        private string DetermineSeverity(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => "Warning",
                ArgumentException => "Warning",
                ValidationException => "Warning",
                DbUpdateException => "Critical",
                OutOfMemoryException => "Critical",
                StackOverflowException => "Critical",
                _ => "Error"
            };
        }

        private async Task<string?> GetRequestBodyAsync(HttpContext httpContext)
        {
            try
            {
                if (httpContext?.Request.Body != null && httpContext.Request.ContentLength > 0)
                {
                    httpContext.Request.EnableBuffering();
                    httpContext.Request.Body.Position = 0;
                    
                    using var reader = new StreamReader(httpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Position = 0;
                    
                    // Limit body size to prevent huge logs
                    return body.Length > 1000 ? body.Substring(0, 1000) + "..." : body;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private string? GetClientIpAddress(HttpContext? httpContext)
        {
            if (httpContext == null) return null;

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
    }

    // Custom validation exception
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
