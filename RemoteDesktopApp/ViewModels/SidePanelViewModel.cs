using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RemoteDesktopApp.Models;

namespace RemoteDesktopApp.ViewModels;

public class SidePanelViewModel : INotifyPropertyChanged
{
    private int _activeSessionsCount;
    
    public ObservableCollection<ConnectionProfile> FavoriteConnections { get; } = new();
    public ObservableCollection<ConnectionProfile> RecentConnections { get; } = new();
    
    public int ActiveSessionsCount
    {
        get => _activeSessionsCount;
        set => SetProperty(ref _activeSessionsCount, value);
    }
    
    // Commands
    public ICommand NewConnectionCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand ImportConnectionsCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand RemoteAppsCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand ConnectCommand => new RelayCommand<ConnectionProfile>(profile => { /* TODO: Implement */ });
    public ICommand EditConnectionCommand => new RelayCommand<ConnectionProfile>(profile => { /* TODO: Implement */ });
    
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

public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;
    
    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
    
    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
    
    public void Execute(object? parameter) => _execute((T?)parameter);
}