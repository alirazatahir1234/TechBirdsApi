using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Slugify;
using TechBirdsApi.Data;
using TechBirdsApi.Dto;
using TechBirdsApi.Models;
using TechBirdsApi.Repositories;
using TechBirdsApi.Services;

namespace TechBirdsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagesController : ControllerBase
    {
        private readonly IPageService _pageService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISlugHelper _slugger;

        public PagesController(IPageService pageService, UserManager<ApplicationUser> userManager)
        {
            _pageService = pageService;
            _userManager = userManager;
            _slugger = new SlugHelper();
        }

        // DTOs
        
        public class PageUpdateRequest : PageCreateRequest { }

      

        // Helpers
        private async Task<string> GenerateUniqueSlugAsync(string titleOrSlug, Guid? ignoreId = null)
        {
            var baseSlug = _slugger.GenerateSlug(titleOrSlug);
            if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = Guid.NewGuid().ToString("N").Substring(0, 8);
            var slug = baseSlug;
            int i = 1;
            while (await _pageService.GetBySlugAsync(slug) is not null && (!ignoreId.HasValue || (await _pageService.GetBySlugAsync(slug))?.Id != ignoreId.Value))
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

            await _pageService.AddAsync(page);
            return Ok(ToDto(page));
        }

        // Update page
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Author,Editor,Admin,SuperAdmin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PageUpdateRequest req)
        {
            var page = await _pageService.GetByIdAsync(id);
            if (page == null) return NotFound(new { message = "Page not found" });

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var roles = await _userManager.GetRolesAsync(user);
            bool isEditor = roles.Any(r => r is "Editor" or "Admin" or "SuperAdmin");
            if (!isEditor && page.AuthorId != userId) return Forbid();

            if (!string.IsNullOrWhiteSpace(req.Slug) && req.Slug != page.Slug)
            {
                page.Slug = _slugger.GenerateSlug(req.Slug);
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

            await _pageService.UpdateAsync(page);
            return Ok(ToDto(page));
        }

        // Get page by id
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var page = await _pageService.GetByIdAsync(id);
            if (page == null) return NotFound(new { message = "Page not found" });
            return Ok(ToDto(page));
        }

        // Get page by slug (public)
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var isAuthed = User?.Identity != null && User.Identity.IsAuthenticated;
            var page = await _pageService.GetBySlugAsync(slug, onlyPublished: !isAuthed);
            if (page == null) return NotFound(new { message = "Page not found" });
            return Ok(ToDto(page));
        }

        // List pages (admin/editor/author visibility)
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] PageQuery q)
        {
                var items = await _pageService.FilterAsync(q);
            var total = await _pageService.CountAsync(q.Search, q.Status, q.ParentId);
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
            var page = await _pageService.GetByIdAsync(id);
            if (page == null) return NotFound(new { message = "Page not found" });

            page.IsDeleted = true;
            page.DeletedAt = DateTime.UtcNow;
            await _pageService.UpdateAsync(page);
            return Ok(new { message = "Page moved to trash" });
        }

        // Hard delete
        [HttpDelete("{id:guid}/hard")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> HardDelete(Guid id)
        {
            var page = await _pageService.GetByIdAsync(id);
            if (page == null) return NotFound(new { message = "Page not found" });
            await _pageService.DeleteAsync(id);
            return Ok(new { message = "Page permanently deleted" });
        }

        // Revisions list
        [HttpGet("{id:guid}/revisions")]
        [Authorize(Roles = "Author,Editor,Admin,SuperAdmin")]
        public async Task<IActionResult> Revisions(Guid id)
        {
            var revs = await _pageService.GetRevisionsAsync(id);
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
            var page = await _pageService.GetByIdAsync(id);
            if (page == null) return NotFound(new { message = "Page not found" });

            var rev = await _pageService.GetRevisionAsync(revisionId);
            if (rev == null || rev.PageId != id) return NotFound(new { message = "Revision not found" });

            var userId = _userManager.GetUserId(User);
            var now = DateTime.UtcNow;
            page.Title = rev.Title;
            page.Content = rev.Content;
            page.Excerpt = rev.Excerpt;
            page.UpdatedAt = now;

            var nextVersion = await _pageService.GetNextRevisionVersionAsync(id);
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

            await _pageService.AddRevisionAsync(newRev);
            await _pageService.UpdateAsync(page);
            return Ok(ToDto(page));
        }
    }
}
