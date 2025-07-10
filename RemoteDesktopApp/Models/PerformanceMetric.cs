using System.ComponentModel.DataAnnotations;

namespace RemoteDesktopApp.Models;

public class PerformanceMetric
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string? SessionId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string MetricType { get; set; } = string.Empty;
    
    [Required]
    public double Value { get; set; }
    
    [MaxLength(20)]
    public string? Unit { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(255)]
    public string? ServerAddress { get; set; }
    
    public string? AdditionalData { get; set; }
}

public static class MetricTypes
{
    public const string CpuUsage = "CPU_USAGE";
    public const string MemoryUsage = "MEMORY_USAGE";
    public const string NetworkLatency = "NETWORK_LATENCY";
    public const string NetworkBandwidth = "NETWORK_BANDWIDTH";
    public const string FrameRate = "FRAME_RATE";
    public const string BytesTransferred = "BYTES_TRANSFERRED";
    public const string BytesReceived = "BYTES_RECEIVED";
    public const string ConnectionQuality = "CONNECTION_QUALITY";
    public const string DiskUsage = "DISK_USAGE";
    public const string ApplicationResponseTime = "APP_RESPONSE_TIME";
}