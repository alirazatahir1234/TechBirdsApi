using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Slugify;
using TechBirdsApi.Data;
using TechBirdsApi.Models;

namespace TechBirdsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISlugHelper _slugger;

        public PagesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _slugger = new SlugHelper();
        }

        // DTOs
        public class PageCreateRequest
        {
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string? Excerpt { get; set; }
            public string? Status { get; set; } = "draft"; // draft|published|private
            public Guid? ParentId { get; set; }
            public int? MenuOrder { get; set; }
            public string? Template { get; set; }
            public Guid? FeaturedMediaId { get; set; }
            public string? SeoTitle { get; set; }
            public string? SeoDescription { get; set; }
            public string? MetaJson { get; set; }
            public string? Slug { get; set; } // optional, will be generated if empty
            public string? ChangeSummary { get; set; }
        }

        public class PageUpdateRequest : PageCreateRequest { }

        public class PageQuery
        {
            public int Page { get; set; } = 1;
            public int Limit { get; set; } = 20;
            public string? Search { get; set; }
            public string? Status { get; set; } // draft|published|private
            public Guid? ParentId { get; set; }
            public string? SortBy { get; set; } = "created"; // created|updated|title|menu
            public string? SortOrder { get; set; } = "desc"; // asc|desc
        }

        // Helpers
        private async Task<string> GenerateUniqueSlugAsync(string titleOrSlug, Guid? ignoreId = null)
        {
            var baseSlug = _slugger.GenerateSlug(titleOrSlug);
            if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = Guid.NewGuid().ToString("N").Substring(0, 8);
            var slug = baseSlug;
            int i = 1;
            while (await _db.Pages.AnyAsync(p => p.Slug == slug && (!ignoreId.HasValue || p.Id != ignoreId.Value)))
            {
                slug = $"{baseSlug}-{i++}";
            }
            return slug;
        }

        private static object ToDto(Page p) => new
        {
            id = p.Id,
            title = p.Title,
            slug = p.Slug,
            content = p.Content,
            excerpt = p.Excerpt,
            status = p.Status,
            publishedAt = p.PublishedAt,
            createdAt = p.CreatedAt,
            updatedAt = p.UpdatedAt,
            parentId = p.ParentId,
            menuOrder = p.MenuOrder,
            template = p.Template,
            authorId = p.AuthorId,
            featuredMediaId = p.FeaturedMediaId,
            seoTitle = p.SeoTitle,
            seoDescription = p.SeoDescription,
            metaJson = p.MetaJson
        };

        // Create page
        [HttpPost]
        [Authorize(Roles = "Author,Editor,Admin,SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] PageCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Title)) return BadRequest(new { message = "Title is required" });
            var userId = _userManager.GetUserId(User);
            if (userId is null) return Unauthorized();

            var slug = string.IsNullOrWhiteSpace(req.Slug) ? await GenerateUniqueSlugAsync(req.Title) : await GenerateUniqueSlugAsync(req.Slug);

            var now = DateTime.UtcNow;
            var page = new Page
            {
                Title = req.Title,
                Slug = slug,
                Content = req.Content ?? string.Empty,
                Excerpt = req.Excerpt,
                Status = (req.Status ?? "draft").ToLowerInvariant(),
                PublishedAt = req.Status?.ToLowerInvariant() == "published" ? now : null,
                ParentId = req.ParentId,
                MenuOrder = req.MenuOrder ?? 0,
                Template = req.Template,
                AuthorId = userId,
                FeaturedMediaId = req.FeaturedMediaId,
                SeoTitle = req.SeoTitle,
                SeoDescription = req.SeoDescription,
                MetaJson = req.MetaJson,
                CreatedAt = now
            };

            // Parent guard (optional)
            if (page.ParentId.HasValue && !await _db.Pages.AnyAsync(p => p.Id == page.ParentId.Value))
            {
                return BadRequest(new { message = "Parent page not found" });
            }

            // Create initial revision
            var rev = new PageRevision
            {
                Page = page,
                Version = 1,
                Title = page.Title,
                Content = page.Content,
                Excerpt = page.Excerpt,
                ChangeSummary = req.ChangeSummary ?? "Initial version",
                CreatedByUserId = userId,
                CreatedAt = now
            };

            _db.Pages.Add(page);
            _db.PageRevisions.Add(rev);
            await _db.SaveChangesAsync();

            return Ok(ToDto(page));
        }

        // Update page
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Author,Editor,Admin,SuperAdmin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PageUpdateRequest req)
        {
            var page = await _db.Pages.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (page == null) return NotFound(new { message = "Page not found" });

            var userId = _userManager.GetUserId(User);
            var roles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));
            bool isEditor = roles.Any(r => r is "Editor" or "Admin" or "SuperAdmin");
            if (!isEditor && page.AuthorId != userId) return Forbid();

            if (!string.IsNullOrWhiteSpace(req.Slug) && req.Slug != page.Slug)
            {
                page.Slug = await GenerateUniqueSlugAsync(req.Slug, id);
            }

            var now = DateTime.UtcNow;
            page.Title = req.Title ?? page.Title;
            page.Content = req.Content ?? page.Content;
            page.Excerpt = req.Excerpt ?? page.Excerpt;
            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                page.Status = req.Status.ToLowerInvariant();
                page.PublishedAt = page.Status == "published" && page.PublishedAt == null ? now : page.PublishedAt;
            }
            page.ParentId = req.ParentId ?? page.ParentId;
            page.MenuOrder = req.MenuOrder ?? page.MenuOrder;
            page.Template = req.Template ?? page.Template;
            page.FeaturedMediaId = req.FeaturedMediaId ?? page.FeaturedMediaId;
            page.SeoTitle = req.SeoTitle ?? page.SeoTitle;
            page.SeoDescription = req.SeoDescription ?? page.SeoDescription;
            page.MetaJson = req.MetaJson ?? page.MetaJson;
            page.UpdatedAt = now;

            // Create new revision
            var nextVersion = (await _db.PageRevisions.Where(r => r.PageId == id).MaxAsync(r => (int?)r.Version)) + 1 ?? 1;
            var rev = new PageRevision
            {
                PageId = id,
                Version = nextVersion,
                Title = page.Title,
                Content = page.Content,
                Excerpt = page.Excerpt,
                ChangeSummary = req.ChangeSummary ?? "Updated",
                CreatedByUserId = userId!,
                CreatedAt = now
            };

            _db.PageRevisions.Add(rev);
            await _db.SaveChangesAsync();

            return Ok(ToDto(page));
        }

        // Get page by id
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var page = await _db.Pages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (page == null) return NotFound(new { message = "Page not found" });
            return Ok(ToDto(page));
        }

        // Get page by slug (public)
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var isAuthed = User?.Identity != null && User.Identity.IsAuthenticated;
            var query = _db.Pages.AsNoTracking().Where(p => p.Slug == slug && !p.IsDeleted);
            if (!isAuthed)
            {
                query = query.Where(p => p.Status == "published");
            }
            var page = await query.FirstOrDefaultAsync();
            if (page == null) return NotFound(new { message = "Page not found" });
            return Ok(ToDto(page));
        }

        // List pages (admin/editor/author visibility)
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] PageQuery q)
        {
            var query = _db.Pages.AsNoTracking().Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                query = query.Where(p => p.Title.Contains(q.Search) || (p.Excerpt != null && p.Excerpt.Contains(q.Search)) || p.Content.Contains(q.Search));
            }
            if (!string.IsNullOrWhiteSpace(q.Status))
            {
                query = query.Where(p => p.Status == q.Status);
            }
            if (q.ParentId.HasValue)
            {
                query = query.Where(p => p.ParentId == q.ParentId);
            }

            query = q.SortBy?.ToLower() switch
            {
                "updated" => q.SortOrder == "asc" ? query.OrderBy(p => p.UpdatedAt) : query.OrderByDescending(p => p.UpdatedAt),
                "title" => q.SortOrder == "asc" ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title),
                "menu" => q.SortOrder == "asc" ? query.OrderBy(p => p.MenuOrder) : query.OrderByDescending(p => p.MenuOrder),
                _ => q.SortOrder == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
            };

            var total = await query.CountAsync();
            var items = await query.Skip((q.Page - 1) * q.Limit).Take(q.Limit).ToListAsync();

            return Ok(new
            {
                items = items.Select(ToDto),
                pagination = new { q.Page, q.Limit, total, totalPages = (int)Math.Ceiling(total / (double)q.Limit) }
            });
        }

        // Delete (soft)
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Editor,Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var page = await _db.Pages.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (page == null) return NotFound(new { message = "Page not found" });

            page.IsDeleted = true;
            page.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Page moved to trash" });
        }

        // Hard delete
        [HttpDelete("{id:guid}/hard")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> HardDelete(Guid id)
        {
            var page = await _db.Pages.Include(p => p.Revisions).FirstOrDefaultAsync(p => p.Id == id);
            if (page == null) return NotFound(new { message = "Page not found" });

            _db.PageRevisions.RemoveRange(page.Revisions);
            _db.Pages.Remove(page);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Page permanently deleted" });
        }

        // Revisions list
        [HttpGet("{id:guid}/revisions")]
        [Authorize(Roles = "Author,Editor,Admin,SuperAdmin")]
        public async Task<IActionResult> Revisions(Guid id)
        {
            var revs = await _db.PageRevisions.AsNoTracking()
                .Where(r => r.PageId == id)
                .OrderByDescending(r => r.Version)
                .ToListAsync();
            return Ok(revs.Select(r => new
            {
                id = r.Id,
                version = r.Version,
                title = r.Title,
                excerpt = r.Excerpt,
                createdAt = r.CreatedAt,
                createdByUserId = r.CreatedByUserId,
                changeSummary = r.ChangeSummary
            }));
        }

        // Restore a revision
        [HttpPost("{id:guid}/restore/{revisionId:guid}")]
        [Authorize(Roles = "Author,Editor,Admin,SuperAdmin")]
        public async Task<IActionResult> Restore(Guid id, Guid revisionId)
        {
            var page = await _db.Pages.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (page == null) return NotFound(new { message = "Page not found" });

            var rev = await _db.PageRevisions.AsNoTracking().FirstOrDefaultAsync(r => r.Id == revisionId && r.PageId == id);
            if (rev == null) return NotFound(new { message = "Revision not found" });

            var userId = _userManager.GetUserId(User);
            var now = DateTime.UtcNow;
            page.Title = rev.Title;
            page.Content = rev.Content;
            page.Excerpt = rev.Excerpt;
            page.UpdatedAt = now;

            var nextVersion = (await _db.PageRevisions.Where(r => r.PageId == id).MaxAsync(r => (int?)r.Version)) + 1 ?? 1;
            var newRev = new PageRevision
            {
                PageId = id,
                Version = nextVersion,
                Title = page.Title,
                Content = page.Content,
                Excerpt = page.Excerpt,
                ChangeSummary = $"Restore from v{rev.Version}",
                CreatedByUserId = userId!,
                CreatedAt = now
            };

            _db.PageRevisions.Add(newRev);
            await _db.SaveChangesAsync();
            return Ok(ToDto(page));
        }
    }
}
