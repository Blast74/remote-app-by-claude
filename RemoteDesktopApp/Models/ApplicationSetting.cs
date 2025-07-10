using System.ComponentModel.DataAnnotations;

namespace RemoteDesktopApp.Models;

public class ApplicationSetting
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
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public string? DefaultValue { get; set; }
    public bool IsSystemSetting { get; set; } = false;
    public bool RequiresRestart { get; set; } = false;
}