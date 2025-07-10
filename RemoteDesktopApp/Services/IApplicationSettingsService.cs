using RemoteDesktopApp.Models;

namespace RemoteDesktopApp.Services;

public interface IApplicationSettingsService
{
    Task InitializeAsync();
    Task<string?> GetSettingAsync(string key);
    Task<T?> GetSettingAsync<T>(string key);
    Task SetSettingAsync(string key, string value, string? category = null, bool isEncrypted = false);
    Task SetSettingAsync<T>(string key, T value, string? category = null, bool isEncrypted = false);
    Task<IEnumerable<ApplicationSetting>> GetSettingsByCategoryAsync(string category);
    Task<bool> DeleteSettingAsync(string key);
    Task ResetToDefaultsAsync();
}