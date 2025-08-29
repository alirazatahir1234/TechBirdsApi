using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TechBirdsApi.Models
{
    public class Settings
    {
        [Key]
        public int Id { get; set; }
    public string? SiteTitle { get; set; }
    public string? SiteDescription { get; set; }
    public string? AdminEmail { get; set; }
    public string? LogoUrl { get; set; }
    public string? DefaultLanguage { get; set; }
        public int ItemsPerPage { get; set; }
        public bool EnableRegistration { get; set; }
        public bool MaintenanceMode { get; set; }
    }
}