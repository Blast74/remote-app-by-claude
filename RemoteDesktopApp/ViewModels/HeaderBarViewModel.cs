using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RemoteDesktopApp.ViewModels;

public class HeaderBarViewModel : INotifyPropertyChanged
{
    private string _userInitials = "AD";
    
    public string UserInitials
    {
        get => _userInitials;
        set => SetProperty(ref _userInitials, value);
    }
    
    // Commands
    public ICommand NewConnectionCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand ShowNotificationsCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand ToggleThemeCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand ShowSettingsCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand ShowUserProfileCommand => new RelayCommand(() => { /* TODO: Implement */ });
    
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