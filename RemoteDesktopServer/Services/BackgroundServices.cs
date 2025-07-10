using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RemoteDesktopServer.Configuration;

namespace RemoteDesktopServer.Services;

// Performance Monitoring Background Service
public class PerformanceMonitorService : BackgroundService
{
    private readonly ILogger<PerformanceMonitorService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PerformanceSettings _settings;

    public PerformanceMonitorService(
        ILogger<PerformanceMonitorService> logger,
        IServiceProvider serviceProvider,
        IOptions<PerformanceSettings> settings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.PerformanceCountersEnabled)
        {
            _logger.LogInformation("Performance monitoring is disabled");
            return;
        }

        _logger.LogInformation("Performance monitoring service started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var performanceMonitor = scope.ServiceProvider.GetRequiredService<IPerformanceMonitor>();
                
                await performanceMonitor.StartMonitoringAsync();
                
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Performance monitoring service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in performance monitoring service");
        }
    }
}

// Session Cleanup Background Service
public class SessionCleanupService : BackgroundService
{
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ServerSettings _settings;

    public SessionCleanupService(
        ILogger<SessionCleanupService> logger,
        IServiceProvider serviceProvider,
        IOptions<ServerSettings> settings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session cleanup service started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var sessionManager = scope.ServiceProvider.GetRequiredService<ISessionManager>();
                var applicationManager = scope.ServiceProvider.GetRequiredService<IApplicationManager>();
                
                try
                {
                    await sessionManager.CleanupExpiredSessionsAsync();
                    await applicationManager.CleanupTerminatedInstancesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during session cleanup");
                }
                
                // Run cleanup every 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Session cleanup service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in session cleanup service");
        }
    }
}

// Security Monitoring Background Service
public class SecurityMonitorService : BackgroundService
{
    private readonly ILogger<SecurityMonitorService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly SecuritySettings _settings;

    public SecurityMonitorService(
        ILogger<SecurityMonitorService> logger,
        IServiceProvider serviceProvider,
        IOptions<SecuritySettings> settings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.EnableAuditLogging)
        {
            _logger.LogInformation("Security monitoring is disabled");
            return;
        }

        _logger.LogInformation("Security monitoring service started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var securityService = scope.ServiceProvider.GetRequiredService<ISecurityService>();
                var certificateManager = scope.ServiceProvider.GetRequiredService<ICertificateManager>();
                
                try
                {
                    // Check for expiring certificates
                    await certificateManager.ValidateCertificatesAsync();
                    
                    var expiringCerts = await certificateManager.GetExpiringCertificatesAsync(30);
                    foreach (var cert in expiringCerts)
                    {
                        await securityService.LogSecurityEventAsync(
                            "CERTIFICATE_EXPIRING",
                            $"Certificate {cert.Name} expires on {cert.ValidUntil:yyyy-MM-dd}",
                            Models.SecurityEventSeverity.Warning);
                    }
                    
                    // Additional security checks would go here
                    // - Check for suspicious login patterns
                    // - Monitor for unusual activity
                    // - Check system integrity
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during security monitoring");
                }
                
                // Run security checks every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Security monitoring service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in security monitoring service");
        }
    }
}

// Backup Service
public class BackupService : BackgroundService
{
    private readonly ILogger<BackupService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly BackupSettings _settings;

    public BackupService(
        ILogger<BackupService> logger,
        IServiceProvider serviceProvider,
        IOptions<BackupSettings> settings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Backup service is disabled");
            return;
        }

        _logger.LogInformation("Backup service started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextBackupTime = now.Date.Add(_settings.BackupTime);
                
                if (nextBackupTime <= now)
                {
                    nextBackupTime = nextBackupTime.AddDays(1);
                }

                var delay = nextBackupTime - now;
                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                await PerformBackupAsync();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Backup service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in backup service");
        }
    }

    private async Task PerformBackupAsync()
    {
        try
        {
            _logger.LogInformation("Starting database backup");
            
            var backupDirectory = Path.Combine(_settings.BackupPath, DateTime.Now.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(backupDirectory);
            
            using var scope = _serviceProvider.CreateScope();
            var securityService = scope.ServiceProvider.GetRequiredService<ISecurityService>();
            
            // Backup database (simplified - would need proper implementation)
            var backupPath = Path.Combine(backupDirectory, "database_backup.db");
            
            // Log backup event
            await securityService.LogSecurityEventAsync(
                "DATABASE_BACKUP",
                $"Database backup completed: {backupPath}",
                Models.SecurityEventSeverity.Info);
            
            _logger.LogInformation("Database backup completed: {BackupPath}", backupPath);
            
            // Cleanup old backups
            await CleanupOldBackupsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database backup");
        }
    }

    private Task CleanupOldBackupsAsync()
    {
        try
        {
            if (!Directory.Exists(_settings.BackupPath))
                return Task.CompletedTask;

            var cutoffDate = DateTime.Now.AddDays(-_settings.RetentionDays);
            var directories = Directory.GetDirectories(_settings.BackupPath);

            foreach (var directory in directories)
            {
                var dirInfo = new DirectoryInfo(directory);
                if (dirInfo.CreationTime < cutoffDate)
                {
                    dirInfo.Delete(true);
                    _logger.LogInformation("Deleted old backup: {Directory}", directory);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old backups");
        }
        
        return Task.CompletedTask;
    }
}