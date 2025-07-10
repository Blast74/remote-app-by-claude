using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteDesktopApp.Models;

public class RemoteApplication
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? DisplayName { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Path { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Arguments { get; set; }
    
    [MaxLength(500)]
    public string? IconPath { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ServerAddress { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsEnabled { get; set; } = true;
    public bool ShowInStartMenu { get; set; } = true;
    public bool IsDesktopShortcut { get; set; } = false;
    
    public string? Category { get; set; }
    public string? Version { get; set; }
    public string? Publisher { get; set; }
    
    public int LaunchCount { get; set; } = 0;
    public DateTime? LastLaunchedAt { get; set; }
    
    // Foreign key
    public int ConnectionProfileId { get; set; }
    
    // Navigation properties
    [ForeignKey("ConnectionProfileId")]
    public virtual ConnectionProfile ConnectionProfile { get; set; } = null!;
}