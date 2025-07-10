using RemoteDesktopServer.Models;

namespace RemoteDesktopServer.Services;

// Session Management
public interface ISessionManager
{
    Task<ServerSession> CreateSessionAsync(SessionRequest request);
    Task<ServerSession?> GetSessionAsync(string sessionId);
    Task<IEnumerable<ServerSession>> GetActiveSessionsAsync();
    Task<IEnumerable<ServerSession>> GetUserSessionsAsync(string username);
    Task UpdateSessionAsync(ServerSession session);
    Task EndSessionAsync(string sessionId, string reason);
    Task LogSessionActivityAsync(string sessionId, string activityType, string details);
    Task<bool> CanUserCreateSessionAsync(string username);
    Task CleanupExpiredSessionsAsync();
}

public class SessionRequest
{
    public string Username { get; set; } = string.Empty;
    public string ClientIP { get; set; } = string.Empty;
    public SessionType SessionType { get; set; } = SessionType.Desktop;
    public string ConnectionId { get; set; } = string.Empty;
    public int DesktopWidth { get; set; } = 1920;
    public int DesktopHeight { get; set; } = 1080;
    public int ColorDepth { get; set; } = 32;
}

// Authentication Service
public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string username, string password, string domain, string clientIP);
    Task<bool> ValidateTwoFactorAsync(string username, string code);
    Task<ServerUser?> GetUserAsync(string username, string domain);
    Task<bool> IsUserLockedAsync(string username, string domain);
    Task LockUserAsync(string username, string domain, string reason);
    Task UnlockUserAsync(string username, string domain);
    Task LogFailedLoginAttemptAsync(string username, string domain, string clientIP, string reason);
}

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public ServerUser? User { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public string? TwoFactorMethod { get; set; }
}

// Performance Monitor
public interface IPerformanceMonitor
{
    Task<PerformanceSnapshot> GetCurrentPerformanceAsync();
    Task<PerformanceSnapshot> GetSessionPerformanceAsync(string sessionId);
    Task StartMonitoringAsync();
    Task StopMonitoringAsync();
    Task<IEnumerable<PerformanceSnapshot>> GetPerformanceHistoryAsync(DateTime from, DateTime to);
    event EventHandler<PerformanceAlert>? PerformanceAlertRaised;
}

public class PerformanceAlert
{
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Threshold { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Security Service
public interface ISecurityService
{
    Task LogSecurityEventAsync(string eventType, string details, SecurityEventSeverity severity, string? username = null, string? sessionId = null);
    Task<IEnumerable<SecurityEvent>> GetSecurityEventsAsync(DateTime from, DateTime to);
    Task<IEnumerable<SecurityEvent>> GetUserSecurityEventsAsync(string username, DateTime from, DateTime to);
    Task<bool> IsIPAddressBlockedAsync(string ipAddress);
    Task BlockIPAddressAsync(string ipAddress, string reason, TimeSpan? duration = null);
    Task UnblockIPAddressAsync(string ipAddress);
    Task<SecurityReport> GenerateSecurityReportAsync(DateTime from, DateTime to);
}

public class SecurityReport
{
    public int TotalEvents { get; set; }
    public int FailedLogins { get; set; }
    public int SuccessfulLogins { get; set; }
    public int ActiveSessions { get; set; }
    public int BlockedIPs { get; set; }
    public List<string> TopFailedUsers { get; set; } = new();
    public List<string> TopSourceIPs { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

// Application Management
public interface IApplicationManager
{
    Task<IEnumerable<PublishedApplication>> GetPublishedApplicationsAsync(bool enabledOnly = true);
    Task<PublishedApplication?> GetApplicationAsync(int applicationId);
    Task<IEnumerable<PublishedApplication>> GetUserApplicationsAsync(string username);
    Task<PublishedApplication> CreateApplicationAsync(PublishedApplication application);
    Task<PublishedApplication> UpdateApplicationAsync(PublishedApplication application);
    Task<bool> DeleteApplicationAsync(int applicationId);
    Task<ApplicationInstance> LaunchApplicationAsync(int applicationId, string sessionId);
    Task<bool> TerminateApplicationInstanceAsync(string instanceId);
    Task<IEnumerable<ApplicationInstance>> GetRunningInstancesAsync();
    Task CleanupTerminatedInstancesAsync();
}

// Configuration Management
public interface IConfigurationManager
{
    Task<T?> GetConfigurationAsync<T>(string key);
    Task SetConfigurationAsync<T>(string key, T value, string? category = null, bool requiresRestart = false);
    Task<IEnumerable<ServerConfiguration>> GetConfigurationsByCategoryAsync(string category);
    Task<bool> DeleteConfigurationAsync(string key);
    Task ReloadConfigurationAsync();
    Task<Dictionary<string, object>> GetAllConfigurationsAsync();
}

// Certificate Management
public interface ICertificateManager
{
    Task<IEnumerable<ServerCertificate>> GetCertificatesAsync();
    Task<ServerCertificate?> GetActiveCertificateAsync(CertificateUsage usage);
    Task<ServerCertificate> InstallCertificateAsync(string certificatePath, string password, CertificateUsage usage);
    Task<bool> RemoveCertificateAsync(int certificateId);
    Task<bool> SetDefaultCertificateAsync(int certificateId, CertificateUsage usage);
    Task<IEnumerable<ServerCertificate>> GetExpiringCertificatesAsync(int daysAhead = 30);
    Task ValidateCertificatesAsync();
}