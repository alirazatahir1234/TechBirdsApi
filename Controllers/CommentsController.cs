using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TechBirdsWebAPI.Data;
using TechBirdsWebAPI.Models;
using TechBirdsWebAPI.Services;

namespace TechBirdsWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserActivityService _userActivityService;
        private readonly IExceptionLoggerService _exceptionLogger;

        public CommentsController(
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

        [HttpGet("article/{articleId}")]
        public async Task<IActionResult> GetByArticleId(int articleId)
        {
            try
            {
                var comments = await _context.Comments
                    .Where(c => c.ArticleId == articleId && c.IsApproved)
                    .Include(c => c.User)
                    .OrderBy(c => c.CreatedAt)
                    .Select(c => new
                    {
                        c.Id,
                        c.ArticleId,
                        c.Content,
                        c.CreatedAt,
                        c.UpdatedAt,
                        c.IsApproved,
                        User = new
                        {
                            c.User!.Id,
                            c.User.Name,
                            c.User.Avatar,
                            c.User.Specialization
                        }
                    })
                    .ToListAsync();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Failed to retrieve comments for article");
                return StatusCode(500, new { message = "An error occurred while retrieving comments" });
            }
        }

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetByPostId(int postId)
        {
            try
            {
                var comments = await _context.Comments
                    .Where(c => c.PostId == postId && c.IsApproved)
                    .Include(c => c.User)
                    .OrderBy(c => c.CreatedAt)
                    .Select(c => new
                    {
                        c.Id,
                        c.PostId,
                        c.Content,
                        c.CreatedAt,
                        c.UpdatedAt,
                        c.IsApproved,
                        User = new
                        {
                            c.User!.Id,
                            c.User.Name,
                            c.User.Avatar,
                            c.User.Specialization
                        }
                    })
                    .ToListAsync();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Failed to retrieve comments for post");
                return StatusCode(500, new { message = "An error occurred while retrieving comments" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var comment = await _context.Comments
                    .Include(c => c.User)
                    .Where(c => c.Id == id)
                    .Select(c => new
                    {
                        c.Id,
                        c.ArticleId,
                        c.PostId,
                        c.Content,
                        c.CreatedAt,
                        c.UpdatedAt,
                        c.IsApproved,
                        User = new
                        {
                            c.User!.Id,
                            c.User.Name,
                            c.User.Avatar,
                            c.User.Specialization
                        }
                    })
                    .FirstOrDefaultAsync();

                if (comment == null) return NotFound(new { message = "Comment not found" });
                return Ok(comment);
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Failed to retrieve comment");
                return StatusCode(500, new { message = "An error occurred while retrieving the comment" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] CreateCommentRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Content))
                    return BadRequest(new { message = "Content is required" });

                if (request.Content.Length > 2000)
                    return BadRequest(new { message = "Content cannot exceed 2000 characters" });

                if (!request.ArticleId.HasValue && !request.PostId.HasValue)
                    return BadRequest(new { message = "Either ArticleId or PostId must be provided" });

                if (request.ArticleId.HasValue && request.PostId.HasValue)
                    return BadRequest(new { message = "Cannot comment on both article and post simultaneously" });

                // Get current user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized();

                // Verify article or post exists
          

                if (request.PostId.HasValue)
                {
                    var postExists = await _context.Posts.AnyAsync(p => p.Id == request.PostId.Value);
                    if (!postExists)
                        return NotFound(new { message = "Post not found" });
                }

                // Create comment
                var comment = new Comment
                {
                    UserId = userId,
                    ArticleId = request.ArticleId,
                    PostId = request.PostId,
                    Content = request.Content.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = true // Auto-approve for now, can be changed based on moderation settings
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                // Log activity
                await _userActivityService.LogActivityAsync(
                    userId,
                    "CommentCreated",
                    $"Created comment on {(request.ArticleId.HasValue ? "article" : "post")}",
                    new { commentId = comment.Id, articleId = request.ArticleId, postId = request.PostId }
                );

                // Return comment with user info
                var createdComment = await _context.Comments
                    .Include(c => c.User)
                    .Where(c => c.Id == comment.Id)
                    .Select(c => new
                    {
                        c.Id,
                        c.ArticleId,
                        c.PostId,
                        c.Content,
                        c.CreatedAt,
                        c.IsApproved,
                        User = new
                        {
                            c.User!.Id,
                            c.User.Name,
                            c.User.Avatar,
                            c.User.Specialization
                        }
                    })
                    .FirstAsync();

                return CreatedAtAction(nameof(Get), new { id = comment.Id }, createdComment);
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Failed to create comment");
                return StatusCode(500, new { message = "An error occurred while creating the comment" });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Content))
                    return BadRequest(new { message = "Content is required" });

                if (request.Content.Length > 2000)
                    return BadRequest(new { message = "Content cannot exceed 2000 characters" });

                // Get current user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Find comment
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                    return NotFound(new { message = "Comment not found" });

                // Check if user owns the comment or is admin/editor
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser == null)
                    return Unauthorized();

                var userRoles = await _userManager.GetRolesAsync(currentUser);
                var canEdit = comment.UserId == userId || 
                             userRoles.Any(r => r == "Admin" || r == "Editor" || r == "SuperAdmin");

                if (!canEdit)
                    return Forbid();

                // Update comment
                comment.Content = request.Content.Trim();
                comment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Log activity
                await _userActivityService.LogActivityAsync(
                    userId,
                    "CommentUpdated",
                    "Updated comment content",
                    new { commentId = id }
                );

                return Ok(new { message = "Comment updated successfully" });
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Failed to update comment");
                return StatusCode(500, new { message = "An error occurred while updating the comment" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get current user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Find comment
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                    return NotFound(new { message = "Comment not found" });

                // Check if user owns the comment or is admin/editor
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser == null)
                    return Unauthorized();

                var userRoles = await _userManager.GetRolesAsync(currentUser);
                var canDelete = comment.UserId == userId || 
                               userRoles.Any(r => r == "Admin" || r == "Editor" || r == "SuperAdmin");

                if (!canDelete)
                    return Forbid();

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();

                // Log activity
                await _userActivityService.LogActivityAsync(
                    userId,
                    "CommentDeleted",
                    "Deleted comment",
                    new { commentId = id, originalUserId = comment.UserId }
                );

                return Ok(new { message = "Comment deleted successfully" });
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Failed to delete comment");
                return StatusCode(500, new { message = "An error occurred while deleting the comment" });
            }
        }

        // Admin endpoints
        [HttpGet("admin/pending")]
        [Authorize(Roles = "Admin,Editor,SuperAdmin")]
        public async Task<IActionResult> GetPendingComments()
        {
            try
            {
                var pendingComments = await _context.Comments
                    .Where(c => !c.IsApproved)
                    .Include(c => c.User)
                    .Include(c => c.Post)
                    .OrderBy(c => c.CreatedAt)
                    .Select(c => new
                    {
                        c.Id,
                        c.ArticleId,
                        c.PostId,
                        c.Content,
                        c.CreatedAt,
                        c.IsApproved,
                        User = new
                        {
                            c.User!.Id,
                            c.User.Name,
                            c.User.Email,
                            c.User.Avatar
                        },
                        PostTitle = c.Post != null ? c.Post.Title : null
                    })
                    .ToListAsync();

                return Ok(pendingComments);
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Failed to retrieve pending comments");
                return StatusCode(500, new { message = "An error occurred while retrieving pending comments" });
            }
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin,Editor,SuperAdmin")]
        public async Task<IActionResult> ApproveComment(int id)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                    return NotFound(new { message = "Comment not found" });

                comment.IsApproved = true;
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _userActivityService.LogActivityAsync(
                    userId!,
                    "CommentApproved",
                    "Approved comment",
                    new { commentId = id }
                );

                return Ok(new { message = "Comment approved successfully" });
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Failed to approve comment");
                return StatusCode(500, new { message = "An error occurred while approving the comment" });
            }
        }
    }

    // DTOs for requests
    public class CreateCommentRequest
    {
        public int? ArticleId { get; set; }
        public int? PostId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateCommentRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}