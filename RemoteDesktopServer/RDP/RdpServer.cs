using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RemoteDesktopServer.Configuration;
using RemoteDesktopServer.Services;
using RemoteDesktopServer.Models;

namespace RemoteDesktopServer.RDP;

public class RdpServer : BackgroundService
{
    private readonly ILogger<RdpServer> _logger;
    private readonly ServerSettings _settings;
    private readonly ISessionManager _sessionManager;
    private readonly IAuthenticationService _authService;
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly ISecurityService _securityService;
    
    private TcpListener? _tcpListener;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly List<RdpConnection> _activeConnections = new();
    private readonly object _connectionsLock = new();

    public RdpServer(
        ILogger<RdpServer> logger,
        IOptions<ServerSettings> settings,
        ISessionManager sessionManager,
        IAuthenticationService authService,
        IPerformanceMonitor performanceMonitor,
        ISecurityService securityService)
    {
        _logger = logger;
        _settings = settings.Value;
        _sessionManager = sessionManager;
        _authService = authService;
        _performanceMonitor = performanceMonitor;
        _securityService = securityService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await StartServerAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RDP Server stopped");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in RDP Server");
            throw;
        }
    }

    private async Task StartServerAsync(CancellationToken cancellationToken)
    {
        _tcpListener = new TcpListener(IPAddress.Any, _settings.ListenPort);
        _tcpListener.Start();
        
        _logger.LogInformation("RDP Server started on port {Port}", _settings.ListenPort);
        
        await _securityService.LogSecurityEventAsync(
            "SERVER_STARTED", 
            $"RDP Server started on port {_settings.ListenPort}",
            SecurityEventSeverity.Info);

        // Start monitoring
        _ = Task.Run(() => MonitorServerHealth(cancellationToken), cancellationToken);
        _ = Task.Run(() => CleanupInactiveConnections(cancellationToken), cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientConnection(tcpClient, cancellationToken), cancellationToken);
            }
            catch (ObjectDisposedException)
            {
                // Server is shutting down
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting client connection");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private async Task HandleClientConnection(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        var clientEndpoint = tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        _logger.LogInformation("New client connection from {ClientEndpoint}", clientEndpoint);

        try
        {
            // Check connection limits
            lock (_connectionsLock)
            {
                if (_activeConnections.Count >= _settings.MaxConcurrentSessions)
                {
                    _logger.LogWarning("Connection rejected from {ClientEndpoint}: Maximum sessions reached", clientEndpoint);
                    tcpClient.Close();
                    return;
                }
            }

            var connection = new RdpConnection(
                tcpClient,
                _sessionManager,
                _authService,
                _performanceMonitor,
                _securityService,
                _logger);

            lock (_connectionsLock)
            {
                _activeConnections.Add(connection);
            }

            await connection.HandleConnectionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client connection from {ClientEndpoint}", clientEndpoint);
            
            await _securityService.LogSecurityEventAsync(
                "CONNECTION_ERROR",
                $"Connection error from {clientEndpoint}: {ex.Message}",
                SecurityEventSeverity.Error);
        }
        finally
        {
            tcpClient.Close();
            
            lock (_connectionsLock)
            {
                _activeConnections.RemoveAll(c => c.TcpClient == tcpClient);
            }
            
            _logger.LogInformation("Client connection from {ClientEndpoint} closed", clientEndpoint);
        }
    }

    private async Task MonitorServerHealth(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var performance = await _performanceMonitor.GetCurrentPerformanceAsync();
                
                // Log performance metrics
                _logger.LogDebug(
                    "Server Performance - CPU: {CpuUsage:F1}%, Memory: {MemoryUsage:F1}%, Active Sessions: {ActiveSessions}",
                    performance.CpuUsagePercent,
                    performance.MemoryUsagePercent,
                    performance.ActiveSessions);

                // Check for critical thresholds
                if (performance.CpuUsagePercent > 95)
                {
                    _logger.LogWarning("Critical CPU usage: {CpuUsage:F1}%", performance.CpuUsagePercent);
                    
                    await _securityService.LogSecurityEventAsync(
                        "HIGH_CPU_USAGE",
                        $"CPU usage: {performance.CpuUsagePercent:F1}%",
                        SecurityEventSeverity.Warning);
                }

                if (performance.MemoryUsagePercent > 95)
                {
                    _logger.LogWarning("Critical memory usage: {MemoryUsage:F1}%", performance.MemoryUsagePercent);
                    
                    await _securityService.LogSecurityEventAsync(
                        "HIGH_MEMORY_USAGE",
                        $"Memory usage: {performance.MemoryUsagePercent:F1}%",
                        SecurityEventSeverity.Warning);
                }

                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in server health monitoring");
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }

    private async Task CleanupInactiveConnections(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                List<RdpConnection> connectionsToRemove;
                
                lock (_connectionsLock)
                {
                    connectionsToRemove = _activeConnections
                        .Where(c => !c.IsConnected || c.IsIdle(TimeSpan.FromMinutes(_settings.MaxIdleTimeMinutes)))
                        .ToList();
                }

                foreach (var connection in connectionsToRemove)
                {
                    try
                    {
                        await connection.DisconnectAsync("Idle timeout or connection lost");
                        
                        lock (_connectionsLock)
                        {
                            _activeConnections.Remove(connection);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error disconnecting idle connection");
                    }
                }

                if (connectionsToRemove.Count > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} inactive connections", connectionsToRemove.Count);
                }

                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in connection cleanup task");
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RDP Server...");
        
        await _securityService.LogSecurityEventAsync(
            "SERVER_STOPPING",
            "RDP Server is shutting down",
            SecurityEventSeverity.Info);

        _tcpListener?.Stop();
        _cancellationTokenSource.Cancel();

        // Disconnect all active connections
        List<RdpConnection> connections;
        lock (_connectionsLock)
        {
            connections = new List<RdpConnection>(_activeConnections);
        }

        var disconnectTasks = connections.Select(c => c.DisconnectAsync("Server shutdown"));
        await Task.WhenAll(disconnectTasks);

        await base.StopAsync(cancellationToken);
        
        _logger.LogInformation("RDP Server stopped");
    }

    public async Task<ServerStatus> GetServerStatusAsync()
    {
        var performance = await _performanceMonitor.GetCurrentPerformanceAsync();
        var activeSessions = await _sessionManager.GetActiveSessionsAsync();
        
        return new ServerStatus
        {
            IsRunning = !_cancellationTokenSource.Token.IsCancellationRequested,
            Port = _settings.ListenPort,
            ActiveConnections = _activeConnections.Count,
            ActiveSessions = activeSessions.Count(),
            MaxSessions = _settings.MaxConcurrentSessions,
            CpuUsage = performance.CpuUsagePercent,
            MemoryUsage = performance.MemoryUsagePercent,
            StartTime = DateTime.UtcNow, // Would be set in actual implementation
            Version = "1.0.0"
        };
    }

    public override void Dispose()
    {
        _cancellationTokenSource.Dispose();
        _tcpListener?.Stop();
        base.Dispose();
    }
}

public class ServerStatus
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
}