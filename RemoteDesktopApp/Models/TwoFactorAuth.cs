using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteDesktopApp.Models;

public class TwoFactorAuth
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string SecretKey { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? BackupCodes { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    
    public string? QRCodePath { get; set; }
    public string? RecoveryEmail { get; set; }
    
    public int UsageCount { get; set; } = 0;
    
    // Foreign key
    public int UserProfileId { get; set; }
    
    // Navigation properties
    [ForeignKey("UserProfileId")]
    public virtual UserProfile UserProfile { get; set; } = null!;
}