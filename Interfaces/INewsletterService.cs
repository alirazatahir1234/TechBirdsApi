using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Models.Newsletter;

namespace TechBirdsApi.Services
{
    public interface INewsletterService
    {
        Task<List<SubscriberResponse>> GetSubscribersAsync(SubscriberQueryParams parameters);
        Task<CampaignResponse> CreateCampaignAsync(CreateCampaignRequest request);
        Task<List<CampaignResponse>> GetCampaignsAsync();
        Task<CampaignResponse?> GetCampaignAsync(int id);
        Task<bool> SendCampaignAsync(int id);
    }
}
