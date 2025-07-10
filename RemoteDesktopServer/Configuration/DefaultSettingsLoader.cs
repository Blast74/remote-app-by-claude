using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RemoteDesktopServer.Configuration;

public class DefaultSettingsLoader
{
    private readonly IConfiguration _configuration;
    private readonly string _defaultSettingsPath;

    public DefaultSettingsLoader(IConfiguration configuration)
    {
        _configuration = configuration;
        _defaultSettingsPath = Path.Combine(AppContext.BaseDirectory, "Configuration", "default-settings.json");
    }

    public T LoadDefaultSettings<T>(string sectionName) where T : new()
    {
        try
        {
            if (!File.Exists(_defaultSettingsPath))
            {
                return new T();
            }

            var json = File.ReadAllText(_defaultSettingsPath);
            var defaultSettings = JsonSerializer.Deserialize<JsonDocument>(json);
            
            if (defaultSettings?.RootElement.TryGetProperty(sectionName, out var sectionElement) == true)
            {
                var sectionJson = sectionElement.GetRawText();
                var result = JsonSerializer.Deserialize<T>(sectionJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result ?? new T();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading default settings for {sectionName}: {ex.Message}");
        }

        return new T();
    }

    public void EnsureDefaultSettingsFile()
    {
        if (!File.Exists(_defaultSettingsPath))
        {
            var directory = Path.GetDirectoryName(_defaultSettingsPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            // Create a basic default settings file
            var defaultContent = "{}";
            File.WriteAllText(_defaultSettingsPath, defaultContent);
        }
    }
}