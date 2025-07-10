using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace RemoteDesktopServer.Configuration;

public interface IConfigurationService
{
    void ApplyDefaults();
    T GetSettings<T>(string sectionName) where T : class, new();
}

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly DefaultSettingsLoader _defaultSettingsLoader;
    private readonly IServiceProvider _serviceProvider;

    public ConfigurationService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _defaultSettingsLoader = new DefaultSettingsLoader(configuration);
    }

    public void ApplyDefaults()
    {
        _defaultSettingsLoader.EnsureDefaultSettingsFile();
        
        // Apply defaults for all configuration sections
        ApplyDefaultsToSection<ServerSettings>("ServerSettings");
        ApplyDefaultsToSection<SecuritySettings>("SecuritySettings");
        ApplyDefaultsToSection<ActiveDirectorySettings>("ActiveDirectorySettings");
        ApplyDefaultsToSection<PerformanceSettings>("PerformanceSettings");
        ApplyDefaultsToSection<RemoteAppSettings>("RemoteAppSettings");
        ApplyDefaultsToSection<LoadBalancingSettings>("LoadBalancingSettings");
        ApplyDefaultsToSection<ApiSettings>("ApiSettings");
        ApplyDefaultsToSection<ClusteringSettings>("ClusteringSettings");
        ApplyDefaultsToSection<BackupSettings>("BackupSettings");
    }

    public T GetSettings<T>(string sectionName) where T : class, new()
    {
        // Get settings from appsettings.json
        var settings = _configuration.GetSection(sectionName).Get<T>();
        
        if (settings == null)
        {
            // If not found in appsettings.json, load from defaults
            settings = _defaultSettingsLoader.LoadDefaultSettings<T>(sectionName);
        }
        else
        {
            // Merge with defaults for any missing properties
            var defaults = _defaultSettingsLoader.LoadDefaultSettings<T>(sectionName);
            settings = MergeSettings(settings, defaults);
        }

        return settings;
    }

    private void ApplyDefaultsToSection<T>(string sectionName) where T : class, new()
    {
        try
        {
            var optionsMonitor = _serviceProvider.GetService<IOptionsMonitor<T>>();
            if (optionsMonitor != null)
            {
                var currentSettings = optionsMonitor.CurrentValue;
                var defaultSettings = _defaultSettingsLoader.LoadDefaultSettings<T>(sectionName);
                
                // This would require reflection to actually merge the settings
                // For now, we ensure the defaults are loaded
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying defaults to {sectionName}: {ex.Message}");
        }
    }

    private T MergeSettings<T>(T current, T defaults) where T : class, new()
    {
        if (current == null) return defaults;
        if (defaults == null) return current;

        var type = typeof(T);
        foreach (var property in type.GetProperties())
        {
            if (property.CanRead && property.CanWrite)
            {
                var currentValue = property.GetValue(current);
                var defaultValue = property.GetValue(defaults);

                // For string properties, use default if current is null or empty
                if (property.PropertyType == typeof(string))
                {
                    if (string.IsNullOrEmpty(currentValue as string) && !string.IsNullOrEmpty(defaultValue as string))
                    {
                        property.SetValue(current, defaultValue);
                    }
                }
                // For numeric properties, use default if current is 0
                else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(double))
                {
                    if ((currentValue is int intVal && intVal == 0) || (currentValue is double doubleVal && doubleVal == 0.0))
                    {
                        property.SetValue(current, defaultValue);
                    }
                }
                // For arrays, use default if current is empty
                else if (property.PropertyType.IsArray)
                {
                    if (currentValue is Array array && array.Length == 0)
                    {
                        property.SetValue(current, defaultValue);
                    }
                }
            }
        }

        return current;
    }
}