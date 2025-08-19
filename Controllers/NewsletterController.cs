using Microsoft.AspNetCore.Mvc;

namespace TechBirdsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsletterController : ControllerBase
    {
        [HttpPost("subscribe")]
        public IActionResult Subscribe([FromBody] EmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }

            // TODO: Implement email subscription logic
            // For now, just return success
            return Ok(new { success = true, message = "Subscribed successfully" });
        }
    }

    public class EmailRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
