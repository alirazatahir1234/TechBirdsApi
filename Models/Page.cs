using System;
using System.Collections.Generic;

namespace TechBirdsWebAPI.Models
{
    public class Page
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Core content
        public string Title { get; set; } = string.Empty;          // Max 200
        public string Slug { get; set; } = string.Empty;           // unique (lowercase, hyphenated)
        public string Content { get; set; } = string.Empty;        // HTML/Markdown/body
        public string? Excerpt { get; set; }                       // summary

        // Publishing
        public string Status { get; set; } = "draft";              // draft|published|private
        public DateTime? PublishedAt { get; set; }                 // when published
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Hierarchy & ordering
        public Guid? ParentId { get; set; }
        public Page? Parent { get; set; }
        public ICollection<Page> Children { get; set; } = new List<Page>();
        public int MenuOrder { get; set; } = 0;                    // ordering in menus
        public string? Template { get; set; }                      // custom template key

        // Ownership
        public string AuthorId { get; set; } = string.Empty;       // FK -> AspNetUsers.Id
        public ApplicationUser? Author { get; set; }

        // Media
        public Guid? FeaturedMediaId { get; set; }                 // FK -> MediaItem.Id
        public MediaItem? FeaturedMedia { get; set; }

        // SEO & Meta
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? MetaJson { get; set; }                      // JSON for arbitrary meta

        // Revisions
        public ICollection<PageRevision> Revisions { get; set; } = new List<PageRevision>();
    }

    public class PageRevision
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PageId { get; set; }
        public Page? Page { get; set; }

        public int Version { get; set; } = 1;                      // incremental version
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? ChangeSummary { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        public ApplicationUser? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
