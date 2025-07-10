namespace RemoteDesktopServer.API.DTOs;

public class SessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ClientIP { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime LastActivity { get; set; }
    public string State { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public float CpuUsage { get; set; }
    public float MemoryUsage { get; set; }
    public long BytesTransferred { get; set; }
    public long BytesReceived { get; set; }
    public string? DisconnectReason { get; set; }
    public TimeSpan? Duration => EndTime?.Subtract(StartTime) ?? DateTime.UtcNow.Subtract(StartTime);
}

public class SessionActivityDto
{
    public int Id { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ResourceAccessed { get; set; }
}

public class ServerStatusDto
{
    public bool IsRunning { get; set; }
    public int Port { get; set; }
    public int ActiveConnections { get; set; }
    public int ActiveSessions { get; set; }
    public int MaxSessions { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public DateTime StartTime { get; set; }
    public string Version { get; set; } = string.Empty;
    public TimeSpan Uptime => DateTime.UtcNow.Subtract(StartTime);
}

public class PerformanceDto
{
    public DateTime Timestamp { get; set; }
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public long MemoryUsageMB { get; set; }
    public double DiskUsagePercent { get; set; }
    public double NetworkInKbps { get; set; }
    public double NetworkOutKbps { get; set; }
    public int ActiveSessions { get; set; }
    public int ConnectedUsers { get; set; }
    public double AverageResponseTime { get; set; }
    public double SystemLoad { get; set; }
}

public class ApplicationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string ExecutablePath { get; set; } = string.Empty;
    public string? IconPath { get; set; }
    public bool IsEnabled { get; set; }
    public string? Category { get; set; }
    public int CurrentInstances { get; set; }
    public int MaxConcurrentInstances { get; set; }
    public int LaunchCount { get; set; }
    public DateTime? LastLaunchedAt { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int ActiveSessionsCount { get; set; }
    public int MaxConcurrentSessions { get; set; }
    public bool TwoFactorEnabled { get; set; }
}

public class SecurityEventDto
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? SourceIP { get; set; }
    public string? Details { get; set; }
    public string Severity { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? SessionId { get; set; }
}