using System.ComponentModel.DataAnnotations;

namespace RemoteDesktopApp.Models;

public class UserProfile
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Email { get; set; }
    
    [MaxLength(200)]
    public string? FullName { get; set; }
    
    [MaxLength(500)]
    public string? EncryptedPassword { get; set; }
    
    public bool TwoFactorEnabled { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    public int FailedLoginAttempts { get; set; } = 0;
    public bool IsLockedOut { get; set; } = false;
    public DateTime? LockoutEndTime { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsAdmin { get; set; } = false;
    
    public string? ProfileImagePath { get; set; }
    public string? Theme { get; set; } = "Dark";
    public string? Language { get; set; } = "en-US";
    
    public string? PreferredConnectionSettings { get; set; }
    
    // Navigation properties
    public virtual ICollection<TwoFactorAuth> TwoFactorAuths { get; set; } = new List<TwoFactorAuth>();
}