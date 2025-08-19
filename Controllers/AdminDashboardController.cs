using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechBirdsApi.Data;

namespace TechBirdsApi.Controllers
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var usersCount = await _context.Users.CountAsync();
                var postsCount = await _context.Posts.CountAsync();
                var categoriesCount = await _context.Categories.CountAsync();
                var commentsCount = await _context.Comments.CountAsync();

                var stats = new
                {
                    totalUsers = usersCount,
                    totalPosts = postsCount,
                    totalCategories = categoriesCount,
                    totalComments = commentsCount,
                    totalViews = 0, // Can be calculated later
                    publishedPosts = postsCount, // Assuming all are published for now
                    draftPosts = 0
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving dashboard stats", error = ex.Message });
            }
        }

        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity()
        {
            try
            {
                // Get recent users
                var recentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .Select(u => new { 
                        type = "user", 
                        title = $"New user: {u.Name}",
                        date = (DateTime?)u.CreatedAt 
                    })
                    .ToListAsync();

                // Get recent articles if the table exists
                var recentArticles = await _context.Posts
                    .Where(a => a.PublishedAt.HasValue)
                    .OrderByDescending(a => a.PublishedAt)
                    .Take(5)
                    .Select(a => new { 
                        type = "post", 
                        title = $"New post: {a.Title}",
                        date = a.PublishedAt 
                    })
                    .ToListAsync();

                var activity = recentUsers.Concat(recentArticles)
                    .Where(a => a.date.HasValue)
                    .OrderByDescending(a => a.date)
                    .Take(10)
                    .ToList();

                return Ok(activity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving recent activity", error = ex.Message });
            }
        }
    }
}
