using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RemoteDesktopApp.ViewModels;

public class SettingsPanelViewModel : INotifyPropertyChanged
{
    private string _selectedTheme = "Dark";
    private string _selectedLanguage = "en-US";
    private bool _autoSaveEnabled = true;
    private int _maxConcurrentSessions = 10;
    private bool _autoReconnectEnabled = true;
    private int _connectionTimeout = 30;
    private bool _twoFactorEnabled = false;
    private string _encryptionLevel = "High";
    private bool _performanceMonitoringEnabled = true;
    private string _loggingLevel = "Information";
    
    public string[] AvailableThemes { get; } = { "Dark", "Light" };
    public string[] AvailableLanguages { get; } = { "en-US", "fr-FR", "es-ES", "de-DE" };
    public string[] AvailableEncryptionLevels { get; } = { "Low", "Medium", "High" };
    public string[] AvailableLoggingLevels { get; } = { "Error", "Warning", "Information", "Debug" };
    
    public string SelectedTheme
    {
        get => _selectedTheme;
        set => SetProperty(ref _selectedTheme, value);
    }
    
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set => SetProperty(ref _selectedLanguage, value);
    }
    
    public bool AutoSaveEnabled
    {
        get => _autoSaveEnabled;
        set => SetProperty(ref _autoSaveEnabled, value);
    }
    
    public int MaxConcurrentSessions
    {
        get => _maxConcurrentSessions;
        set => SetProperty(ref _maxConcurrentSessions, value);
    }
    
    public bool AutoReconnectEnabled
    {
        get => _autoReconnectEnabled;
        set => SetProperty(ref _autoReconnectEnabled, value);
    }
    
    public int ConnectionTimeout
    {
        get => _connectionTimeout;
        set => SetProperty(ref _connectionTimeout, value);
    }
    
    public bool TwoFactorEnabled
    {
        get => _twoFactorEnabled;
        set => SetProperty(ref _twoFactorEnabled, value);
    }
    
    public string EncryptionLevel
    {
        get => _encryptionLevel;
        set => SetProperty(ref _encryptionLevel, value);
    }
    
    public bool PerformanceMonitoringEnabled
    {
        get => _performanceMonitoringEnabled;
        set => SetProperty(ref _performanceMonitoringEnabled, value);
    }
    
    public string LoggingLevel
    {
        get => _loggingLevel;
        set => SetProperty(ref _loggingLevel, value);
    }
    
    // Commands
    public ICommand CloseSettingsCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand ResetToDefaultsCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand SaveSettingsCommand => new RelayCommand(() => { /* TODO: Implement */ });
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}