namespace RemoteDesktopApp.Services;

public interface IThemeService
{
    Task SetThemeAsync(string themeName);
    string GetCurrentTheme();
    IEnumerable<string> GetAvailableThemes();
    event EventHandler<string>? ThemeChanged;
}