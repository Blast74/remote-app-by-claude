using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RemoteDesktopServer.Configuration;
using RemoteDesktopServer.Data;
using RemoteDesktopServer.Models;

namespace RemoteDesktopServer.Services;

public class SessionManager : ISessionManager
{
    private readonly ServerDbContext _context;
    private readonly ILogger<SessionManager> _logger;
    private readonly ServerSettings _settings;
    private readonly ISecurityService _securityService;

    public SessionManager(
        ServerDbContext context,
        ILogger<SessionManager> logger,
        IOptions<ServerSettings> settings,
        ISecurityService securityService)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
        _securityService = securityService;
    }

    public async Task<ServerSession> CreateSessionAsync(SessionRequest request)
    {
        try
        {
            // Check if user can create a session
            if (!await CanUserCreateSessionAsync(request.Username))
            {
                throw new InvalidOperationException("User has reached maximum concurrent sessions limit");
            }

            // Get user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null)
            {
                throw new InvalidOperationException("User not found or inactive");
            }

            // Create session
            var session = new ServerSession
            {
                SessionId = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Username = request.Username,
                ClientIP = request.ClientIP,
                State = SessionState.Connecting,
                Type = request.SessionType,
                DesktopWidth = request.DesktopWidth,
                DesktopHeight = request.DesktopHeight,
                ColorDepth = request.ColorDepth,
                StartTime = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            // Log session creation
            await _securityService.LogSecurityEventAsync(
                "SESSION_CREATED",
                $"Session {session.SessionId} created for user {request.Username}",
                SecurityEventSeverity.Info,
                request.Username,
                session.SessionId);

            _logger.LogInformation(
                "Session {SessionId} created for user {Username} from {ClientIP}",
                session.SessionId, request.Username, request.ClientIP);

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user {Username}", request.Username);
            throw;
        }
    }

    public async Task<ServerSession?> GetSessionAsync(string sessionId)
    {
        return await _context.Sessions
            .Include(s => s.User)
            .Include(s => s.Activities)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<IEnumerable<ServerSession>> GetActiveSessionsAsync()
    {
        return await _context.Sessions
            .Include(s => s.User)
            .Where(s => s.State == SessionState.Connected)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServerSession>> GetUserSessionsAsync(string username)
    {
        return await _context.Sessions
            .Include(s => s.User)
            .Where(s => s.Username == username && s.State == SessionState.Connected)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task UpdateSessionAsync(ServerSession session)
    {
        try
        {
            session.LastActivity = DateTime.UtcNow;
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId}", session.SessionId);
            throw;
        }
    }

    public async Task EndSessionAsync(string sessionId, string reason)
    {
        try
        {
            var session = await GetSessionAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("Attempted to end non-existent session {SessionId}", sessionId);
                return;
            }

            session.EndTime = DateTime.UtcNow;
            session.State = SessionState.Disconnected;
            session.DisconnectReason = reason;

            await UpdateSessionAsync(session);

            // Log session end
            await _securityService.LogSecurityEventAsync(
                "SESSION_ENDED",
                $"Session {sessionId} ended: {reason}",
                SecurityEventSeverity.Info,
                session.Username,
                sessionId);

            _logger.LogInformation(
                "Session {SessionId} ended for user {Username}: {Reason}",
                sessionId, session.Username, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task LogSessionActivityAsync(string sessionId, string activityType, string details)
    {
        try
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null)
            {
                _logger.LogWarning("Attempted to log activity for non-existent session {SessionId}", sessionId);
                return;
            }

            var activity = new SessionActivity
            {
                SessionId = session.Id,
                ActivityType = activityType,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.SessionActivities.Add(activity);
            await _context.SaveChangesAsync();

            // Update session last activity
            session.LastActivity = DateTime.UtcNow;
            await UpdateSessionAsync(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging session activity for {SessionId}", sessionId);
        }
    }

    public async Task<bool> CanUserCreateSessionAsync(string username)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null || user.IsCurrentlyLocked)
            {
                return false;
            }

            // Check user's concurrent session limit
            var activeSessions = await _context.Sessions
                .CountAsync(s => s.Username == username && s.State == SessionState.Connected);

            if (activeSessions >= user.MaxConcurrentSessions)
            {
                return false;
            }

            // Check server's global session limit
            var totalActiveSessions = await _context.Sessions
                .CountAsync(s => s.State == SessionState.Connected);

            if (totalActiveSessions >= _settings.MaxConcurrentSessions)
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {Username} can create session", username);
            return false;
        }
    }

    public async Task CleanupExpiredSessionsAsync()
    {
        try
        {
            var expiredSessions = await _context.Sessions
                .Where(s => s.State == SessionState.Connected &&
                           s.LastActivity < DateTime.UtcNow.AddMinutes(-_settings.SessionTimeoutMinutes))
                .ToListAsync();

            foreach (var session in expiredSessions)
            {
                await EndSessionAsync(session.SessionId, "Session timeout");
            }

            if (expiredSessions.Any())
            {
                _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired sessions");
        }
    }
}