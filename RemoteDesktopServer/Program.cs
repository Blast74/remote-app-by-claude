using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RemoteDesktopServer.Configuration;
using RemoteDesktopServer.Data;
using RemoteDesktopServer.RDP;
using RemoteDesktopServer.Services;
using RemoteDesktopServer.Security;
using RemoteDesktopServer.API;

namespace RemoteDesktopServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var host = CreateHostBuilder(args).Build();
            
            // Initialize database
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ServerDbContext>();
                await context.Database.EnsureCreatedAsync();
                
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Database initialized successfully");
            }
            
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = "RemoteDesktopServer";
            })
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                
                // Configuration Services
                services.AddSingleton<IConfigurationService, ConfigurationService>();
                services.AddHostedService<ConfigurationInitializationService>();
                
                // Configuration with defaults
                services.Configure<ServerSettings>(options => 
                {
                    var configService = services.BuildServiceProvider().GetService<IConfigurationService>();
                    var settings = configService?.GetSettings<ServerSettings>("ServerSettings") ?? new ServerSettings();
                    configuration.GetSection("ServerSettings").Bind(settings);
                    
                    options.ServerName = !string.IsNullOrEmpty(settings.ServerName) ? settings.ServerName : "RDP-SERVER-01";
                    options.ListenPort = settings.ListenPort > 0 ? settings.ListenPort : 3389;
                    options.MaxConcurrentSessions = settings.MaxConcurrentSessions > 0 ? settings.MaxConcurrentSessions : 50;
                    options.SessionTimeoutMinutes = settings.SessionTimeoutMinutes > 0 ? settings.SessionTimeoutMinutes : 480;
                    options.MaxIdleTimeMinutes = settings.MaxIdleTimeMinutes > 0 ? settings.MaxIdleTimeMinutes : 30;
                    options.RecordingPath = !string.IsNullOrEmpty(settings.RecordingPath) ? settings.RecordingPath : "recordings";
                    options.TempPath = !string.IsNullOrEmpty(settings.TempPath) ? settings.TempPath : "temp";
                    options.LogsPath = !string.IsNullOrEmpty(settings.LogsPath) ? settings.LogsPath : "logs";
                    options.EnableSessionRecording = settings.EnableSessionRecording;
                });
                
                services.Configure<SecuritySettings>(options => 
                {
                    configuration.GetSection("Security").Bind(options);
                    
                    if (string.IsNullOrEmpty(options.EncryptionLevel))
                        options.EncryptionLevel = "High";
                    if (string.IsNullOrEmpty(options.EncryptionKey))
                        options.EncryptionKey = "DefaultKey123456789012345678901234";
                    if (options.AllowedAuthMethods.Length == 0)
                        options.AllowedAuthMethods = new[] { "NTLM", "Kerberos", "Certificate" };
                    if (options.FailedLoginAttempts == 0)
                        options.FailedLoginAttempts = 3;
                    if (options.LockoutDurationMinutes == 0)
                        options.LockoutDurationMinutes = 15;
                    if (string.IsNullOrEmpty(options.SessionEncryption))
                        options.SessionEncryption = "AES256";
                    if (!options.RequireNLA)
                        options.RequireNLA = true;
                    if (!options.RequireSecureRPC)
                        options.RequireSecureRPC = true;
                    if (!options.EnableAuditLogging)
                        options.EnableAuditLogging = true;
                    if (!options.PasswordComplexity)
                        options.PasswordComplexity = true;
                });
                
                services.Configure<ActiveDirectorySettings>(options => 
                {
                    configuration.GetSection("ActiveDirectory").Bind(options);
                    if (options.AuthenticationTimeout == 0)
                        options.AuthenticationTimeout = 30;
                    if (!options.UseSSL)
                        options.UseSSL = true;
                });
                
                services.Configure<PerformanceSettings>(options => 
                {
                    configuration.GetSection("Performance").Bind(options);
                    if (options.MonitoringInterval == 0)
                        options.MonitoringInterval = 5000;
                    if (options.MaxCpuUsage == 0)
                        options.MaxCpuUsage = 85.0;
                    if (options.MaxMemoryUsage == 0)
                        options.MaxMemoryUsage = 90.0;
                    if (options.MaxDiskUsage == 0)
                        options.MaxDiskUsage = 95.0;
                    if (!options.PerformanceCountersEnabled)
                        options.PerformanceCountersEnabled = true;
                    
                    if (options.AlertThresholds.CpuCritical == 0)
                        options.AlertThresholds.CpuCritical = 95.0;
                    if (options.AlertThresholds.MemoryCritical == 0)
                        options.AlertThresholds.MemoryCritical = 95.0;
                    if (options.AlertThresholds.DiskCritical == 0)
                        options.AlertThresholds.DiskCritical = 98.0;
                });
                
                services.Configure<RemoteAppSettings>(options => 
                {
                    configuration.GetSection("RemoteApp").Bind(options);
                    if (string.IsNullOrEmpty(options.ApplicationsPath))
                        options.ApplicationsPath = "applications";
                    if (string.IsNullOrEmpty(options.IconsPath))
                        options.IconsPath = "icons";
                    if (options.MaxApplications == 0)
                        options.MaxApplications = 100;
                    if (options.ApplicationTimeout == 0)
                        options.ApplicationTimeout = 300;
                    if (!options.Enabled)
                        options.Enabled = true;
                });
                
                services.Configure<LoadBalancingSettings>(options => 
                {
                    configuration.GetSection("LoadBalancing").Bind(options);
                    if (string.IsNullOrEmpty(options.LoadBalanceMethod))
                        options.LoadBalanceMethod = "RoundRobin";
                    if (options.HealthCheckInterval == 0)
                        options.HealthCheckInterval = 30;
                });
                
                services.Configure<ApiSettings>(options => 
                {
                    configuration.GetSection("API").Bind(options);
                    if (options.Port == 0)
                        options.Port = 8443;
                    if (string.IsNullOrEmpty(options.JwtSecret))
                        options.JwtSecret = "RemoteDesktopServer2024SuperSecretKey!@#$%^&*";
                    if (options.JwtExpirationHours == 0)
                        options.JwtExpirationHours = 24;
                    if (!options.Enabled)
                        options.Enabled = true;
                    if (!options.UseHttps)
                        options.UseHttps = true;
                    
                    if (options.RateLimiting.RequestsPerMinute == 0)
                        options.RateLimiting.RequestsPerMinute = 100;
                    if (options.RateLimiting.BurstSize == 0)
                        options.RateLimiting.BurstSize = 10;
                });
                
                services.Configure<ClusteringSettings>(options => 
                {
                    configuration.GetSection("Clustering").Bind(options);
                    if (string.IsNullOrEmpty(options.NodeId))
                        options.NodeId = "NODE-01";
                    if (options.HeartbeatInterval == 0)
                        options.HeartbeatInterval = 10;
                    if (options.ElectionTimeout == 0)
                        options.ElectionTimeout = 5000;
                });
                
                services.Configure<BackupSettings>(options => 
                {
                    configuration.GetSection("Backup").Bind(options);
                    if (string.IsNullOrEmpty(options.BackupPath))
                        options.BackupPath = "backups";
                    if (options.RetentionDays == 0)
                        options.RetentionDays = 30;
                    if (string.IsNullOrEmpty(options.BackupInterval))
                        options.BackupInterval = "Daily";
                    if (options.BackupTime == TimeSpan.Zero)
                        options.BackupTime = TimeSpan.FromHours(2);
                    if (!options.Enabled)
                        options.Enabled = true;
                });
                
                // Database
                services.AddDbContext<ServerDbContext>(options =>
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    if (connectionString?.Contains("localdb") == true)
                    {
                        options.UseSqlServer(connectionString);
                    }
                    else
                    {
                        options.UseSqlite(configuration.GetConnectionString("SQLiteConnection"));
                    }
                });
                
                // Core Services
                services.AddSingleton<IEncryptionService, EncryptionService>();
                services.AddScoped<ISessionManager, SessionManager>();
                services.AddScoped<IAuthenticationService, AuthenticationService>();
                services.AddScoped<ISecurityService, SecurityService>();
                services.AddScoped<IPerformanceMonitor, PerformanceMonitor>();
                services.AddScoped<IApplicationManager, ApplicationManager>();
                services.AddScoped<Services.IConfigurationManager, Services.ConfigurationManager>();
                services.AddScoped<ICertificateManager, CertificateManager>();
                
                // Background Services
                services.AddHostedService<RdpServer>();
                services.AddHostedService<PerformanceMonitorService>();
                services.AddHostedService<SessionCleanupService>();
                services.AddHostedService<SecurityMonitorService>();
                
                // API Services (if enabled)
                var apiSettings = configuration.GetSection("API").Get<ApiSettings>();
                if (apiSettings?.Enabled == true)
                {
                    services.AddHostedService<ApiServer>();
                }
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.AddEventLog();
                
                // Add file logging
                logging.AddProvider(new FileLoggerProvider("logs"));
            });
}