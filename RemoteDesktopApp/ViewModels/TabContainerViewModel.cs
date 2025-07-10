using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RemoteDesktopApp.Models;

namespace RemoteDesktopApp.ViewModels;

public class TabContainerViewModel : INotifyPropertyChanged
{
    private ConnectionSession? _selectedSession;
    
    public ObservableCollection<ConnectionSession> ActiveSessions { get; } = new();
    
    public ConnectionSession? SelectedSession
    {
        get => _selectedSession;
        set => SetProperty(ref _selectedSession, value);
    }
    
    public bool HasNoSessions => ActiveSessions.Count == 0;
    
    // Commands
    public ICommand NewConnectionCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand CloseSessionCommand => new RelayCommand<ConnectionSession>(session => { /* TODO: Implement */ });
    
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