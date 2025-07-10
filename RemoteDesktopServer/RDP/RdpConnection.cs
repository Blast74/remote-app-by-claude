using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using RemoteDesktopServer.Models;
using RemoteDesktopServer.Services;

namespace RemoteDesktopServer.RDP;

public class RdpConnection
{
    public TcpClient TcpClient { get; }
    public string ConnectionId { get; }
    public bool IsConnected => TcpClient.Connected;
    public DateTime LastActivity { get; private set; } = DateTime.UtcNow;
    public string? SessionId { get; private set; }
    public string? Username { get; private set; }
    public string ClientIP { get; }

    private readonly ISessionManager _sessionManager;
    private readonly IAuthenticationService _authService;
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly ISecurityService _securityService;
    private readonly ILogger _logger;
    
    private NetworkStream? _stream;
    private bool _isAuthenticated = false;
    private ServerSession? _currentSession;
    private readonly object _lockObject = new();

    public RdpConnection(
        TcpClient tcpClient,
        ISessionManager sessionManager,
        IAuthenticationService authService,
        IPerformanceMonitor performanceMonitor,
        ISecurityService securityService,
        ILogger logger)
    {
        TcpClient = tcpClient;
        ConnectionId = Guid.NewGuid().ToString();
        ClientIP = tcpClient.Client.RemoteEndPoint?.ToString()?.Split(':')[0] ?? "Unknown";
        
        _sessionManager = sessionManager;
        _authService = authService;
        _performanceMonitor = performanceMonitor;
        _securityService = securityService;
        _logger = logger;
        
        _stream = tcpClient.GetStream();
    }

    public async Task HandleConnectionAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling RDP connection {ConnectionId} from {ClientIP}", ConnectionId, ClientIP);
        
        try
        {
            await _securityService.LogSecurityEventAsync(
                "CONNECTION_ATTEMPT",
                $"Client connection from {ClientIP}",
                SecurityEventSeverity.Info);

            // RDP Protocol Handshake
            if (!await PerformRdpHandshakeAsync(cancellationToken))
            {
                _logger.LogWarning("RDP handshake failed for connection {ConnectionId}", ConnectionId);
                return;
            }

            // Authentication
            if (!await AuthenticateUserAsync(cancellationToken))
            {
                _logger.LogWarning("Authentication failed for connection {ConnectionId}", ConnectionId);
                
                await _securityService.LogSecurityEventAsync(
                    "AUTHENTICATION_FAILED",
                    $"Authentication failed from {ClientIP}",
                    SecurityEventSeverity.Warning);
                return;
            }

            // Create session
            _currentSession = await _sessionManager.CreateSessionAsync(new SessionRequest
            {
                Username = Username!,
                ClientIP = ClientIP,
                SessionType = SessionType.Desktop,
                ConnectionId = ConnectionId
            });

            SessionId = _currentSession.SessionId;
            
            _logger.LogInformation(
                "Session {SessionId} created for user {Username} from {ClientIP}",
                SessionId, Username, ClientIP);

            await _securityService.LogSecurityEventAsync(
                "SESSION_CREATED",
                $"Session {SessionId} created for user {Username}",
                SecurityEventSeverity.Info);

            // Start session monitoring
            _ = Task.Run(() => MonitorSessionPerformance(cancellationToken), cancellationToken);

            // Handle RDP protocol messages
            await HandleRdpProtocolAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling RDP connection {ConnectionId}", ConnectionId);
            
            await _securityService.LogSecurityEventAsync(
                "CONNECTION_ERROR",
                $"Connection error for {ConnectionId}: {ex.Message}",
                SecurityEventSeverity.Error);
        }
        finally
        {
            await CleanupConnectionAsync();
        }
    }

    private async Task<bool> PerformRdpHandshakeAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_stream == null) return false;

            // Simplified RDP handshake - in real implementation would handle full RDP protocol
            var buffer = new byte[1024];
            
            // Read client connection request
            var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            if (bytesRead == 0) return false;

            _logger.LogDebug("Received {BytesRead} bytes in RDP handshake from {ConnectionId}", bytesRead, ConnectionId);

            // Send server response (simplified)
            var response = Encoding.UTF8.GetBytes("RDP_SERVER_READY");
            await _stream.WriteAsync(response, 0, response.Length, cancellationToken);
            await _stream.FlushAsync(cancellationToken);

            UpdateLastActivity();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RDP handshake for connection {ConnectionId}", ConnectionId);
            return false;
        }
    }

    private async Task<bool> AuthenticateUserAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_stream == null) return false;

            // Read authentication credentials (simplified)
            var buffer = new byte[1024];
            var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            
            if (bytesRead == 0) return false;

            // Parse credentials (in real implementation would handle proper RDP auth)
            var authData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var parts = authData.Split('|');
            
            if (parts.Length < 2) return false;

            var username = parts[0];
            var password = parts[1];
            var domain = parts.Length > 2 ? parts[2] : "LOCAL";

            // Authenticate with authentication service
            var authResult = await _authService.AuthenticateAsync(username, password, domain, ClientIP);
            
            if (authResult.Success)
            {
                _isAuthenticated = true;
                Username = username;
                
                // Send success response
                var response = Encoding.UTF8.GetBytes("AUTH_SUCCESS");
                await _stream.WriteAsync(response, 0, response.Length, cancellationToken);
                
                _logger.LogInformation(
                    "User {Username} authenticated successfully from {ClientIP}",
                    username, ClientIP);
                
                await _securityService.LogSecurityEventAsync(
                    "AUTHENTICATION_SUCCESS",
                    $"User {username} authenticated from {ClientIP}",
                    SecurityEventSeverity.Info);
                
                UpdateLastActivity();
                return true;
            }
            else
            {
                // Send failure response
                var response = Encoding.UTF8.GetBytes($"AUTH_FAILED:{authResult.ErrorMessage}");
                await _stream.WriteAsync(response, 0, response.Length, cancellationToken);
                
                _logger.LogWarning(
                    "Authentication failed for user {Username} from {ClientIP}: {ErrorMessage}",
                    username, ClientIP, authResult.ErrorMessage);
                
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in authentication for connection {ConnectionId}", ConnectionId);
            return false;
        }
    }

    private async Task HandleRdpProtocolAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected && _isAuthenticated)
            {
                if (_stream == null) break;

                var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                
                if (bytesRead == 0)
                {
                    _logger.LogInformation("Client disconnected: {ConnectionId}", ConnectionId);
                    break;
                }

                UpdateLastActivity();

                // Process RDP protocol messages (simplified)
                await ProcessRdpMessage(buffer, bytesRead, cancellationToken);

                // Update session statistics
                if (_currentSession != null)
                {
                    _currentSession.BytesReceived += bytesRead;
                    await _sessionManager.UpdateSessionAsync(_currentSession);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RDP protocol handling for connection {ConnectionId}", ConnectionId);
        }
    }

    private async Task ProcessRdpMessage(byte[] buffer, int length, CancellationToken cancellationToken)
    {
        try
        {
            // Simplified RDP message processing
            var messageType = buffer[0];
            
            switch (messageType)
            {
                case 0x01: // Desktop request
                    await HandleDesktopRequest(buffer, length, cancellationToken);
                    break;
                    
                case 0x02: // Input event
                    await HandleInputEvent(buffer, length, cancellationToken);
                    break;
                    
                case 0x03: // Application launch
                    await HandleApplicationLaunch(buffer, length, cancellationToken);
                    break;
                    
                default:
                    _logger.LogDebug("Unknown RDP message type: {MessageType}", messageType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RDP message for connection {ConnectionId}", ConnectionId);
        }
    }

    private async Task HandleDesktopRequest(byte[] buffer, int length, CancellationToken cancellationToken)
    {
        // Send desktop frame (simplified)
        if (_stream != null)
        {
            var response = Encoding.UTF8.GetBytes("DESKTOP_FRAME_DATA");
            await _stream.WriteAsync(response, 0, response.Length, cancellationToken);
            
            if (_currentSession != null)
            {
                _currentSession.BytesTransferred += response.Length;
            }
        }
    }

    private async Task HandleInputEvent(byte[] buffer, int length, CancellationToken cancellationToken)
    {
        // Process keyboard/mouse input (simplified)
        _logger.LogDebug("Processing input event for session {SessionId}", SessionId);
        
        if (_currentSession != null)
        {
            await _sessionManager.LogSessionActivityAsync(_currentSession.SessionId, "INPUT_EVENT", "User input received");
        }
    }

    private async Task HandleApplicationLaunch(byte[] buffer, int length, CancellationToken cancellationToken)
    {
        // Handle RemoteApp launch (simplified)
        var appName = Encoding.UTF8.GetString(buffer, 1, length - 1);
        
        _logger.LogInformation("Application launch requested: {AppName} for session {SessionId}", appName, SessionId);
        
        if (_currentSession != null)
        {
            await _sessionManager.LogSessionActivityAsync(_currentSession.SessionId, "APP_LAUNCH", $"Application: {appName}");
        }
    }

    private async Task MonitorSessionPerformance(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && IsConnected && _isAuthenticated)
        {
            try
            {
                if (_currentSession != null)
                {
                    var performance = await _performanceMonitor.GetSessionPerformanceAsync(_currentSession.SessionId);
                    
                    _currentSession.CpuUsage = (float)performance.CpuUsagePercent;
                    _currentSession.MemoryUsageMB = (float)performance.MemoryUsageMB;
                    _currentSession.AverageLatency = performance.AverageResponseTime;
                    
                    await _sessionManager.UpdateSessionAsync(_currentSession);
                }
                
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring session performance for {SessionId}", SessionId);
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }

    public bool IsIdle(TimeSpan maxIdleTime)
    {
        return DateTime.UtcNow - LastActivity > maxIdleTime;
    }

    public async Task DisconnectAsync(string reason)
    {
        _logger.LogInformation("Disconnecting connection {ConnectionId}: {Reason}", ConnectionId, reason);
        
        if (_currentSession != null)
        {
            await _sessionManager.EndSessionAsync(_currentSession.SessionId, reason);
            
            await _securityService.LogSecurityEventAsync(
                "SESSION_ENDED",
                $"Session {SessionId} ended: {reason}",
                SecurityEventSeverity.Info);
        }
        
        await CleanupConnectionAsync();
    }

    private async Task CleanupConnectionAsync()
    {
        try
        {
            if (_currentSession != null)
            {
                await _sessionManager.EndSessionAsync(_currentSession.SessionId, "Connection closed");
            }
            
            _stream?.Close();
            TcpClient.Close();
            
            _logger.LogInformation("Connection {ConnectionId} cleaned up", ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up connection {ConnectionId}", ConnectionId);
        }
    }

    private void UpdateLastActivity()
    {
        lock (_lockObject)
        {
            LastActivity = DateTime.UtcNow;
        }
    }
}