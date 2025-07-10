using System.ComponentModel.DataAnnotations;

namespace RemoteDesktopApp.Models;

public class SecurityAuditLog
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string? Username { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [MaxLength(45)]
    public string? IPAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    [MaxLength(2000)]
    public string? Details { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [Required]
    public AuditSeverity Severity { get; set; } = AuditSeverity.Info;
    
    public string? SessionId { get; set; }
    public string? Resource { get; set; }
    public bool Success { get; set; } = true;
}

public enum AuditSeverity
{
    Info,
    Warning,
    Error,
    Critical
}