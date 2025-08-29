using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechBirdsApi.Services;
using TechBirdsApi.Models.Newsletter;

namespace TechBirdsApi.Controllers
{
    [Route("api/admin/newsletter")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminNewsletterController : ControllerBase
    {
        private readonly INewsletterService _newsletterService;

        public AdminNewsletterController(INewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }

        [HttpGet("subscribers")]
    public async Task<IActionResult> GetSubscribers([FromQuery] SubscriberQueryParams parameters)
        {
            try
            {
                var subscribers = await _newsletterService.GetSubscribersAsync(parameters);
                var totalCount = subscribers.Count;
                return Ok(new SubscribersListResponse
                {
                    Data = subscribers,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving subscribers", error = ex.Message });
            }
        }

    [HttpPost("campaigns")]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignRequest request)
        {
            try
            {
                var campaign = await _newsletterService.CreateCampaignAsync(request);
                return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, (object)campaign);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating campaign", error = ex.Message });
            }
        }

    [HttpGet("campaigns")]
    public async Task<IActionResult> GetCampaigns()
        {
            try
            {
                var campaigns = await _newsletterService.GetCampaignsAsync();
                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving campaigns", error = ex.Message });
            }
        }

    [HttpGet("campaigns/{id}")]
    public async Task<IActionResult> GetCampaign(int id)
        {
            try
            {
                var campaign = await _newsletterService.GetCampaignAsync(id);
                if (campaign is null)
                    return NotFound(new { message = "Campaign not found" });
                else
                    return Ok(campaign!);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving campaign", error = ex.Message });
            }
        }

    [HttpPost("campaigns/{id}/send")]
    public async Task<IActionResult> SendCampaign(int id)
        {
            try
            {
                var sent = await _newsletterService.SendCampaignAsync(id);
                if (!sent)
                    return NotFound(new { message = "Campaign not found" });
                return Ok(new { message = "Campaign sent successfully", campaignId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending campaign", error = ex.Message });
            }
        }
    }

    // DTOs
    // DTOs moved to TechBirdsApi.Models.Newsletter
}
