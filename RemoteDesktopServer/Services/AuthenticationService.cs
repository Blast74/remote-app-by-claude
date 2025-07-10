using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RemoteDesktopServer.Configuration;
using RemoteDesktopServer.Data;
using RemoteDesktopServer.Models;
using RemoteDesktopServer.Security;

namespace RemoteDesktopServer.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ServerDbContext _context;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly SecuritySettings _securitySettings;
    private readonly ActiveDirectorySettings _adSettings;
    private readonly IEncryptionService _encryptionService;
    private readonly ISecurityService _securityService;

    public AuthenticationService(
        ServerDbContext context,
        ILogger<AuthenticationService> logger,
        IOptions<SecuritySettings> securitySettings,
        IOptions<ActiveDirectorySettings> adSettings,
        IEncryptionService encryptionService,
        ISecurityService securityService)
    {
        _context = context;
        _logger = logger;
        _securitySettings = securitySettings.Value;
        _adSettings = adSettings.Value;
        _encryptionService = encryptionService;
        _securityService = securityService;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password, string domain, string clientIP)
    {
        try
        {
            // Check if user is locked
            if (await IsUserLockedAsync(username, domain))
            {
                await LogFailedLoginAttemptAsync(username, domain, clientIP, "User account is locked");
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "User account is locked"
                };
            }

            // Get user from database
            var user = await GetUserAsync(username, domain);
            if (user == null)
            {
                await LogFailedLoginAttemptAsync(username, domain, clientIP, "User not found");
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password"
                };
            }

            // Check if account is expired
            if (user.IsAccountExpired)
            {
                await LogFailedLoginAttemptAsync(username, domain, clientIP, "Account expired");
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Account has expired"
                };
            }

            // Authenticate based on domain
            bool isAuthenticated = false;
            if (domain.ToUpper() == "LOCAL" || string.IsNullOrEmpty(_adSettings.Domain))
            {
                // Local authentication (for demo purposes)
                isAuthenticated = await AuthenticateLocalAsync(username, password);
            }
            else
            {
                // Active Directory authentication
                isAuthenticated = await AuthenticateActiveDirectoryAsync(username, password, domain);
            }

            if (!isAuthenticated)
            {
                await HandleFailedLoginAsync(user, clientIP, "Invalid password");
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password"
                };
            }

            // Reset failed login attempts on successful authentication
            user.FailedLoginAttempts = 0;
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Log successful authentication
            await _securityService.LogSecurityEventAsync(
                "AUTHENTICATION_SUCCESS",
                $"User {username} authenticated successfully from {clientIP}",
                SecurityEventSeverity.Info,
                username);

            var result = new AuthenticationResult
            {
                Success = true,
                User = user,
                RequiresTwoFactor = user.TwoFactorEnabled,
                TwoFactorMethod = user.TwoFactorEnabled ? "TOTP" : null
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user {Username}", username);
            
            await _securityService.LogSecurityEventAsync(
                "AUTHENTICATION_ERROR",
                $"Authentication error for user {username}: {ex.Message}",
                SecurityEventSeverity.Error,
                username);

            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "Authentication service error"
            };
        }
    }

    private async Task<bool> AuthenticateLocalAsync(string username, string password)
    {
        try
        {
            // For demonstration - in production, this would validate against local user database
            // with proper password hashing
            _logger.LogDebug("Performing local authentication for user {Username}", username);
            
            // Simulate authentication (always succeed for demo)
            await Task.Delay(100); // Simulate processing time
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in local authentication for user {Username}", username);
            return false;
        }
    }

    private async Task<bool> AuthenticateActiveDirectoryAsync(string username, string password, string domain)
    {
        try
        {
            _logger.LogDebug("Performing Active Directory authentication for user {Username} in domain {Domain}", username, domain);

            if (string.IsNullOrEmpty(_adSettings.Domain))
            {
                _logger.LogWarning("Active Directory domain not configured");
                return false;
            }

            using var context = new PrincipalContext(
                ContextType.Domain,
                _adSettings.Domain,
                _adSettings.LdapPath);

            var isValid = await Task.Run(() => context.ValidateCredentials(username, password));

            if (isValid)
            {
                // Get additional user information from AD
                using var userPrincipal = UserPrincipal.FindByIdentity(context, username);
                if (userPrincipal != null)
                {
                    // Update user information from AD
                    await UpdateUserFromActiveDirectoryAsync(username, domain, userPrincipal);
                }
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Active Directory authentication for user {Username}", username);
            return false;
        }
    }

    private async Task UpdateUserFromActiveDirectoryAsync(string username, string domain, UserPrincipal userPrincipal)
    {
        try
        {
            var user = await GetUserAsync(username, domain);
            if (user == null)
            {
                // Create new user from AD
                user = new ServerUser
                {
                    Username = username,
                    Domain = domain,
                    FullName = userPrincipal.DisplayName,
                    Email = userPrincipal.EmailAddress,
                    IsActive = userPrincipal.Enabled ?? true,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Users.Add(user);
            }
            else
            {
                // Update existing user
                user.FullName = userPrincipal.DisplayName;
                user.Email = userPrincipal.EmailAddress;
                user.IsActive = userPrincipal.Enabled ?? true;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user from Active Directory: {Username}", username);
        }
    }

    public async Task<bool> ValidateTwoFactorAsync(string username, string code)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.TwoFactorAuth)
                .FirstOrDefaultAsync(u => u.Username == username && u.TwoFactorEnabled);

            if (user == null || user.TwoFactorAuth == null)
            {
                return false;
            }

            var twoFactorAuth = user.TwoFactorAuth;
            if (twoFactorAuth == null || !twoFactorAuth.IsActive)
            {
                return false;
            }

            // Validate TOTP code (simplified - would use proper TOTP library)
            var isValid = ValidateTOTPCode(twoFactorAuth.SecretKey, code);

            if (isValid)
            {
                twoFactorAuth.LastUsedAt = DateTime.UtcNow;
                twoFactorAuth.UsageCount++;
                await _context.SaveChangesAsync();

                await _securityService.LogSecurityEventAsync(
                    "TWO_FACTOR_SUCCESS",
                    $"Two-factor authentication successful for user {username}",
                    SecurityEventSeverity.Info,
                    username);
            }
            else
            {
                await _securityService.LogSecurityEventAsync(
                    "TWO_FACTOR_FAILED",
                    $"Two-factor authentication failed for user {username}",
                    SecurityEventSeverity.Warning,
                    username);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating two-factor authentication for user {Username}", username);
            return false;
        }
    }

    private bool ValidateTOTPCode(string secretKey, string code)
    {
        // Simplified TOTP validation - in production would use proper TOTP library
        // For demo purposes, accept any 6-digit code
        return code.Length == 6 && code.All(char.IsDigit);
    }

    public async Task<ServerUser?> GetUserAsync(string username, string domain)
    {
        return await _context.Users
            .Include(u => u.Groups)
            .Include(u => u.TwoFactorAuth)
            .FirstOrDefaultAsync(u => u.Username == username && u.Domain == domain);
    }

    public async Task<bool> IsUserLockedAsync(string username, string domain)
    {
        var user = await GetUserAsync(username, domain);
        return user?.IsCurrentlyLocked ?? false;
    }

    public async Task LockUserAsync(string username, string domain, string reason)
    {
        try
        {
            var user = await GetUserAsync(username, domain);
            if (user == null) return;

            user.IsLocked = true;
            user.LockoutEndTime = DateTime.UtcNow.AddMinutes(_securitySettings.LockoutDurationMinutes);
            
            await _context.SaveChangesAsync();

            await _securityService.LogSecurityEventAsync(
                "USER_LOCKED",
                $"User {username} locked: {reason}",
                SecurityEventSeverity.Warning,
                username);

            _logger.LogWarning("User {Username} locked: {Reason}", username, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking user {Username}", username);
        }
    }

    public async Task UnlockUserAsync(string username, string domain)
    {
        try
        {
            var user = await GetUserAsync(username, domain);
            if (user == null) return;

            user.IsLocked = false;
            user.LockoutEndTime = null;
            user.FailedLoginAttempts = 0;
            
            await _context.SaveChangesAsync();

            await _securityService.LogSecurityEventAsync(
                "USER_UNLOCKED",
                $"User {username} unlocked",
                SecurityEventSeverity.Info,
                username);

            _logger.LogInformation("User {Username} unlocked", username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user {Username}", username);
        }
    }

    public async Task LogFailedLoginAttemptAsync(string username, string domain, string clientIP, string reason)
    {
        try
        {
            await _securityService.LogSecurityEventAsync(
                "AUTHENTICATION_FAILED",
                $"Failed login attempt for user {username} from {clientIP}: {reason}",
                SecurityEventSeverity.Warning,
                username);

            _logger.LogWarning(
                "Failed login attempt for user {Username} from {ClientIP}: {Reason}",
                username, clientIP, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging failed login attempt for user {Username}", username);
        }
    }

    private async Task HandleFailedLoginAsync(ServerUser user, string clientIP, string reason)
    {
        user.FailedLoginAttempts++;
        
        if (user.FailedLoginAttempts >= _securitySettings.FailedLoginAttempts)
        {
            await LockUserAsync(user.Username, user.Domain, "Too many failed login attempts");
        }
        else
        {
            await _context.SaveChangesAsync();
        }

        await LogFailedLoginAttemptAsync(user.Username, user.Domain, clientIP, reason);
    }
}