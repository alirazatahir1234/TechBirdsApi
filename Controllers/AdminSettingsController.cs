using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechBirdsApi.Data;
using TechBirdsApi.Models;
using TechBirdsApi.Data.Dto;

[Authorize(Roles = "Admin,SuperAdmin")]
[ApiController]
[Route("api/admin/settings")]
public class AdminSettingsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public AdminSettingsController(ApplicationDbContext db) { _db = db; }

    [HttpGet]
    public ActionResult<SettingsDto> GetSettings()
    {
        var settings = _db.Settings.FirstOrDefault();
        if (settings == null) return NotFound();
        return Ok(new SettingsDto
        {
            SiteTitle = settings.SiteTitle,
            SiteDescription = settings.SiteDescription,
            AdminEmail = settings.AdminEmail,
            LogoUrl = settings.LogoUrl,
            DefaultLanguage = settings.DefaultLanguage,
            Theme = settings.Theme,
            ItemsPerPage = settings.ItemsPerPage,
            EnableRegistration = settings.EnableRegistration,
            MaintenanceMode = settings.MaintenanceMode
        });
    }

    [HttpPost]
    public ActionResult UpdateSettings([FromBody] SettingsDto dto)
    {
        var settings = _db.Settings.FirstOrDefault();
        if (settings == null)
        {
            settings = new Settings();
            _db.Settings.Add(settings);
        }
        settings.SiteTitle = dto.SiteTitle;
        settings.SiteDescription = dto.SiteDescription;
        settings.AdminEmail = dto.AdminEmail;
        settings.LogoUrl = dto.LogoUrl;
        settings.DefaultLanguage = dto.DefaultLanguage;
        settings.Theme = dto.Theme;
        settings.ItemsPerPage = dto.ItemsPerPage;
        settings.EnableRegistration = dto.EnableRegistration;
        settings.MaintenanceMode = dto.MaintenanceMode;
        _db.SaveChanges();
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
        var settings = _db.Settings.FirstOrDefault();
        if (settings != null)
        {
            settings.LogoUrl = $"/uploads/{fileName}";
            _db.SaveChanges();
        }
        return Ok(new { logoUrl = $"/uploads/{fileName}" });
    }
}
