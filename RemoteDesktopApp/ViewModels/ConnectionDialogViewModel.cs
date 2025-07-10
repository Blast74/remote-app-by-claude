using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RemoteDesktopApp.Models;

namespace RemoteDesktopApp.ViewModels;

public class ConnectionDialogViewModel : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _serverAddress = string.Empty;
    private int _port = 3389;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _domain = string.Empty;
    private bool _saveCredentials;
    private bool _connectToConsole;
    private bool _isFavorite;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    public string ServerAddress
    {
        get => _serverAddress;
        set => SetProperty(ref _serverAddress, value);
    }
    
    public int Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }
    
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }
    
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
    
    public string Domain
    {
        get => _domain;
        set => SetProperty(ref _domain, value);
    }
    
    public bool SaveCredentials
    {
        get => _saveCredentials;
        set => SetProperty(ref _saveCredentials, value);
    }
    
    public bool ConnectToConsole
    {
        get => _connectToConsole;
        set => SetProperty(ref _connectToConsole, value);
    }
    
    public bool IsFavorite
    {
        get => _isFavorite;
        set => SetProperty(ref _isFavorite, value);
    }
    
    // Commands
    public ICommand ConnectCommand => new RelayCommand(() => { /* TODO: Implement */ }, () => !string.IsNullOrEmpty(ServerAddress));
    public ICommand SaveCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand CancelCommand => new RelayCommand(() => { /* TODO: Implement */ });
    public ICommand TestConnectionCommand => new RelayCommand(async () => await TestConnectionAsync());
    
    private async Task TestConnectionAsync()
    {
        // TODO: Implement connection test
        await Task.Delay(1000);
    }
    
    public ConnectionProfile ToConnectionProfile()
    {
        return new ConnectionProfile
        {
            Name = Name,
            ServerAddress = ServerAddress,
            Port = Port,
            Username = Username,
            Domain = Domain,
            SaveCredentials = SaveCredentials,
            ConnectToConsole = ConnectToConsole,
            IsFavorite = IsFavorite
        };
    }
    
    public void LoadFromConnectionProfile(ConnectionProfile profile)
    {
        Name = profile.Name;
        ServerAddress = profile.ServerAddress;
        Port = profile.Port;
        Username = profile.Username ?? string.Empty;
        Domain = profile.Domain ?? string.Empty;
        SaveCredentials = profile.SaveCredentials;
        ConnectToConsole = profile.ConnectToConsole;
        IsFavorite = profile.IsFavorite;
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