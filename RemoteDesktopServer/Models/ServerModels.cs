using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteDesktopServer.Models;

// Session Activity Tracking
public class SessionActivity
{
    [Key]
    public int Id { get; set; }
    
    public int SessionId { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(100)]
    public string ActivityType { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Details { get; set; }
    
    [MaxLength(500)]
    public string? ResourceAccessed { get; set; }
    
    [ForeignKey("SessionId")]
    public virtual ServerSession Session { get; set; } = null!;
}

// Performance Monitoring
public class PerformanceSnapshot
{
    [Key]
    public int Id { get; set; }
    
    public int? SessionId { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public long MemoryUsageMB { get; set; }
    public double DiskUsagePercent { get; set; }
    public double NetworkInKbps { get; set; }
    public double NetworkOutKbps { get; set; }
    
    public int ActiveSessions { get; set; }
    public int ConnectedUsers { get; set; }
    
    public double AverageResponseTime { get; set; }
    public double SystemLoad { get; set; }
    
    [ForeignKey("SessionId")]
    public virtual ServerSession? Session { get; set; }
}

// Security Events
public class SecurityEvent
{
    [Key]
    public int Id { get; set; }
    
    public int? UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(45)]
    public string? SourceIP { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    [MaxLength(2000)]
    public string? Details { get; set; }
    
    public SecurityEventSeverity Severity { get; set; } = SecurityEventSeverity.Info;
    
    public bool Success { get; set; } = true;
    
    [MaxLength(100)]
    public string? SessionId { get; set; }
    
    [MaxLength(100)]
    public string? Username { get; set; }
    
    [ForeignKey("UserId")]
    public virtual ServerUser? User { get; set; }
}

public enum SecurityEventSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

// User Groups
public class UserGroup
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Permissions
    public bool CanAccessDesktop { get; set; } = true;
    public bool CanAccessRemoteApp { get; set; } = true;
    public bool CanAccessAdmin { get; set; } = false;
    
    public int MaxConcurrentSessions { get; set; } = 5;
    public int SessionTimeoutMinutes { get; set; } = 480;
    
    [MaxLength(2000)]
    public string? AllowedHours { get; set; } // JSON format
    
    [MaxLength(500)]
    public string? AllowedIPs { get; set; }
    
    // Navigation properties
    public virtual ICollection<ServerUser> Users { get; set; } = new List<ServerUser>();
    public virtual ICollection<PublishedApplication> Applications { get; set; } = new List<PublishedApplication>();
}

// Application Instances
public class ApplicationInstance
{
    [Key]
    public int Id { get; set; }
    
    public int ApplicationId { get; set; }
    public int SessionId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();
    
    public int ProcessId { get; set; }
    
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    
    public bool IsRunning { get; set; } = true;
    
    public double CpuUsage { get; set; }
    public long MemoryUsageMB { get; set; }
    
    [MaxLength(500)]
    public string? ExitReason { get; set; }
    
    public int ExitCode { get; set; }
    
    [ForeignKey("ApplicationId")]
    public virtual PublishedApplication Application { get; set; } = null!;
    
    [ForeignKey("SessionId")]
    public virtual ServerSession Session { get; set; } = null!;
    
    public TimeSpan? Runtime => EndTime?.Subtract(StartTime) ?? DateTime.UtcNow.Subtract(StartTime);
}

// Application Launch Logs
public class ApplicationLaunchLog
{
    [Key]
    public int Id { get; set; }
    
    public int ApplicationId { get; set; }
    public int UserId { get; set; }
    public int SessionId { get; set; }
    
    public DateTime LaunchTime { get; set; } = DateTime.UtcNow;
    public DateTime? ExitTime { get; set; }
    
    public bool Success { get; set; } = true;
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    [MaxLength(45)]
    public string? ClientIP { get; set; }
    
    public TimeSpan? Duration => ExitTime?.Subtract(LaunchTime);
    
    [ForeignKey("ApplicationId")]
    public virtual PublishedApplication Application { get; set; } = null!;
    
    [ForeignKey("UserId")]
    public virtual ServerUser User { get; set; } = null!;
    
    [ForeignKey("SessionId")]
    public virtual ServerSession Session { get; set; } = null!;
}

// Server Configuration
public class ServerConfiguration
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Value { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Category { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsEncrypted { get; set; } = false;
    public bool RequiresRestart { get; set; } = false;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }
}

// Certificate Management
public class ServerCertificate
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Thumbprint { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Subject { get; set; }
    
    [MaxLength(500)]
    public string? Issuer { get; set; }
    
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    
    public CertificateUsage Usage { get; set; } = CertificateUsage.RDP;
    
    [MaxLength(500)]
    public string? CertificatePath { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsExpired => DateTime.UtcNow > ValidUntil;
    public bool IsExpiringSoon => DateTime.UtcNow.AddDays(30) > ValidUntil;
}

public enum CertificateUsage
{
    RDP,
    API,
    SSL,
    Authentication
}