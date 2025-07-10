using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RemoteDesktopServer.Configuration;

namespace RemoteDesktopServer.Services;

public class ConfigurationInitializationService : IHostedService
{
    private readonly ILogger<ConfigurationInitializationService> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly IOptionsMonitor<ServerSettings> _serverSettings;
    private readonly IOptionsMonitor<SecuritySettings> _securitySettings;

    public ConfigurationInitializationService(
        ILogger<ConfigurationInitializationService> logger,
        IConfigurationService configurationService,
        IOptionsMonitor<ServerSettings> serverSettings,
        IOptionsMonitor<SecuritySettings> securitySettings)
    {
        _logger = logger;
        _configurationService = configurationService;
        _serverSettings = serverSettings;
        _securitySettings = securitySettings;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing configuration with default values...");

        try
        {
            // Apply default values
            _configurationService.ApplyDefaults();

            // Log current configuration values (without sensitive data)
            var serverConfig = _serverSettings.CurrentValue;
            var securityConfig = _securitySettings.CurrentValue;

            _logger.LogInformation("Configuration initialized successfully");
            _logger.LogInformation("Server Name: {ServerName}", serverConfig.ServerName);
            _logger.LogInformation("Listen Port: {ListenPort}", serverConfig.ListenPort);
            _logger.LogInformation("Max Concurrent Sessions: {MaxSessions}", serverConfig.MaxConcurrentSessions);
            _logger.LogInformation("Encryption Level: {EncryptionLevel}", securityConfig.EncryptionLevel);
            _logger.LogInformation("Audit Logging: {AuditLogging}", securityConfig.EnableAuditLogging);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing configuration");
            throw;
        }

        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}