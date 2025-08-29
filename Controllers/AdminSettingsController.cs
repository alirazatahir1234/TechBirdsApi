using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechBirdsApi.Data;
using TechBirdsApi.Models;
using TechBirdsApi.Data.Dto;
using TechBirdsApi.Repositories;
using TechBirdsApi.Services;

[Authorize(Roles = "Admin,SuperAdmin")]
[ApiController]
[Route("api/admin/settings")]
public class AdminSettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    public AdminSettingsController(ISettingsService settingsService) { _settingsService = settingsService; }

    [HttpGet]
    public ActionResult<SettingsDto> GetSettings()
    {
    var settings = _settingsService.GetAsync().Result;
        if (settings == null) return NotFound();
        return Ok(new SettingsDto
        {
            SiteTitle = settings.SiteTitle ?? string.Empty,
            SiteDescription = settings.SiteDescription ?? string.Empty,
            AdminEmail = settings.AdminEmail ?? string.Empty,
            LogoUrl = settings.LogoUrl ?? string.Empty,
            DefaultLanguage = settings.DefaultLanguage ?? string.Empty,
            ItemsPerPage = settings.ItemsPerPage,
            EnableRegistration = settings.EnableRegistration,
            MaintenanceMode = settings.MaintenanceMode
        });
    }

    [HttpPost]
    public ActionResult UpdateSettings([FromBody] SettingsDto dto)
    {
    var settings = _settingsService.GetAsync().Result;
        if (settings == null)
        {
            settings = new Settings();
        }
        settings.SiteTitle = dto.SiteTitle;
        settings.SiteDescription = dto.SiteDescription;
        settings.AdminEmail = dto.AdminEmail;
        settings.LogoUrl = dto.LogoUrl;
        settings.DefaultLanguage = dto.DefaultLanguage;
        settings.ItemsPerPage = dto.ItemsPerPage;
        settings.EnableRegistration = dto.EnableRegistration;
        settings.MaintenanceMode = dto.MaintenanceMode;
    _settingsService.UpdateAsync(settings).Wait();
        return Ok();
    }

    [HttpPost("logo")]
    public async Task<ActionResult> UploadLogo([FromForm] LogoUploadDto dto)
    {
        var file = dto.File;
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(uploads);
        var fileName = $"logo_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploads, fileName);
        using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }
    var settings = await _settingsService.GetAsync();
        if (settings != null)
        {
            settings.LogoUrl = $"/uploads/{fileName}";
            await _settingsService.UpdateAsync(settings);
        }
        return Ok(new { logoUrl = $"/uploads/{fileName}" });
    }
}
