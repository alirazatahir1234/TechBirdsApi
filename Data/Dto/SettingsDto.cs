using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TechBirdsApi.Data.Dto
{
    public class SettingsDto
    {
        [Required]
        [MaxLength(100)]
        public string SiteTitle { get; set; }

        [MaxLength(500)]
        public string SiteDescription { get; set; }

        [Required]
        [EmailAddress]
        public string AdminEmail { get; set; }

        [MaxLength(255)]
        public string LogoUrl { get; set; }

        [MaxLength(10)]
        public string DefaultLanguage { get; set; }

        [Required]
        [RegularExpression("^(light|dark)$", ErrorMessage = "Theme must be 'light' or 'dark'.")]
        public string Theme { get; set; }

        [Range(1, 100)]
        public int ItemsPerPage { get; set; }

        public bool EnableRegistration { get; set; }
        public bool MaintenanceMode { get; set; }
    }

        public class LogoUploadDto
        {
            [FromForm]
            public IFormFile File { get; set; }
        }
}
