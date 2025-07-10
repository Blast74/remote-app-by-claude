using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteDesktopApp.Models;

public class ConnectionSession
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    
    public DateTime? EndTime { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ServerAddress { get; set; } = string.Empty;
    
    public int Port { get; set; } = 3389;
    
    [MaxLength(100)]
    public string? Username { get; set; }
    
    [MaxLength(500)]
    public string? ClientInfo { get; set; }
    
    public SessionStatus Status { get; set; } = SessionStatus.Connecting;
    
    [MaxLength(500)]
    public string? DisconnectReason { get; set; }
    
    public TimeSpan? Duration => EndTime?.Subtract(StartTime);
    
    public long BytesTransferred { get; set; } = 0;
    public long BytesReceived { get; set; } = 0;
    
    public int ReconnectAttempts { get; set; } = 0;
    public DateTime? LastReconnectAttempt { get; set; }
    
    public double AverageLatency { get; set; } = 0;
    public double MinLatency { get; set; } = 0;
    public double MaxLatency { get; set; } = 0;
    
    public float CpuUsage { get; set; } = 0;
    public float MemoryUsage { get; set; } = 0;
    public float NetworkUsage { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    // Foreign key
    public int? ConnectionProfileId { get; set; }
    
    // Navigation properties
    [ForeignKey("ConnectionProfileId")]
    public virtual ConnectionProfile? ConnectionProfile { get; set; }
}

public enum SessionStatus
{
    Connecting,
    Connected,
    Disconnected,
    Reconnecting,
    Failed,
    Terminated
}