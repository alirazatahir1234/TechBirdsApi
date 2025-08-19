using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechBirdsApi.Data;
using TechBirdsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace TechBirdsApi.Controllers
{
    [ApiController]
    [Route("api/admin/posts")]
    [Authorize(Roles = "Admin,SuperAdmin,Editor,Author")]
    public class AdminPostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminPostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/admin/posts
        [HttpGet]
        public async Task<ActionResult<AdminPostsListResponse>> GetPosts([FromQuery] PostQueryParams queryParams)
        {
            try
            {
                var query = _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(queryParams.Status))
                {
                    query = query.Where(p => p.Status == queryParams.Status);
                }

                if (!string.IsNullOrEmpty(queryParams.Type))
                {
                    query = query.Where(p => p.Type == queryParams.Type);
                }

                if (queryParams.Featured.HasValue)
                {
                    query = query.Where(p => p.Featured == queryParams.Featured.Value);
                }

                if (!string.IsNullOrEmpty(queryParams.UserId))
                {
                    query = query.Where(p => p.UserId == queryParams.UserId);
                }

                if (queryParams.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == queryParams.CategoryId.Value);
                }

                if (!string.IsNullOrEmpty(queryParams.Search))
                {
                    query = query.Where(p => p.Title.Contains(queryParams.Search) || 
                                           p.Content.Contains(queryParams.Search) || 
                                           (p.Summary != null && p.Summary.Contains(queryParams.Search)));
                }

                if (!string.IsNullOrEmpty(queryParams.Tags))
                {
                    query = query.Where(p => p.Tags != null && p.Tags.Contains(queryParams.Tags));
                }

                if (queryParams.DateFrom.HasValue)
                {
                    query = query.Where(p => p.CreatedAt >= queryParams.DateFrom.Value);
                }

                if (queryParams.DateTo.HasValue)
                {
                    query = query.Where(p => p.CreatedAt <= queryParams.DateTo.Value);
                }

                // Apply sorting
                query = queryParams.SortBy?.ToLower() switch
                {
                    "title" => queryParams.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(p => p.Title)
                        : query.OrderBy(p => p.Title),
                    "createdat" => queryParams.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(p => p.CreatedAt)
                        : query.OrderBy(p => p.CreatedAt),
                    "publishedat" => queryParams.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(p => p.PublishedAt)
                        : query.OrderBy(p => p.PublishedAt),
                    "viewcount" => queryParams.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(p => p.ViewCount)
                        : query.OrderBy(p => p.ViewCount),
                    _ => query.OrderByDescending(p => p.CreatedAt) // Default sort
                };

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var posts = await query
                    .Skip((queryParams.Page - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize)
                    .Select(p => new AdminPostResponse
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Content = p.Content,
                        Summary = p.Summary,
                        ImageUrl = p.ImageUrl,
                        UserId = p.UserId,
                        UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}".Trim() : "",
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        Tags = p.Tags,
                        PublishedAt = p.PublishedAt,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        Type = p.Type,
                        Status = p.Status,
                        Featured = p.Featured,
                        AllowComments = p.AllowComments,
                        ViewCount = p.ViewCount,
                        LikeCount = p.LikeCount,
                        ShareCount = p.ShareCount,
                        ExternalUrl = p.ExternalUrl,
                        ExternalSource = p.ExternalSource
                    })
                    .ToListAsync();

                var response = new AdminPostsListResponse
                {
                    Posts = posts,
                    TotalCount = totalCount,
                    Page = queryParams.Page,
                    PageSize = queryParams.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / queryParams.PageSize),
                    Filters = new FilterSummary
                    {
                        Status = queryParams.Status,
                        Type = queryParams.Type,
                        Featured = queryParams.Featured,
                        UserId = queryParams.UserId,
                        CategoryId = queryParams.CategoryId
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving posts.", error = ex.Message });
            }
        }

        // POST: api/admin/posts
        [HttpPost]
        public async Task<ActionResult<AdminPostResponse>> CreatePost([FromBody] PostRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get current user ID from JWT token
                var currentUserId = _userManager.GetUserId(User);
                if (currentUserId == null)
                {
                    return Unauthorized();
                }
                
                // Verify that the user exists and is active
                var user = await _userManager.FindByIdAsync(currentUserId);
                if (user == null)
                {
                    return BadRequest(new { message = $"User with ID {currentUserId} not found." });
                }
                
                // Check if user is locked out (inactive)
                var isLockedOut = await _userManager.IsLockedOutAsync(user);
                if (isLockedOut)
                {
                    return BadRequest(new { message = $"User with ID {currentUserId} is not active." });
                }

                var post = new Post
                {
                    Title = request.Title,
                    Content = request.Content,
                    Summary = request.Summary,
                    ImageUrl = request.ImageUrl,
                    UserId = currentUserId, // Use validated user ID
                    CategoryId = request.CategoryId,
                    Tags = request.Tags,
                    Type = request.Type ?? "update",
                    Status = request.Status ?? "draft",
                    Featured = request.Featured,
                    AllowComments = request.AllowComments,
                    ExternalUrl = request.ExternalUrl,
                    ExternalSource = request.ExternalSource,
                    CreatedAt = DateTime.UtcNow,
                    PublishedAt = request.Status == "published" ? DateTime.UtcNow : null
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                // Fetch the created post with related data
                var createdPost = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Where(p => p.Id == post.Id)
                    .Select(p => new AdminPostResponse
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Content = p.Content,
                        Summary = p.Summary,
                        ImageUrl = p.ImageUrl,
                        UserId = p.UserId,
                        UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}".Trim() : "",
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        Tags = p.Tags,
                        PublishedAt = p.PublishedAt,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        Type = p.Type,
                        Status = p.Status,
                        Featured = p.Featured,
                        AllowComments = p.AllowComments,
                        ViewCount = p.ViewCount,
                        LikeCount = p.LikeCount,
                        ShareCount = p.ShareCount,
                        ExternalUrl = p.ExternalUrl,
                        ExternalSource = p.ExternalSource
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, createdPost);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the post.", error = ex.Message });
            }
        }

        // GET: api/admin/posts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminPostResponse>> GetPost(int id)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Where(p => p.Id == id)
                    .Select(p => new AdminPostResponse
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Content = p.Content,
                        Summary = p.Summary,
                        ImageUrl = p.ImageUrl,
                        UserId = p.UserId,
                        UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}".Trim() : "",
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        Tags = p.Tags,
                        PublishedAt = p.PublishedAt,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        Type = p.Type,
                        Status = p.Status,
                        Featured = p.Featured,
                        AllowComments = p.AllowComments,
                        ViewCount = p.ViewCount,
                        LikeCount = p.LikeCount,
                        ShareCount = p.ShareCount,
                        ExternalUrl = p.ExternalUrl,
                        ExternalSource = p.ExternalSource
                    })
                    .FirstOrDefaultAsync();

                if (post == null)
                {
                    return NotFound(new { message = "Post not found." });
                }

                return Ok(post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the post.", error = ex.Message });
            }
        }

        // PUT: api/admin/posts/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<AdminPostResponse>> UpdatePost(int id, [FromBody] PostRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    return NotFound(new { message = "Post not found." });
                }

                // Get current user ID from JWT token
                var currentUserId = _userManager.GetUserId(User);
                if (currentUserId == null)
                {
                    return Unauthorized();
                }
                
                // Verify that the user exists and is active
                var user = await _userManager.FindByIdAsync(currentUserId);
                if (user == null)
                {
                    return BadRequest(new { message = $"User with ID {currentUserId} not found." });
                }
                
                // Check if user is locked out (inactive)
                var isLockedOut = await _userManager.IsLockedOutAsync(user);
                if (isLockedOut)
                {
                    return BadRequest(new { message = $"User with ID {currentUserId} is not active." });
                }
                
                // Update post properties (only after validation passes)
                post.Title = request.Title;
                post.Content = request.Content;
                post.Summary = request.Summary;
                post.ImageUrl = request.ImageUrl;
                post.UserId = currentUserId; // Use validated user ID
                post.CategoryId = request.CategoryId;
                post.Tags = request.Tags;
                post.Type = request.Type ?? post.Type;
                post.Status = request.Status ?? post.Status;
                post.Featured = request.Featured;
                post.AllowComments = request.AllowComments;
                post.ExternalUrl = request.ExternalUrl;
                post.ExternalSource = request.ExternalSource;
                post.UpdatedAt = DateTime.UtcNow;

                // Update PublishedAt if status changed to published
                if (request.Status == "published" && post.PublishedAt == null)
                {
                    post.PublishedAt = DateTime.UtcNow;
                }
                else if (request.Status != "published")
                {
                    post.PublishedAt = null;
                }

                _context.Posts.Update(post);
                await _context.SaveChangesAsync();

                // Fetch the updated post with related data
                var updatedPost = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Where(p => p.Id == id)
                    .Select(p => new AdminPostResponse
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Content = p.Content,
                        Summary = p.Summary,
                        ImageUrl = p.ImageUrl,
                        UserId = p.UserId,
                        UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}".Trim() : "",
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        Tags = p.Tags,
                        PublishedAt = p.PublishedAt,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        Type = p.Type,
                        Status = p.Status,
                        Featured = p.Featured,
                        AllowComments = p.AllowComments,
                        ViewCount = p.ViewCount,
                        LikeCount = p.LikeCount,
                        ShareCount = p.ShareCount,
                        ExternalUrl = p.ExternalUrl,
                        ExternalSource = p.ExternalSource
                    })
                    .FirstOrDefaultAsync();

                return Ok(updatedPost);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the post.", error = ex.Message });
            }
        }

        // DELETE: api/admin/posts/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePost(int id)
        {
            try
            {
                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    return NotFound(new { message = "Post not found." });
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Post deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the post.", error = ex.Message });
            }
        }
    }

    // DTOs for Admin Posts
    public class PostQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; }
        public string? Type { get; set; }
        public bool? Featured { get; set; }
        public string? UserId { get; set; } // Changed from AuthorId to UserId
        public int? CategoryId { get; set; }
        public string? Search { get; set; }
        public string? Tags { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }

    public class PostRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public string? Tags { get; set; }
        public string? Type { get; set; } = "update";
        public string? Status { get; set; } = "draft";
        public bool Featured { get; set; } = false;
        public bool AllowComments { get; set; } = true;
        public string? ExternalUrl { get; set; }
        public string? ExternalSource { get; set; }
    }

    public class AdminPostResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? ImageUrl { get; set; }
        public string UserId { get; set; } = string.Empty; // Changed from AuthorId to UserId
        public string UserName { get; set; } = string.Empty; // Changed from AuthorName to UserName
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Tags { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool Featured { get; set; }
        public bool AllowComments { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int ShareCount { get; set; }
        public string? ExternalUrl { get; set; }
        public string? ExternalSource { get; set; }
    }

    public class AdminPostsListResponse
    {
        public List<AdminPostResponse> Posts { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public FilterSummary Filters { get; set; } = new();
    }

    public class FilterSummary
    {
        public string? Status { get; set; }
        public string? Type { get; set; }
        public bool? Featured { get; set; }
        public string? UserId { get; set; } // Changed from AuthorId to UserId
        public int? CategoryId { get; set; }
    }
}
