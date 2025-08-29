using System;
using System.Collections.Generic;

namespace TechBirdsApi.Models.Newsletter
{
    public class SubscriberQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Status { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }

    public class CreateCampaignRequest
    {
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class SubscriberResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime SubscribedAt { get; set; }
        public DateTime? UnsubscribedAt { get; set; }
    }

    public class CampaignResponse
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public int SubscriberCount { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
    }

    public class SubscribersListResponse
    {
        public List<SubscriberResponse> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
