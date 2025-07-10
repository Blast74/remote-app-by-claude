using RemoteDesktopApp.Models;

namespace RemoteDesktopApp.Services;

// Connection Profile Service
public interface IConnectionProfileService
{
    Task<IEnumerable<ConnectionProfile>> GetAllAsync();
    Task<ConnectionProfile?> GetByIdAsync(int id);
    Task<ConnectionProfile> CreateAsync(ConnectionProfile profile);
    Task<ConnectionProfile> UpdateAsync(ConnectionProfile profile);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<ConnectionProfile>> GetFavoritesAsync();
    Task<IEnumerable<ConnectionProfile>> GetRecentAsync(int count = 10);
}

// Session Manager Service
public interface ISessionManagerService
{
    Task<ConnectionSession> CreateSessionAsync(ConnectionProfile profile);
    Task<bool> CloseSessionAsync(string sessionId);
    Task<IEnumerable<ConnectionSession>> GetActiveSessionsAsync();
    Task<ConnectionSession?> GetSessionAsync(string sessionId);
    event EventHandler<ConnectionSession>? SessionCreated;
    event EventHandler<string>? SessionClosed;
}

// Security Service
public interface ISecurityService
{
    Task<bool> AuthenticateUserAsync(string username, string password);
    Task<bool> ValidateTwoFactorAsync(string username, string code);
    Task LogSecurityEventAsync(string action, string details, AuditSeverity severity = AuditSeverity.Info);
    Task<bool> IsUserLockedOutAsync(string username);
    Task LockUserAsync(string username);
    Task UnlockUserAsync(string username);
}

// Performance Monitor Service
public interface IPerformanceMonitorService
{
    Task StartMonitoringAsync(string sessionId);
    Task StopMonitoringAsync(string sessionId);
    Task<PerformanceMetric> GetLatestMetricAsync(string sessionId, string metricType);
    Task<IEnumerable<PerformanceMetric>> GetMetricsAsync(string sessionId, DateTime from, DateTime to);
    event EventHandler<PerformanceMetric>? MetricUpdated;
}

// Remote Application Service
public interface IRemoteApplicationService
{
    Task<IEnumerable<RemoteApplication>> GetApplicationsAsync(int connectionProfileId);
    Task<RemoteApplication> CreateApplicationAsync(RemoteApplication app);
    Task<bool> LaunchApplicationAsync(int applicationId);
    Task<bool> DeleteApplicationAsync(int applicationId);
}

// Two Factor Auth Service
public interface ITwoFactorAuthService
{
    Task<string> GenerateSecretKeyAsync(int userProfileId);
    Task<string> GenerateQRCodeAsync(string secretKey, string username);
    Task<bool> ValidateCodeAsync(string secretKey, string code);
    Task<string[]> GenerateBackupCodesAsync();
    Task<bool> ValidateBackupCodeAsync(int userProfileId, string code);
}

// RDP Connection Service
public interface IRdpConnectionService
{
    Task<bool> ConnectAsync(ConnectionProfile profile);
    Task<bool> DisconnectAsync(string sessionId);
    Task<bool> IsConnectedAsync(string sessionId);
    event EventHandler<string>? Connected;
    event EventHandler<string>? Disconnected;
    event EventHandler<(string sessionId, string error)>? ConnectionFailed;
}

// RDP Session Service
public interface IRdpSessionService
{
    Task<object> CreateRdpControlAsync(ConnectionProfile profile);
    Task<bool> SendKeysAsync(string sessionId, string keys);
    Task<bool> SetFullScreenAsync(string sessionId, bool fullScreen);
    Task<byte[]> CaptureScreenAsync(string sessionId);
}