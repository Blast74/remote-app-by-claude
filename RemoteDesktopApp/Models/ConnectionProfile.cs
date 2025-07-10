using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteDesktopApp.Models;

public class ConnectionProfile
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string ServerAddress { get; set; } = string.Empty;
    
    public int Port { get; set; } = 3389;
    
    [MaxLength(100)]
    public string? Username { get; set; }
    
    [MaxLength(500)]
    public string? EncryptedPassword { get; set; }
    
    [MaxLength(100)]
    public string? Domain { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public bool SaveCredentials { get; set; } = false;
    public bool ConnectToConsole { get; set; } = false;
    public bool EnableCredSSP { get; set; } = true;
    public bool AudioRedirection { get; set; } = true;
    public bool PrinterRedirection { get; set; } = true;
    public bool DriveRedirection { get; set; } = false;
    public bool ClipboardRedirection { get; set; } = true;
    
    public int ColorDepth { get; set; } = 32;
    public int DesktopWidth { get; set; } = 1920;
    public int DesktopHeight { get; set; } = 1080;
    public bool FullScreen { get; set; } = false;
    
    public bool IsFavorite { get; set; } = false;
    public string? IconPath { get; set; }
    public string? Category { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastConnectedAt { get; set; }
    
    public int ConnectionCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<ConnectionSession> Sessions { get; set; } = new List<ConnectionSession>();
    public virtual ICollection<RemoteApplication> RemoteApplications { get; set; } = new List<RemoteApplication>();
}