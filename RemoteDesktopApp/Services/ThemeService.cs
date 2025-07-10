using System.Windows;
using Microsoft.Extensions.Logging;

namespace RemoteDesktopApp.Services;

public class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService> _logger;
    private string _currentTheme = "Dark";
    
    public event EventHandler<string>? ThemeChanged;

    public ThemeService(ILogger<ThemeService> logger)
    {
        _logger = logger;
    }

    public async Task SetThemeAsync(string themeName)
    {
        try
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var theme = themeName.ToLower() == "dark" ? "Dark" : "Light";
                    
                    // This would typically update Material Design theme resources
                    _currentTheme = theme;
                    ThemeChanged?.Invoke(this, theme);
                    
                    _logger.LogInformation("Theme changed to: {Theme}", theme);
                });
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set theme: {Theme}", themeName);
            throw;
        }
    }

    public string GetCurrentTheme() => _currentTheme;

    public IEnumerable<string> GetAvailableThemes() => new[] { "Dark", "Light" };
}