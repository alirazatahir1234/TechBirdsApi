using System;

namespace TechBirdsApi.Models
{
    public class MediaItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // File info
        public string FileName { get; set; } = string.Empty; // Stored file name (unique)
        public string OriginalFileName { get; set; } = string.Empty; // Name from client
        public string MimeType { get; set; } = string.Empty;
        public long Size { get; set; }

        // Image metadata (optional)
        public int? Width { get; set; }
        public int? Height { get; set; }

        // Public URLs
        public string Url { get; set; } = string.Empty; // Public URL under /uploads/yyyy/MM/
        public string? ThumbnailUrl { get; set; }

        // Storage paths (server-side)
        public string StoragePath { get; set; } = string.Empty; // Absolute path on disk
        public string? ThumbnailPath { get; set; }

        // Descriptive metadata (like WordPress)
        public string Title { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public string? Caption { get; set; }
        public string? Description { get; set; }

        // Ownership
        public string UploadedByUserId { get; set; } = string.Empty;
        public ApplicationUser? UploadedBy { get; set; }

        // Lifecycle
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
