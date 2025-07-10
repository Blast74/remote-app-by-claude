using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RemoteDesktopApp.Data;
using RemoteDesktopApp.Models;
using RemoteDesktopApp.Security;

namespace RemoteDesktopApp.Services;

public class ApplicationSettingsService : IApplicationSettingsService
{
    private readonly ApplicationDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<ApplicationSettingsService> _logger;

    public ApplicationSettingsService(
        ApplicationDbContext context,
        IEncryptionService encryptionService,
        ILogger<ApplicationSettingsService> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Initialize default settings if they don't exist
            var defaultSettings = new Dictionary<string, (string value, string category, bool encrypted)>
            {
                { "Theme", ("Dark", "UI", false) },
                { "Language", ("en-US", "UI", false) },
                { "AutoSave", ("true", "General", false) },
                { "MaxSessions", ("10", "Performance", false) },
                { "LogLevel", ("Information", "Logging", false) },
                { "SecurityMode", ("High", "Security", false) },
                { "EncryptionEnabled", ("true", "Security", false) },
                { "BackupEnabled", ("true", "Backup", false) },
                { "BackupInterval", ("24", "Backup", false) }
            };

            foreach (var setting in defaultSettings)
            {
                var existing = await _context.ApplicationSettings
                    .FirstOrDefaultAsync(s => s.Key == setting.Key);
                
                if (existing == null)
                {
                    var newSetting = new ApplicationSetting
                    {
                        Key = setting.Key,
                        Value = setting.Value.value,
                        Category = setting.Value.category,
                        IsEncrypted = setting.Value.encrypted,
                        IsSystemSetting = true
                    };
                    
                    _context.ApplicationSettings.Add(newSetting);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Application settings initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize application settings");
            throw;
        }
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        try
        {
            var setting = await _context.ApplicationSettings
                .FirstOrDefaultAsync(s => s.Key == key);
            
            if (setting == null)
                return null;

            return setting.IsEncrypted ? _encryptionService.Decrypt(setting.Value) : setting.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get setting: {Key}", key);
            return null;
        }
    }

    public async Task<T?> GetSettingAsync<T>(string key)
    {
        var value = await GetSettingAsync(key);
        if (value == null)
            return default(T);

        try
        {
            if (typeof(T) == typeof(string))
                return (T)(object)value;
            
            return JsonConvert.DeserializeObject<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize setting: {Key}", key);
            return default(T);
        }
    }

    public async Task SetSettingAsync(string key, string value, string? category = null, bool isEncrypted = false)
    {
        try
        {
            var setting = await _context.ApplicationSettings
                .FirstOrDefaultAsync(s => s.Key == key);
            
            var finalValue = isEncrypted ? _encryptionService.Encrypt(value) : value;
            
            if (setting == null)
            {
                setting = new ApplicationSetting
                {
                    Key = key,
                    Value = finalValue,
                    Category = category,
                    IsEncrypted = isEncrypted,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ApplicationSettings.Add(setting);
            }
            else
            {
                setting.Value = finalValue;
                setting.Category = category ?? setting.Category;
                setting.IsEncrypted = isEncrypted;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Setting updated: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set setting: {Key}", key);
            throw;
        }
    }

    public async Task SetSettingAsync<T>(string key, T value, string? category = null, bool isEncrypted = false)
    {
        var serializedValue = typeof(T) == typeof(string) ? 
            value?.ToString() ?? "" : 
            JsonConvert.SerializeObject(value);
        
        await SetSettingAsync(key, serializedValue, category, isEncrypted);
    }

    public async Task<IEnumerable<ApplicationSetting>> GetSettingsByCategoryAsync(string category)
    {
        return await _context.ApplicationSettings
            .Where(s => s.Category == category)
            .ToListAsync();
    }

    public async Task<bool> DeleteSettingAsync(string key)
    {
        try
        {
            var setting = await _context.ApplicationSettings
                .FirstOrDefaultAsync(s => s.Key == key);
            
            if (setting == null)
                return false;

            _context.ApplicationSettings.Remove(setting);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Setting deleted: {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete setting: {Key}", key);
            return false;
        }
    }

    public async Task ResetToDefaultsAsync()
    {
        try
        {
            var userSettings = await _context.ApplicationSettings
                .Where(s => !s.IsSystemSetting)
                .ToListAsync();
            
            _context.ApplicationSettings.RemoveRange(userSettings);
            await _context.SaveChangesAsync();
            
            await InitializeAsync();
            _logger.LogInformation("Settings reset to defaults");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset settings to defaults");
            throw;
        }
    }
}