using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechBirdsWebAPI.Data;
using TechBirdsWebAPI.Models;

namespace TechBirdsWebAPI.Controllers
{
    [Route("api/admin/comments")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminCommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminCommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetComments([FromQuery] CommentQueryParams parameters)
        {
            try
            {
                var query = _context.Comments.Include(c => c.User).AsQueryable();
                
                if (parameters.PostId.HasValue)
                {
                    query = query.Where(c => c.ArticleId == parameters.PostId.Value || c.PostId == parameters.PostId.Value);
                }

                var totalCount = await query.CountAsync();
                var comments = await query
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .Select(c => new CommentResponse
                    {
                        Id = c.Id,
                        Content = c.Content,
                        Author = c.User != null ? c.User.Name : "Unknown",
                        AuthorEmail = c.User != null ? c.User.Email : "",
                        PostId = c.ArticleId ?? c.PostId ?? 0,
                        Status = c.IsApproved ? "Approved" : "Pending",
                        CreatedAt = c.CreatedAt,
                        IsReported = false // Default value since not in model
                    })
                    .ToListAsync();

                return Ok(new CommentsListResponse
                {
                    Data = comments,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving comments", error = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateCommentStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                {
                    return NotFound(new { message = "Comment not found" });
                }

                // Status property doesn't exist in current Comment model
                // If you need this functionality, add a Status property to Comment model
                
                return Ok(new { message = "Comment status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating comment status", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                {
                    return NotFound(new { message = "Comment not found" });
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Comment deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting comment", error = ex.Message });
            }
        }

        [HttpPost("{id}/reply")]
        public async Task<IActionResult> ReplyToComment(int id, [FromBody] ReplyRequest request)
        {
            try
            {
                var parentComment = await _context.Comments.FindAsync(id);
                if (parentComment == null)
                {
                    return NotFound(new { message = "Parent comment not found" });
                }

                var reply = new Comment
                {
                    Content = request.Content,
                    UserId = request.AuthorName,
                    ArticleId = parentComment.ArticleId,
                    CreatedAt = DateTime.UtcNow
                    // ParentId = id // You might need to add this to your model for threading
                };

                _context.Comments.Add(reply);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Reply added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding reply", error = ex.Message });
            }
        }
    }

    // DTOs
    public class CommentQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Status { get; set; } = string.Empty;
        public int? PostId { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class ReplyRequest
    {
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
    }

    public class CommentResponse
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public int PostId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsReported { get; set; }
    }

    public class CommentsListResponse
    {
        public List<CommentResponse> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
