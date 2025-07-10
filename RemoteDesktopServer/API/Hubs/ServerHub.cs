using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using RemoteDesktopServer.Services;
using RemoteDesktopServer.Models;

namespace RemoteDesktopServer.API.Hubs;

[Authorize]
public class ServerHub : Hub
{
    private readonly ILogger<ServerHub> _logger;
    private readonly ISessionManager _sessionManager;
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly ISecurityService _securityService;

    public ServerHub(
        ILogger<ServerHub> logger,
        ISessionManager sessionManager,
        IPerformanceMonitor performanceMonitor,
        ISecurityService securityService)
    {
        _logger = logger;
        _sessionManager = sessionManager;
        _performanceMonitor = performanceMonitor;
        _securityService = securityService;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, "Administrators");
        
        await _securityService.LogSecurityEventAsync(
            "HUB_CONNECTION",
            $"SignalR client connected: {Context.ConnectionId}",
            SecurityEventSeverity.Info);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Administrators");
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task GetServerStatus()
    {
        try
        {
            var performance = await _performanceMonitor.GetCurrentPerformanceAsync();
            var activeSessions = await _sessionManager.GetActiveSessionsAsync();
            
            var status = new
            {
                IsRunning = true,
                ActiveSessions = activeSessions.Count(),
                ConnectedUsers = activeSessions.Select(s => s.Username).Distinct().Count(),
                CpuUsage = performance.CpuUsagePercent,
                MemoryUsage = performance.MemoryUsagePercent,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Caller.SendAsync("ServerStatusUpdate", status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting server status for client {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", "Failed to get server status");
        }
    }

    public async Task GetActiveSessions()
    {
        try
        {
            var sessions = await _sessionManager.GetActiveSessionsAsync();
            var sessionData = sessions.Select(s => new
            {
                s.SessionId,
                s.Username,
                s.ClientIP,
                s.StartTime,
                s.LastActivity,
                State = s.State.ToString(),
                Type = s.Type.ToString(),
                s.CpuUsage,
                s.MemoryUsageMB
            });

            await Clients.Caller.SendAsync("ActiveSessionsUpdate", sessionData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions for client {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", "Failed to get active sessions");
        }
    }

    public async Task TerminateSession(string sessionId)
    {
        try
        {
            var session = await _sessionManager.GetSessionAsync(sessionId);
            if (session == null)
            {
                await Clients.Caller.SendAsync("Error", $"Session {sessionId} not found");
                return;
            }

            await _sessionManager.EndSessionAsync(sessionId, "Terminated by administrator via API");
            
            await _securityService.LogSecurityEventAsync(
                "SESSION_TERMINATED_API",
                $"Session {sessionId} terminated by administrator via SignalR",
                SecurityEventSeverity.Warning,
                session.Username,
                sessionId);

            await Clients.Group("Administrators").SendAsync("SessionTerminated", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", "Failed to terminate session");
        }
    }

    public async Task StartPerformanceMonitoring()
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "PerformanceMonitoring");
            _logger.LogInformation("Client {ConnectionId} started performance monitoring", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting performance monitoring for client {ConnectionId}", Context.ConnectionId);
        }
    }

    public async Task StopPerformanceMonitoring()
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "PerformanceMonitoring");
            _logger.LogInformation("Client {ConnectionId} stopped performance monitoring", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping performance monitoring for client {ConnectionId}", Context.ConnectionId);
        }
    }
}

// Hub notification service
public class HubNotificationService
{
    private readonly IHubContext<ServerHub> _hubContext;
    private readonly ILogger<HubNotificationService> _logger;

    public HubNotificationService(
        IHubContext<ServerHub> hubContext,
        ILogger<HubNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifySessionCreated(ServerSession session)
    {
        try
        {
            var sessionData = new
            {
                session.SessionId,
                session.Username,
                session.ClientIP,
                session.StartTime,
                State = session.State.ToString(),
                Type = session.Type.ToString()
            };

            await _hubContext.Clients.Group("Administrators").SendAsync("SessionCreated", sessionData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying session created: {SessionId}", session.SessionId);
        }
    }

    public async Task NotifySessionEnded(string sessionId, string username, string reason)
    {
        try
        {
            var sessionData = new
            {
                SessionId = sessionId,
                Username = username,
                Reason = reason,
                EndTime = DateTime.UtcNow
            };

            await _hubContext.Clients.Group("Administrators").SendAsync("SessionEnded", sessionData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying session ended: {SessionId}", sessionId);
        }
    }

    public async Task NotifyPerformanceUpdate(PerformanceSnapshot performance)
    {
        try
        {
            var performanceData = new
            {
                performance.Timestamp,
                performance.CpuUsagePercent,
                performance.MemoryUsagePercent,
                performance.ActiveSessions,
                performance.ConnectedUsers,
                performance.SystemLoad
            };

            await _hubContext.Clients.Group("PerformanceMonitoring").SendAsync("PerformanceUpdate", performanceData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying performance update");
        }
    }

    public async Task NotifySecurityAlert(SecurityEvent securityEvent)
    {
        try
        {
            var alertData = new
            {
                securityEvent.EventType,
                securityEvent.Username,
                securityEvent.Details,
                securityEvent.Severity,
                securityEvent.Timestamp,
                securityEvent.SourceIP
            };

            await _hubContext.Clients.Group("Administrators").SendAsync("SecurityAlert", alertData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying security alert: {EventType}", securityEvent.EventType);
        }
    }

    public async Task NotifyServerStatusChange(string status, string message)
    {
        try
        {
            var statusData = new
            {
                Status = status,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("ServerStatusChange", statusData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying server status change: {Status}", status);
        }
    }
}