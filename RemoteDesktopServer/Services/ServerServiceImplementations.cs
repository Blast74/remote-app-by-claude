using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RemoteDesktopServer.Configuration;
using RemoteDesktopServer.Data;
using RemoteDesktopServer.Models;
using RemoteDesktopServer.Security;
using System.Diagnostics;
using System.Management;

namespace RemoteDesktopServer.Services;

// Performance Monitor Implementation
public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly ServerDbContext _context;
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly PerformanceSettings _settings;
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;
    private bool _isRunning = false;

    public event EventHandler<PerformanceAlert>? PerformanceAlertRaised;

    public PerformanceMonitor(
        ServerDbContext context,
        ILogger<PerformanceMonitor> logger,
        IOptions<PerformanceSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
    }

    public async Task<PerformanceSnapshot> GetCurrentPerformanceAsync()
    {
        try
        {
            var cpuUsage = _cpuCounter.NextValue();
            var availableMemory = _memoryCounter.NextValue();
            var totalMemory = GetTotalMemoryMB();
            var memoryUsage = ((totalMemory - availableMemory) / totalMemory) * 100;

            var activeSessions = await _context.Sessions
                .CountAsync(s => s.State == SessionState.Connected);

            var connectedUsers = await _context.Sessions
                .Where(s => s.State == SessionState.Connected)
                .Select(s => s.Username)
                .Distinct()
                .CountAsync();

            var snapshot = new PerformanceSnapshot
            {
                CpuUsagePercent = cpuUsage,
                MemoryUsagePercent = memoryUsage,
                MemoryUsageMB = (long)(totalMemory - availableMemory),
                ActiveSessions = activeSessions,
                ConnectedUsers = connectedUsers,
                SystemLoad = (cpuUsage + memoryUsage) / 2
            };

            return snapshot;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current performance");
            throw;
        }
    }

    public async Task<PerformanceSnapshot> GetSessionPerformanceAsync(string sessionId)
    {
        // For session-specific performance, we'd need to implement process monitoring
        // This is a simplified implementation
        var generalPerf = await GetCurrentPerformanceAsync();
        generalPerf.SessionId = await _context.Sessions
            .Where(s => s.SessionId == sessionId)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();
        
        return generalPerf;
    }

    public async Task StartMonitoringAsync()
    {
        _isRunning = true;
        _logger.LogInformation("Performance monitoring started");
        
        while (_isRunning)
        {
            try
            {
                var snapshot = await GetCurrentPerformanceAsync();
                
                // Save to database
                _context.PerformanceSnapshots.Add(snapshot);
                await _context.SaveChangesAsync();
                
                // Check for alerts
                CheckPerformanceAlerts(snapshot);
                
                await Task.Delay(_settings.MonitoringInterval);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in performance monitoring loop");
                await Task.Delay(5000); // Wait before retrying
            }
        }
    }

    public Task StopMonitoringAsync()
    {
        _isRunning = false;
        _logger.LogInformation("Performance monitoring stopped");
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<PerformanceSnapshot>> GetPerformanceHistoryAsync(DateTime from, DateTime to)
    {
        return await _context.PerformanceSnapshots
            .Where(p => p.Timestamp >= from && p.Timestamp <= to)
            .OrderBy(p => p.Timestamp)
            .ToListAsync();
    }

    private void CheckPerformanceAlerts(PerformanceSnapshot snapshot)
    {
        if (snapshot.CpuUsagePercent > _settings.AlertThresholds.CpuCritical)
        {
            PerformanceAlertRaised?.Invoke(this, new PerformanceAlert
            {
                AlertType = "CPU_CRITICAL",
                Message = $"CPU usage is critically high: {snapshot.CpuUsagePercent:F1}%",
                Value = snapshot.CpuUsagePercent,
                Threshold = _settings.AlertThresholds.CpuCritical
            });
        }

        if (snapshot.MemoryUsagePercent > _settings.AlertThresholds.MemoryCritical)
        {
            PerformanceAlertRaised?.Invoke(this, new PerformanceAlert
            {
                AlertType = "MEMORY_CRITICAL",
                Message = $"Memory usage is critically high: {snapshot.MemoryUsagePercent:F1}%",
                Value = snapshot.MemoryUsagePercent,
                Threshold = _settings.AlertThresholds.MemoryCritical
            });
        }
    }

    private long GetTotalMemoryMB()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (var obj in searcher.Get())
            {
                return Convert.ToInt64(obj["TotalPhysicalMemory"]) / (1024 * 1024);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total memory");
        }
        return 8192; // Default fallback
    }

    public void Dispose()
    {
        _cpuCounter?.Dispose();
        _memoryCounter?.Dispose();
    }
}

// Security Service Implementation
public class SecurityService : ISecurityService
{
    private readonly ServerDbContext _context;
    private readonly ILogger<SecurityService> _logger;
    private readonly SecuritySettings _settings;

    public SecurityService(
        ServerDbContext context,
        ILogger<SecurityService> logger,
        IOptions<SecuritySettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task LogSecurityEventAsync(string eventType, string details, SecurityEventSeverity severity, string? username = null, string? sessionId = null)
    {
        try
        {
            var securityEvent = new SecurityEvent
            {
                EventType = eventType,
                Details = details,
                Severity = severity,
                Username = username,
                SessionId = sessionId,
                Timestamp = DateTime.UtcNow
            };

            if (username != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                securityEvent.UserId = user?.Id;
            }

            _context.SecurityEvents.Add(securityEvent);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging security event: {EventType}", eventType);
        }
    }

    public async Task<IEnumerable<SecurityEvent>> GetSecurityEventsAsync(DateTime from, DateTime to)
    {
        return await _context.SecurityEvents
            .Where(e => e.Timestamp >= from && e.Timestamp <= to)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<SecurityEvent>> GetUserSecurityEventsAsync(string username, DateTime from, DateTime to)
    {
        return await _context.SecurityEvents
            .Where(e => e.Username == username && e.Timestamp >= from && e.Timestamp <= to)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public Task<bool> IsIPAddressBlockedAsync(string ipAddress)
    {
        // Implementation would check against IP blocking list
        return Task.FromResult(false);
    }

    public async Task BlockIPAddressAsync(string ipAddress, string reason, TimeSpan? duration = null)
    {
        await LogSecurityEventAsync(
            "IP_BLOCKED",
            $"IP address {ipAddress} blocked: {reason}",
            SecurityEventSeverity.Warning);
    }

    public async Task UnblockIPAddressAsync(string ipAddress)
    {
        await LogSecurityEventAsync(
            "IP_UNBLOCKED",
            $"IP address {ipAddress} unblocked",
            SecurityEventSeverity.Info);
    }

    public async Task<SecurityReport> GenerateSecurityReportAsync(DateTime from, DateTime to)
    {
        var events = await GetSecurityEventsAsync(from, to);
        var eventsList = events.ToList();

        return new SecurityReport
        {
            TotalEvents = eventsList.Count,
            FailedLogins = eventsList.Count(e => e.EventType == "AUTHENTICATION_FAILED"),
            SuccessfulLogins = eventsList.Count(e => e.EventType == "AUTHENTICATION_SUCCESS"),
            TopFailedUsers = eventsList
                .Where(e => e.EventType == "AUTHENTICATION_FAILED")
                .GroupBy(e => e.Username)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key ?? "Unknown")
                .ToList(),
            TopSourceIPs = eventsList
                .Where(e => e.SourceIP != null)
                .GroupBy(e => e.SourceIP)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key ?? "Unknown")
                .ToList()
        };
    }
}

// Application Manager Implementation
public class ApplicationManager : IApplicationManager
{
    private readonly ServerDbContext _context;
    private readonly ILogger<ApplicationManager> _logger;
    private readonly RemoteAppSettings _settings;

    public ApplicationManager(
        ServerDbContext context,
        ILogger<ApplicationManager> logger,
        IOptions<RemoteAppSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<IEnumerable<PublishedApplication>> GetPublishedApplicationsAsync(bool enabledOnly = true)
    {
        var query = _context.PublishedApplications.AsQueryable();
        
        if (enabledOnly)
        {
            query = query.Where(a => a.IsEnabled);
        }
        
        return await query.OrderBy(a => a.Name).ToListAsync();
    }

    public async Task<PublishedApplication?> GetApplicationAsync(int applicationId)
    {
        return await _context.PublishedApplications
            .Include(a => a.Instances)
            .Include(a => a.LaunchLogs)
            .FirstOrDefaultAsync(a => a.Id == applicationId);
    }

    public async Task<IEnumerable<PublishedApplication>> GetUserApplicationsAsync(string username)
    {
        // This would check user permissions - simplified implementation
        return await GetPublishedApplicationsAsync(true);
    }

    public async Task<PublishedApplication> CreateApplicationAsync(PublishedApplication application)
    {
        _context.PublishedApplications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<PublishedApplication> UpdateApplicationAsync(PublishedApplication application)
    {
        application.UpdatedAt = DateTime.UtcNow;
        _context.PublishedApplications.Update(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<bool> DeleteApplicationAsync(int applicationId)
    {
        var application = await GetApplicationAsync(applicationId);
        if (application == null) return false;

        _context.PublishedApplications.Remove(application);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ApplicationInstance> LaunchApplicationAsync(int applicationId, string sessionId)
    {
        var application = await GetApplicationAsync(applicationId);
        if (application == null)
        {
            throw new ArgumentException($"Application {applicationId} not found");
        }

        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
        if (session == null)
        {
            throw new ArgumentException($"Session {sessionId} not found");
        }

        // Create application instance
        var instance = new ApplicationInstance
        {
            ApplicationId = applicationId,
            SessionId = session.Id,
            ProcessId = 0, // Would be set after launching actual process
            StartTime = DateTime.UtcNow,
            IsRunning = true
        };

        _context.ApplicationInstances.Add(instance);
        
        // Log launch
        var launchLog = new ApplicationLaunchLog
        {
            ApplicationId = applicationId,
            UserId = session.UserId,
            SessionId = session.Id,
            LaunchTime = DateTime.UtcNow,
            Success = true,
            ClientIP = session.ClientIP
        };

        _context.ApplicationLaunchLogs.Add(launchLog);
        
        await _context.SaveChangesAsync();

        return instance;
    }

    public async Task<bool> TerminateApplicationInstanceAsync(string instanceId)
    {
        var instance = await _context.ApplicationInstances
            .FirstOrDefaultAsync(i => i.InstanceId == instanceId);
        
        if (instance == null) return false;

        instance.IsRunning = false;
        instance.EndTime = DateTime.UtcNow;
        instance.ExitReason = "Terminated";
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ApplicationInstance>> GetRunningInstancesAsync()
    {
        return await _context.ApplicationInstances
            .Include(i => i.Application)
            .Include(i => i.Session)
            .Where(i => i.IsRunning)
            .ToListAsync();
    }

    public async Task CleanupTerminatedInstancesAsync()
    {
        var oldInstances = await _context.ApplicationInstances
            .Where(i => !i.IsRunning && i.EndTime < DateTime.UtcNow.AddDays(-7))
            .ToListAsync();

        _context.ApplicationInstances.RemoveRange(oldInstances);
        await _context.SaveChangesAsync();
    }
}

// Configuration Manager stub
public class ConfigurationManager : IConfigurationManager
{
    private readonly ServerDbContext _context;
    private readonly ILogger<ConfigurationManager> _logger;

    public ConfigurationManager(ServerDbContext context, ILogger<ConfigurationManager> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<T?> GetConfigurationAsync<T>(string key)
    {
        var config = await _context.ServerConfigurations.FirstOrDefaultAsync(c => c.Key == key);
        if (config == null) return default(T);
        
        // Simple type conversion - would need more robust implementation
        return (T)Convert.ChangeType(config.Value, typeof(T));
    }

    public async Task SetConfigurationAsync<T>(string key, T value, string? category = null, bool requiresRestart = false)
    {
        var config = await _context.ServerConfigurations.FirstOrDefaultAsync(c => c.Key == key);
        
        if (config == null)
        {
            config = new ServerConfiguration
            {
                Key = key,
                Value = value?.ToString() ?? "",
                Category = category,
                RequiresRestart = requiresRestart
            };
            _context.ServerConfigurations.Add(config);
        }
        else
        {
            config.Value = value?.ToString() ?? "";
            config.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ServerConfiguration>> GetConfigurationsByCategoryAsync(string category)
    {
        return await _context.ServerConfigurations
            .Where(c => c.Category == category)
            .ToListAsync();
    }

    public async Task<bool> DeleteConfigurationAsync(string key)
    {
        var config = await _context.ServerConfigurations.FirstOrDefaultAsync(c => c.Key == key);
        if (config == null) return false;
        
        _context.ServerConfigurations.Remove(config);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task ReloadConfigurationAsync() => Task.CompletedTask;

    public async Task<Dictionary<string, object>> GetAllConfigurationsAsync()
    {
        var configs = await _context.ServerConfigurations.ToListAsync();
        return configs.ToDictionary(c => c.Key, c => (object)c.Value);
    }
}

// Certificate Manager stub
public class CertificateManager : ICertificateManager
{
    private readonly ServerDbContext _context;
    private readonly ILogger<CertificateManager> _logger;

    public CertificateManager(ServerDbContext context, ILogger<CertificateManager> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ServerCertificate>> GetCertificatesAsync()
    {
        return await _context.ServerCertificates.ToListAsync();
    }

    public async Task<ServerCertificate?> GetActiveCertificateAsync(CertificateUsage usage)
    {
        return await _context.ServerCertificates
            .FirstOrDefaultAsync(c => c.Usage == usage && c.IsActive && !c.IsExpired);
    }

    public async Task<ServerCertificate> InstallCertificateAsync(string certificatePath, string password, CertificateUsage usage)
    {
        // Implementation would actually install the certificate
        var certificate = new ServerCertificate
        {
            Name = Path.GetFileName(certificatePath),
            CertificatePath = certificatePath,
            Usage = usage,
            IsActive = true,
            ValidFrom = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddYears(1) // Would read from actual certificate
        };
        
        _context.ServerCertificates.Add(certificate);
        await _context.SaveChangesAsync();
        return certificate;
    }

    public async Task<bool> RemoveCertificateAsync(int certificateId)
    {
        var certificate = await _context.ServerCertificates.FindAsync(certificateId);
        if (certificate == null) return false;
        
        _context.ServerCertificates.Remove(certificate);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetDefaultCertificateAsync(int certificateId, CertificateUsage usage)
    {
        var certificate = await _context.ServerCertificates.FindAsync(certificateId);
        if (certificate == null) return false;
        
        // Remove default from other certificates
        var others = await _context.ServerCertificates
            .Where(c => c.Usage == usage && c.IsDefault)
            .ToListAsync();
        
        foreach (var other in others)
        {
            other.IsDefault = false;
        }
        
        certificate.IsDefault = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ServerCertificate>> GetExpiringCertificatesAsync(int daysAhead = 30)
    {
        var expirationDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.ServerCertificates
            .Where(c => c.ValidUntil <= expirationDate && c.IsActive)
            .ToListAsync();
    }

    public async Task ValidateCertificatesAsync()
    {
        var certificates = await GetCertificatesAsync();
        foreach (var cert in certificates)
        {
            if (cert.IsExpired)
            {
                cert.IsActive = false;
                _logger.LogWarning("Certificate {Name} has expired", cert.Name);
            }
        }
        await _context.SaveChangesAsync();
    }
}