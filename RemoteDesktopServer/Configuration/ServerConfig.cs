namespace RemoteDesktopServer.Configuration;

public class ServerSettings
{
    public string ServerName { get; set; } = string.Empty;
    public int ListenPort { get; set; }
    public int MaxConcurrentSessions { get; set; }
    public int SessionTimeoutMinutes { get; set; }
    public int MaxIdleTimeMinutes { get; set; }
    public bool EnableSessionRecording { get; set; }
    public string RecordingPath { get; set; } = string.Empty;
    public string TempPath { get; set; } = string.Empty;
    public string LogsPath { get; set; } = string.Empty;
}

public class SecuritySettings
{
    public string EncryptionLevel { get; set; } = string.Empty;
    public string EncryptionKey { get; set; } = string.Empty;
    public bool RequireNLA { get; set; }
    public string[] AllowedAuthMethods { get; set; } = Array.Empty<string>();
    public string CertificateThumbprint { get; set; } = string.Empty;
    public bool RequireSecureRPC { get; set; }
    public bool EnableAuditLogging { get; set; }
    public int FailedLoginAttempts { get; set; }
    public int LockoutDurationMinutes { get; set; }
    public bool PasswordComplexity { get; set; }
    public string SessionEncryption { get; set; } = string.Empty;
}

public class ActiveDirectorySettings
{
    public string Domain { get; set; } = string.Empty;
    public string LdapPath { get; set; } = string.Empty;
    public bool UseSSL { get; set; }
    public int AuthenticationTimeout { get; set; }
}

public class PerformanceSettings
{
    public int MonitoringInterval { get; set; }
    public bool PerformanceCountersEnabled { get; set; }
    public double MaxCpuUsage { get; set; }
    public double MaxMemoryUsage { get; set; }
    public double MaxDiskUsage { get; set; }
    public AlertThresholds AlertThresholds { get; set; } = new();
}

public class AlertThresholds
{
    public double CpuCritical { get; set; }
    public double MemoryCritical { get; set; }
    public double DiskCritical { get; set; }
}

public class RemoteAppSettings
{
    public bool Enabled { get; set; }
    public string ApplicationsPath { get; set; } = string.Empty;
    public string IconsPath { get; set; } = string.Empty;
    public int MaxApplications { get; set; }
    public int ApplicationTimeout { get; set; }
}

public class LoadBalancingSettings
{
    public bool Enabled { get; set; }
    public string[] ServerFarm { get; set; } = Array.Empty<string>();
    public string LoadBalanceMethod { get; set; } = string.Empty;
    public int HealthCheckInterval { get; set; }
}

public class ApiSettings
{
    public bool Enabled { get; set; }
    public int Port { get; set; }
    public bool UseHttps { get; set; }
    public string JwtSecret { get; set; } = string.Empty;
    public int JwtExpirationHours { get; set; }
    public RateLimitingSettings RateLimiting { get; set; } = new();
}

public class RateLimitingSettings
{
    public int RequestsPerMinute { get; set; }
    public int BurstSize { get; set; }
}

public class ClusteringSettings
{
    public bool Enabled { get; set; }
    public string NodeId { get; set; } = string.Empty;
    public string[] ClusterNodes { get; set; } = Array.Empty<string>();
    public int HeartbeatInterval { get; set; }
    public int ElectionTimeout { get; set; }
}

public class BackupSettings
{
    public bool Enabled { get; set; }
    public string BackupPath { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public string BackupInterval { get; set; } = string.Empty;
    public TimeSpan BackupTime { get; set; }
}