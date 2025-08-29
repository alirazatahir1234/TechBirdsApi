using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechBirdsApi.Models.Newsletter;

namespace TechBirdsApi.Services
{
    public class NewsletterService : INewsletterService
    {
        // In-memory stub for demonstration
        private static readonly List<SubscriberResponse> Subscribers = new()
        {
            new SubscriberResponse
            {
                Id = 1,
                Email = "subscriber@example.com",
                FirstName = "John",
                LastName = "Doe",
                Status = "active",
                Source = "website",
                SubscribedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        private static readonly List<CampaignResponse> Campaigns = new()
        {
            new CampaignResponse
            {
                Id = 1,
                Subject = "Welcome to TechBirds!",
                Content = "Thank you for subscribing...",
                Status = "sent",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                SentAt = DateTime.UtcNow.AddDays(-6),
                SubscriberCount = 150,
                OpenRate = 25.5,
                ClickRate = 5.2
            }
        };

        public Task<List<SubscriberResponse>> GetSubscribersAsync(SubscriberQueryParams parameters)
        {
            var paginated = Subscribers
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();
            return Task.FromResult(paginated);
        }

        public Task<CampaignResponse> CreateCampaignAsync(CreateCampaignRequest request)
        {
            var campaign = new CampaignResponse
            {
                Id = Campaigns.Count + 1,
                Subject = request.Subject,
                Content = request.Content,
                Status = "draft",
                CreatedAt = DateTime.UtcNow,
                SubscriberCount = 0
            };
            Campaigns.Add(campaign);
            return Task.FromResult(campaign);
        }

        public Task<List<CampaignResponse>> GetCampaignsAsync()
        {
            return Task.FromResult(Campaigns);
        }

        public Task<CampaignResponse?> GetCampaignAsync(int id)
        {
            var campaign = Campaigns.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(campaign);
        }

        public Task<bool> SendCampaignAsync(int id)
        {
            var campaign = Campaigns.FirstOrDefault(c => c.Id == id);
            if (campaign != null)
            {
                campaign.Status = "sent";
                campaign.SentAt = DateTime.UtcNow;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
