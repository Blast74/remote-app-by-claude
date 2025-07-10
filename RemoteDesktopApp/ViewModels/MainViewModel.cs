using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using RemoteDesktopApp.Models;
using RemoteDesktopApp.Services;

namespace RemoteDesktopApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly IApplicationSettingsService _settingsService;
    private readonly ISessionManagerService _sessionManager;
    private readonly IThemeService _themeService;
    
    private bool _isLoading;
    private bool _isSettingsPanelVisible;
    private string _statusMessage = "Ready";
    private string _currentTheme = "Dark";

    public MainViewModel(
        ILogger<MainViewModel> logger,
        IApplicationSettingsService settingsService,
        ISessionManagerService sessionManager,
        IThemeService themeService)
    {
        _logger = logger;
        _settingsService = settingsService;
        _sessionManager = sessionManager;
        _themeService = themeService;
        
        InitializeAsync();
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsSettingsPanelVisible
    {
        get => _isSettingsPanelVisible;
        set => SetProperty(ref _isSettingsPanelVisible, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string CurrentTheme
    {
        get => _currentTheme;
        set => SetProperty(ref _currentTheme, value);
    }

    public ObservableCollection<ConnectionSession> ActiveSessions { get; } = new();
    public ObservableCollection<ConnectionProfile> RecentConnections { get; } = new();
    public ObservableCollection<ConnectionProfile> FavoriteConnections { get; } = new();

    // Commands
    public ICommand ShowSettingsCommand => new RelayCommand(() => IsSettingsPanelVisible = true);
    public ICommand HideSettingsCommand => new RelayCommand(() => IsSettingsPanelVisible = false);
    public ICommand ToggleThemeCommand => new RelayCommand(async () => await ToggleThemeAsync());
    public ICommand ExitCommand => new RelayCommand(() => App.Current.Shutdown());

    private async void InitializeAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Initializing application...";

            // Load theme
            var theme = await _settingsService.GetSettingAsync("Theme") ?? "Dark";
            CurrentTheme = theme;
            await _themeService.SetThemeAsync(theme);

            // Load recent connections
            await LoadRecentConnectionsAsync();
            
            // Load favorite connections
            await LoadFavoriteConnectionsAsync();
            
            StatusMessage = "Application ready";
            _logger.LogInformation("MainViewModel initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MainViewModel");
            StatusMessage = "Initialization failed";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadRecentConnectionsAsync()
    {
        try
        {
            // This would be implemented by ConnectionProfileService
            // For now, we'll add placeholder data
            RecentConnections.Clear();
            
            // Load from database would go here
            _logger.LogInformation("Recent connections loaded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load recent connections");
        }
    }

    private async Task LoadFavoriteConnectionsAsync()
    {
        try
        {
            // This would be implemented by ConnectionProfileService
            // For now, we'll add placeholder data
            FavoriteConnections.Clear();
            
            // Load from database would go here
            _logger.LogInformation("Favorite connections loaded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load favorite connections");
        }
    }

    private async Task ToggleThemeAsync()
    {
        try
        {
            var newTheme = CurrentTheme == "Dark" ? "Light" : "Dark";
            await _themeService.SetThemeAsync(newTheme);
            await _settingsService.SetSettingAsync("Theme", newTheme);
            CurrentTheme = newTheme;
            
            _logger.LogInformation("Theme changed to: {Theme}", newTheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle theme");
        }
    }

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

// Helper command class
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();
}