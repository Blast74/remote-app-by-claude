using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteDesktopServer.Models;

public class ServerSession
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Domain { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(45)]
    public string ClientIP { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string ClientInfo { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    
    public DateTime? EndTime { get; set; }
    
    public SessionState State { get; set; } = SessionState.Connecting;
    
    public SessionType Type { get; set; } = SessionType.Desktop;
    
    public int DesktopWidth { get; set; } = 1920;
    public int DesktopHeight { get; set; } = 1080;
    public int ColorDepth { get; set; } = 32;
    
    public bool AudioEnabled { get; set; } = true;
    public bool PrinterRedirectionEnabled { get; set; } = true;
    public bool DriveRedirectionEnabled { get; set; } = false;
    public bool ClipboardRedirectionEnabled { get; set; } = true;
    
    public long BytesTransferred { get; set; } = 0;
    public long BytesReceived { get; set; } = 0;
    
    public double AverageLatency { get; set; } = 0;
    public float CpuUsage { get; set; } = 0;
    public float MemoryUsageMB { get; set; } = 0;
    
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string? DisconnectReason { get; set; }
    
    public bool IsRecorded { get; set; } = false;
    [MaxLength(500)]
    public string? RecordingPath { get; set; }
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ServerUser User { get; set; } = null!;
    
    public virtual ICollection<SessionActivity> Activities { get; set; } = new List<SessionActivity>();
    public virtual ICollection<PerformanceSnapshot> PerformanceSnapshots { get; set; } = new List<PerformanceSnapshot>();
    
    // Computed properties
    public TimeSpan? Duration => EndTime?.Subtract(StartTime);
    public bool IsActive => State == SessionState.Connected;
    public TimeSpan IdleTime => DateTime.UtcNow.Subtract(LastActivity);
}

public enum SessionState
{
    Connecting,
    Connected,
    Disconnected,
    Reconnecting,
    Terminated,
    Failed,
    Idle,
    Locked
}

public enum SessionType
{
    Desktop,
    RemoteApp,
    Published,
    Console,
    Admin
}