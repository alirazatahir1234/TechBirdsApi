using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechBirdsApi.Data;
using TechBirdsApi.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Identity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using TechBirdsApi.Repositories;
using TechBirdsApi.Services;

namespace TechBirdsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
    private readonly IMediaService _mediaService;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly HashSet<string> AllowedImageTypes = new(new[]
        {
            "image/jpeg", "image/png", "image/gif", "image/webp", "image/svg+xml"
        });
        private static readonly HashSet<string> AllowedDocTypes = new(new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            // Archives
            "application/zip",
            "application/x-zip-compressed"
        });
        private static readonly HashSet<string> AllowedVideoTypes = new(new[]
        {
            "video/mp4", "video/webm", "video/ogg", "video/x-matroska", "video/quicktime"
        });
        private static readonly HashSet<string> AllowedOctetExtensions = new(new[] { ".zip", ".mp4", ".webm", ".ogg", ".mkv", ".mov", ".pdf" });

        public MediaController(IMediaService mediaService, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
        {
            _mediaService = mediaService;
            _env = env;
            _userManager = userManager;
        }
        
    [HttpPost]
    // [Authorize(Roles = "Author,Editor,Admin,SuperAdmin")]
        public async Task<IActionResult> Upload([FromForm] UploadMediaRequest request)
        {
            var file = request.FileUpload;
            if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded" });

            var contentType = file.ContentType?.ToLowerInvariant() ?? "";
            var originalExt = Path.GetExtension(file.FileName).ToLowerInvariant();
            bool allowed = AllowedImageTypes.Contains(contentType)
                           || AllowedDocTypes.Contains(contentType)
                           || AllowedVideoTypes.Contains(contentType)
                           || (contentType == "application/octet-stream" && AllowedOctetExtensions.Contains(originalExt));
            if (!allowed)
            {
                return BadRequest(new { message = $"Unsupported file type: {contentType}" });
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var now = DateTime.UtcNow;
            var relDir = $"uploads/{now:yyyy}/{now:MM}";
            var absDir = Path.Combine(_env.ContentRootPath, relDir);
            Directory.CreateDirectory(absDir);

            var ext = Path.GetExtension(file.FileName);
            var uniqueName = $"{Guid.NewGuid():N}{ext}";
            var absPath = Path.Combine(absDir, uniqueName);
            await using (var stream = System.IO.File.Create(absPath))
            {
                await file.CopyToAsync(stream);
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(uniqueName, out var mime)) mime = contentType;

            string urlBase = $"/{relDir.Replace("\\", "/")}/{uniqueName}";

            var media = new MediaItem
            {
                FileName = uniqueName,
                OriginalFileName = file.FileName,
                MimeType = mime ?? contentType,
                Size = file.Length,
                Url = urlBase,
                StoragePath = absPath,
                Title = request.Title ?? Path.GetFileNameWithoutExtension(file.FileName),
                AltText = request.AltText,
                Caption = request.Caption,
                Description = request.Description,
                UploadedByUserId = userId,
            };
            // Generate thumbnail for images
            if (mime != null && mime.StartsWith("image/") && !mime.Contains("svg"))
            {
                var thumbName = $"thumb_{Path.GetFileNameWithoutExtension(uniqueName)}.jpg";
                var thumbAbsPath = Path.Combine(absDir, thumbName);
                var thumbUrl = $"/{relDir.Replace("\\", "/")}/{thumbName}";
                try
                {
                    using var image = await Image.LoadAsync(absPath);
                    var maxWidth = 400;
                    var maxHeight = 400;
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Sampler = KnownResamplers.Bicubic,
                        Size = new Size(maxWidth, maxHeight)
                    }));
                    await image.SaveAsJpegAsync(thumbAbsPath, new JpegEncoder { Quality = 80 });
                    media.ThumbnailPath = thumbAbsPath;
                    media.ThumbnailUrl = thumbUrl;
                    media.Width = image.Width;
                    media.Height = image.Height;
                }
                catch
                {
                    // if thumbnail generation fails, continue without blocking upload
                }
            }

            await _mediaService.AddAsync(media);
            return Ok(ToDto(media));
        }

        // GET: api/media - List media with filters and pagination
        [HttpGet]
        [Authorize(Roles = "Author,Editor,Admin,SuperAdmin")] // Adjust as needed
        public async Task<IActionResult> List([FromQuery] MediaQuery query)
        {
            var items = await _mediaService.GetAllAsync();
            var filtered = items.Where(m => !m.IsDeleted);
            if (!string.IsNullOrWhiteSpace(query.Search))
                filtered = filtered.Where(m => m.Title.Contains(query.Search) || (m.Description != null && m.Description.Contains(query.Search)) || m.OriginalFileName.Contains(query.Search));
            if (!string.IsNullOrWhiteSpace(query.MimeType))
                filtered = filtered.Where(m => m.MimeType.StartsWith(query.MimeType));
            if (!string.IsNullOrWhiteSpace(query.UploadedByUserId))
                filtered = filtered.Where(m => m.UploadedByUserId == query.UploadedByUserId);
            filtered = query.SortBy?.ToLower() switch
            {
                "size" => query.SortOrder == "asc" ? filtered.OrderBy(m => m.Size) : filtered.OrderByDescending(m => m.Size),
                "title" => query.SortOrder == "asc" ? filtered.OrderBy(m => m.Title) : filtered.OrderByDescending(m => m.Title),
                _ => query.SortOrder == "asc" ? filtered.OrderBy(m => m.CreatedAt) : filtered.OrderByDescending(m => m.CreatedAt)
            };
            var total = filtered.Count();
            var paged = filtered.Skip((query.Page - 1) * query.Limit).Take(query.Limit).ToList();
            return Ok(new
            {
                items = paged.Select(ToDto),
                pagination = new { query.Page, query.Limit, total, totalPages = (int)Math.Ceiling(total / (double)query.Limit) }
            });
        }

        // GET: api/media/{id}
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Author,Editor,Admin,SuperAdmin")]
        public async Task<IActionResult> Get(Guid id)
        {
            var media = await _mediaService.GetByIdAsync(id);
            if (media == null) return NotFound(new { message = "Media not found" });
            return Ok(ToDto(media));
        }

        // GET: api/media/{id}/file - stream original file (supports video/pdf preview)
        [HttpGet("{id:guid}/file")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFile(Guid id)
        {
            var media = await _mediaService.GetByIdAsync(id);
            if (media == null || string.IsNullOrWhiteSpace(media.StoragePath) || !System.IO.File.Exists(media.StoragePath))
            {
                return NotFound();
            }

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{media.OriginalFileName}\"";
            return PhysicalFile(media.StoragePath, media.MimeType, enableRangeProcessing: true);
        }

        // GET: api/media/{id}/thumbnail - returns thumbnail image or placeholder
        [HttpGet("{id:guid}/thumbnail")]
        [AllowAnonymous] // previews can be public; adjust if needed
        public async Task<IActionResult> Thumbnail(Guid id)
        {
            var media = await _mediaService.GetByIdAsync(id);
            if (media == null) return NotFound();

            // If we already have a generated thumbnail, serve it
            if (!string.IsNullOrWhiteSpace(media.ThumbnailPath) && System.IO.File.Exists(media.ThumbnailPath))
            {
                return PhysicalFile(media.ThumbnailPath, "image/jpeg");
            }

            // If it's an image, try to generate on the fly
            if (media.MimeType.StartsWith("image/") && !media.MimeType.Contains("svg") && System.IO.File.Exists(media.StoragePath))
            {
                try
                {
                    using var image = await Image.LoadAsync(media.StoragePath);
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Sampler = KnownResamplers.Bicubic,
                        Size = new Size(400, 400)
                    }));
                    using var outStream = new MemoryStream();
                    await image.SaveAsJpegAsync(outStream, new JpegEncoder { Quality = 80 });
                    outStream.Position = 0;
                    return File(outStream.ToArray(), "image/jpeg");
                }
                catch { }
            }

            // For videos, pdfs, zips, etc., return a simple SVG placeholder
            string svg = "<svg xmlns='http://www.w3.org/2000/svg' width='200' height='150'><rect width='100%' height='100%' fill='#f1f5f9'/><text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='#475569' font-family='Arial' font-size='16'>Preview Unavailable</text></svg>";
            return Content(svg, "image/svg+xml");
        }

        // PUT: api/media/{id} (update metadata only)
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Author,Editor,Admin,SuperAdmin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMediaRequest req)
        {
            var media = await _mediaService.GetByIdAsync(id);
            if (media == null) return NotFound(new { message = "Media not found" });

            // Optional: author can edit only their items; admins/editors can edit any
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
            bool isAdmin = roles.Any(r => r is "Admin" or "SuperAdmin" or "Editor");
            if (!isAdmin && media.UploadedByUserId != userId)
            {
                return Forbid();
            }

            media.Title = req.Title ?? media.Title;
            media.AltText = req.AltText ?? media.AltText;
            media.Caption = req.Caption ?? media.Caption;
            media.Description = req.Description ?? media.Description;
            media.UpdatedAt = DateTime.UtcNow;

            await _mediaService.UpdateAsync(media);
            return Ok(ToDto(media));
        }

        // DELETE: api/media/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Author,Editor,Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var media = await _mediaService.GetByIdAsync(id);
            if (media == null) return NotFound(new { message = "Media not found" });

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
            bool isAdmin = roles.Any(r => r is "Admin" or "SuperAdmin" or "Editor");
            if (!isAdmin && media.UploadedByUserId != userId)
            {
                return Forbid();
            }

            // Soft delete like WordPress (don't remove file immediately)
            media.IsDeleted = true;
            media.DeletedAt = DateTime.UtcNow;
            await _mediaService.UpdateAsync(media);

            return Ok(new { message = "Media moved to trash" });
        }

        // DELETE: api/media/{id}/hard - permanently delete and remove file
        [HttpDelete("{id:guid}/hard")]
        [Authorize(Roles = "Admin,SuperAdmin,Editor")]
        public async Task<IActionResult> HardDelete(Guid id)
        {
            var media = await _mediaService.GetByIdAsync(id);
            if (media == null) return NotFound(new { message = "Media not found" });

            await _mediaService.DeleteAsync(id);

            try
            {
                if (System.IO.File.Exists(media.StoragePath)) System.IO.File.Delete(media.StoragePath);
                if (!string.IsNullOrWhiteSpace(media.ThumbnailPath) && System.IO.File.Exists(media.ThumbnailPath)) System.IO.File.Delete(media.ThumbnailPath!);
            }
            catch
            {
                // swallow IO exceptions to not block API
            }

            return Ok(new { message = "Media permanently deleted" });
        }

        private static object ToDto(MediaItem m) => new
        {
            id = m.Id,
            url = m.Url,
            thumbnailUrl = m.ThumbnailUrl,
            fileName = m.FileName,
            originalFileName = m.OriginalFileName,
            mimeType = m.MimeType,
            size = m.Size,
            width = m.Width,
            height = m.Height,
            title = m.Title,
            altText = m.AltText,
            caption = m.Caption,
            description = m.Description,
            uploadedByUserId = m.UploadedByUserId,
            createdAt = m.CreatedAt,
            updatedAt = m.UpdatedAt,
            isDeleted = m.IsDeleted,
            deletedAt = m.DeletedAt
        };
    }
}

        public class MediaQuery
{
    private int _page = 1;
    private int _limit = 20;
    public int Page { get => _page; set => _page = value > 0 ? value : 1; }
    public int Limit { get => _limit; set => _limit = value > 0 && value <= 100 ? value : 20; }
    public string? Search { get; set; }
    public string? MimeType { get; set; } // filter by category like image/*
    public string? UploadedByUserId { get; set; }
    public string? SortBy { get; set; } = "created"; // created|title|size
    public string? SortOrder { get; set; } = "desc"; // asc|desc
}

        public class UploadMediaRequest
        {
            [FromForm]
            public IFormFile FileUpload { get; set; } = default!;
            [FromForm]
            public string? Title { get; set; }
            [FromForm]
            public string? AltText { get; set; }
            [FromForm]
            public string? Caption { get; set; }
            [FromForm]
            public string? Description { get; set; }
        }

        public class UpdateMediaRequest
        {
            public string? Title { get; set; }
            public string? AltText { get; set; }
            public string? Caption { get; set; }
            public string? Description { get; set; }
        }
    
