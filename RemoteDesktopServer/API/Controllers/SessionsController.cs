using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using RemoteDesktopServer.Services;
using RemoteDesktopServer.Models;
using RemoteDesktopServer.API.DTOs;

namespace RemoteDesktopServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ILogger<SessionsController> _logger;
    private readonly ISessionManager _sessionManager;
    private readonly ISecurityService _securityService;

    public SessionsController(
        ILogger<SessionsController> logger,
        ISessionManager sessionManager,
        ISecurityService securityService)
    {
        _logger = logger;
        _sessionManager = sessionManager;
        _securityService = securityService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetActiveSessions()
    {
        try
        {
            var sessions = await _sessionManager.GetActiveSessionsAsync();
            var sessionDtos = sessions.Select(s => new SessionDto
            {
                SessionId = s.SessionId,
                Username = s.Username,
                ClientIP = s.ClientIP,
                StartTime = s.StartTime,
                LastActivity = s.LastActivity,
                State = s.State.ToString(),
                Type = s.Type.ToString(),
                CpuUsage = s.CpuUsage,
                MemoryUsage = s.MemoryUsageMB,
                BytesTransferred = s.BytesTransferred,
                BytesReceived = s.BytesReceived
            });

            return Ok(sessionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sessions");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{sessionId}")]
    public async Task<ActionResult<SessionDto>> GetSession(string sessionId)
    {
        try
        {
            var session = await _sessionManager.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound($"Session {sessionId} not found");
            }

            var sessionDto = new SessionDto
            {
                SessionId = session.SessionId,
                Username = session.Username,
                ClientIP = session.ClientIP,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                LastActivity = session.LastActivity,
                State = session.State.ToString(),
                Type = session.Type.ToString(),
                CpuUsage = session.CpuUsage,
                MemoryUsage = session.MemoryUsageMB,
                BytesTransferred = session.BytesTransferred,
                BytesReceived = session.BytesReceived,
                DisconnectReason = session.DisconnectReason
            };

            return Ok(sessionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{sessionId}")]
    public async Task<ActionResult> TerminateSession(string sessionId)
    {
        try
        {
            var session = await _sessionManager.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound($"Session {sessionId} not found");
            }

            await _sessionManager.EndSessionAsync(sessionId, "Terminated by administrator");
            
            await _securityService.LogSecurityEventAsync(
                "SESSION_TERMINATED",
                $"Session {sessionId} terminated by administrator",
                SecurityEventSeverity.Warning,
                session.Username,
                sessionId);

            return Ok($"Session {sessionId} terminated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{sessionId}/activities")]
    public async Task<ActionResult<IEnumerable<SessionActivityDto>>> GetSessionActivities(string sessionId)
    {
        try
        {
            var session = await _sessionManager.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound($"Session {sessionId} not found");
            }

            var activities = session.Activities.Select(a => new SessionActivityDto
            {
                Id = a.Id,
                ActivityType = a.ActivityType,
                Details = a.Details,
                Timestamp = a.Timestamp,
                ResourceAccessed = a.ResourceAccessed
            }).OrderByDescending(a => a.Timestamp);

            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities for session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("user/{username}")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetUserSessions(string username)
    {
        try
        {
            var sessions = await _sessionManager.GetUserSessionsAsync(username);
            var sessionDtos = sessions.Select(s => new SessionDto
            {
                SessionId = s.SessionId,
                Username = s.Username,
                ClientIP = s.ClientIP,
                StartTime = s.StartTime,
                LastActivity = s.LastActivity,
                State = s.State.ToString(),
                Type = s.Type.ToString(),
                CpuUsage = s.CpuUsage,
                MemoryUsage = s.MemoryUsageMB
            });

            return Ok(sessionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions for user {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("cleanup")]
    public async Task<ActionResult> CleanupExpiredSessions()
    {
        try
        {
            await _sessionManager.CleanupExpiredSessionsAsync();
            return Ok("Expired sessions cleaned up");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired sessions");
            return StatusCode(500, "Internal server error");
        }
    }
}