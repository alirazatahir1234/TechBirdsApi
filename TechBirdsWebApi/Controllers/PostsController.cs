using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechBirdsWebAPI.Data;
using TechBirdsWebAPI.Models;

namespace TechBirdsWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/posts
        [HttpGet]
        public async Task<ActionResult<PostsListResponse>> GetPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? type = null, // update, announcement, quick, social
            [FromQuery] string? status = "published",
            [FromQuery] bool? featured = null,
            [FromQuery] int? UserId = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? tags = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(p => p.Status == status);
                }

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(p => p.Type == type);
                }

                if (featured.HasValue)
                {
                    query = query.Where(p => p.Featured == featured.Value);
                }

                if (UserId.HasValue)
                {
                    query = query.Where(p => p.UserId == UserId.Value.ToString());
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Title.Contains(search) || p.Content.Contains(search) || (p.Summary != null && p.Summary.Contains(search)));
                }

                if (!string.IsNullOrEmpty(tags))
                {
                    query = query.Where(p => p.Tags != null && p.Tags.Contains(tags));
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination and ordering (latest first for posts)
                var posts = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PostResponse
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Content = p.Content,
                        Summary = p.Summary,
                        ImageUrl = p.ImageUrl,
                        UserId = p.UserId,
                        UserName = p.User.UserName != null ? $"{p.User.FirstName} {p.User.LastName}".Trim() : "",
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

                var response = new PostsListResponse
                {
                    Posts = posts,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving posts.", error = ex.Message });
            }
        }

        // GET: api/posts/featured
        [HttpGet("featured")]
        public async Task<ActionResult<IEnumerable<PostResponse>>> GetFeaturedPosts([FromQuery] int limit = 5)
        {
            try
            {
                var posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Where(p => p.Status == "published" && p.Featured)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(limit)
                    .Select(p => new PostResponse
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

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving featured posts.", error = ex.Message });
            }
        }

        // GET: api/posts/recent
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<PostResponse>>> GetRecentPosts([FromQuery] int limit = 10)
        {
            try
            {
                var posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Where(p => p.Status == "published")
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(limit)
                    .Select(p => new PostResponse
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

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving recent posts.", error = ex.Message });
            }
        }

        // GET: api/posts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PostResponse>> GetPost(int id)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Where(p => p.Id == id)
                    .Select(p => new PostResponse
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Content = p.Content,
                        Summary = p.Summary,
                        ImageUrl = p.ImageUrl,
                        UserId = p.UserId,
                        UserName = p.User.UserName != null ? $"{p.User.FirstName} {p.User.LastName}".Trim() : "",
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

                // Increment view count for published posts
                if (post.Status == "published")
                {
                    var postEntity = await _context.Posts.FindAsync(id);
                    if (postEntity != null)
                    {
                        postEntity.ViewCount++;
                        await _context.SaveChangesAsync();
                        post.ViewCount = postEntity.ViewCount;
                    }
                }

                return Ok(post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the post.", error = ex.Message });
            }
        }

        // POST: api/posts/{id}/like
        [HttpPost("{id}/like")]
        public async Task<ActionResult> LikePost(int id)
        {
            try
            {
                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    return NotFound(new { message = "Post not found." });
                }

                post.LikeCount++;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Post liked successfully.", likeCount = post.LikeCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while liking the post.", error = ex.Message });
            }
        }

        // POST: api/posts/{id}/share
        [HttpPost("{id}/share")]
        public async Task<ActionResult> SharePost(int id)
        {
            try
            {
                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    return NotFound(new { message = "Post not found." });
                }

                post.ShareCount++;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Post shared successfully.", shareCount = post.ShareCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while sharing the post.", error = ex.Message });
            }
        }

        // POST: api/posts (for creating draft posts)
        [HttpPost]
        public async Task<ActionResult<PostResponse>> CreatePost([FromBody] PostsPostRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var post = new Post
                {
                    Title = request.Title,
                    Content = request.Content,
                    Summary = request.Summary,
                    ImageUrl = request.ImageUrl,
                    UserId = request.UserId,
                    CategoryId = request.CategoryId,
                    Tags = request.Tags,
                    Type = request.Type ?? "update",
                    Status = request.Status ?? "draft", // Default to draft
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
                    .Select(p => new PostResponse
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
    }

    // Response DTOs
    public class PostResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? ImageUrl { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
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

    public class PostsListResponse
    {
        public List<PostResponse> Posts { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PostsPostRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? ImageUrl { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string? Tags { get; set; }
        public string? Type { get; set; } = "update";
        public string? Status { get; set; } = "draft";
        public bool Featured { get; set; } = false;
        public bool AllowComments { get; set; } = true;
        public string? ExternalUrl { get; set; }
        public string? ExternalSource { get; set; }
    }
}
