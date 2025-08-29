using System;

namespace TechBirdsApi.Dto
{
   

    public class PageUpdateRequest
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? Status { get; set; }
        public Guid? ParentId { get; set; }
        public int? MenuOrder { get; set; }
        public string? Template { get; set; }
    }

    public class PageResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? Status { get; set; }
        public Guid? ParentId { get; set; }
        public int? MenuOrder { get; set; }
        public string? Template { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
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

}
