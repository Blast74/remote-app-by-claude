using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteDesktopServer.Models;

public class PublishedApplication
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? DisplayName { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string ExecutablePath { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? CommandLineArguments { get; set; }
    
    [MaxLength(500)]
    public string? WorkingDirectory { get; set; }
    
    [MaxLength(500)]
    public string? IconPath { get; set; }
    
    public bool IsEnabled { get; set; } = true;
    public bool IsPublic { get; set; } = false;
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [MaxLength(100)]
    public string? Version { get; set; }
    
    [MaxLength(200)]
    public string? Publisher { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLaunchedAt { get; set; }
    public int LaunchCount { get; set; } = 0;
    
    public int MaxConcurrentInstances { get; set; } = 10;
    public int TimeoutMinutes { get; set; } = 60;
    
    public bool RequireAdminRights { get; set; } = false;
    public bool AllowFileAccess { get; set; } = true;
    public bool AllowPrinterAccess { get; set; } = true;
    public bool AllowClipboardAccess { get; set; } = true;
    
    [MaxLength(2000)]
    public string? EnvironmentVariables { get; set; } // JSON format
    
    [MaxLength(1000)]
    public string? FileAssociations { get; set; } // JSON format
    
    public int Priority { get; set; } = 0; // Display order
    
    public bool AutoStart { get; set; } = false;
    public bool MinimizeOnStart { get; set; } = false;
    
    // Security and Access Control
    [MaxLength(2000)]
    public string? AllowedUsers { get; set; } // JSON format for user restrictions
    
    [MaxLength(2000)]
    public string? AllowedGroups { get; set; } // JSON format for group restrictions
    
    [MaxLength(500)]
    public string? AllowedIPs { get; set; } // Comma-separated IP ranges
    
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableUntil { get; set; }
    
    [MaxLength(1000)]
    public string? AvailableHours { get; set; } // JSON format for time restrictions
    
    // Licensing
    public int MaxLicenses { get; set; } = -1; // -1 = unlimited
    public int UsedLicenses { get; set; } = 0;
    
    // Performance Settings
    public int MaxMemoryMB { get; set; } = 1024;
    public double MaxCpuPercent { get; set; } = 50.0;
    
    // Navigation properties
    public virtual ICollection<ServerUser> AuthorizedUsers { get; set; } = new List<ServerUser>();
    public virtual ICollection<UserGroup> AuthorizedGroups { get; set; } = new List<UserGroup>();
    public virtual ICollection<ApplicationInstance> Instances { get; set; } = new List<ApplicationInstance>();
    public virtual ICollection<ApplicationLaunchLog> LaunchLogs { get; set; } = new List<ApplicationLaunchLog>();
    
    // Computed properties
    public int CurrentInstances => Instances.Count(i => i.IsRunning);
    public bool IsAvailable => IsEnabled && 
                              (!AvailableFrom.HasValue || AvailableFrom <= DateTime.UtcNow) &&
                              (!AvailableUntil.HasValue || AvailableUntil >= DateTime.UtcNow);
    public bool HasAvailableLicenses => MaxLicenses == -1 || UsedLicenses < MaxLicenses;
}