using System.ComponentModel.DataAnnotations;

namespace RemoteDesktopServer.Models;

public class ServerUser
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Domain { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? FullName { get; set; }
    
    [MaxLength(255)]
    public string? Email { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsAdmin { get; set; } = false;
    public bool IsLocked { get; set; } = false;
    
    public DateTime? LockoutEndTime { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    
    public bool RequirePasswordChange { get; set; } = false;
    public DateTime? PasswordLastChanged { get; set; }
    
    public bool TwoFactorEnabled { get; set; } = false;
    public string? TwoFactorSecret { get; set; }
    
    public int MaxConcurrentSessions { get; set; } = 2;
    public bool AllowRemoteApp { get; set; } = true;
    public bool AllowDesktop { get; set; } = true;
    
    public string? ProfilePath { get; set; }
    public string? HomeDirectory { get; set; }
    
    [MaxLength(2000)]
    public string? AllowedHours { get; set; } // JSON format for time restrictions
    
    [MaxLength(500)]
    public string? AllowedIPs { get; set; } // Comma-separated IP ranges
    
    public DateTime? AccountExpirationDate { get; set; }
    
    // Navigation properties
    public virtual ICollection<ServerSession> Sessions { get; set; } = new List<ServerSession>();
    public virtual ICollection<UserGroup> Groups { get; set; } = new List<UserGroup>();
    public virtual ICollection<SecurityEvent> SecurityEvents { get; set; } = new List<SecurityEvent>();
    public virtual ICollection<PublishedApplication> PublishedApplications { get; set; } = new List<PublishedApplication>();
    public virtual TwoFactorAuth? TwoFactorAuth { get; set; }
    
    // Computed properties
    public int ActiveSessionsCount => Sessions.Count(s => s.IsActive);
    public bool IsAccountExpired => AccountExpirationDate.HasValue && AccountExpirationDate < DateTime.UtcNow;
    public bool IsCurrentlyLocked => IsLocked && (!LockoutEndTime.HasValue || LockoutEndTime > DateTime.UtcNow);
}