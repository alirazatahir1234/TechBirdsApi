namespace TechBirdsWebAPI.Models
{
    public class SystemException
    {
        public int Id { get; set; }
        
        // User Context (optional - can be null for system-level exceptions)
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        
        // Exception Information
        public string ExceptionType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }
        
        // Request Context
        public string? RequestPath { get; set; }
        public string? HttpMethod { get; set; }
        public string? QueryString { get; set; }
        public string? RequestBody { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        
        // Classification
        public string Severity { get; set; } = "Error"; // Info, Warning, Error, Critical
        public string Source { get; set; } = string.Empty; // Controller, Service, etc.
        public string? Category { get; set; } // Authentication, Database, Validation, etc.
        
        // Status
        public bool IsResolved { get; set; } = false;
        public string? Resolution { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Properties
        public virtual ApplicationUser? User { get; set; }
    }
}
