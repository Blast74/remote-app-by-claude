using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace RemoteDesktopApp.ViewModels;

public class StatusBarViewModel : INotifyPropertyChanged
{
    private readonly DispatcherTimer _timer;
    private string _statusMessage = "Ready";
    private int _activeSessionsCount;
    private float _cpuUsage;
    private float _memoryUsage;
    private double _networkLatency;
    private DateTime _currentTime = DateTime.Now;
    
    public StatusBarViewModel()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }
    
    private void Timer_Tick(object? sender, EventArgs e)
    {
        CurrentTime = DateTime.Now;
        // Here we would update performance metrics
    }
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
    
    public int ActiveSessionsCount
    {
        get => _activeSessionsCount;
        set => SetProperty(ref _activeSessionsCount, value);
    }
    
    public float CpuUsage
    {
        get => _cpuUsage;
        set => SetProperty(ref _cpuUsage, value);
    }
    
    public float MemoryUsage
    {
        get => _memoryUsage;
        set => SetProperty(ref _memoryUsage, value);
    }
    
    public double NetworkLatency
    {
        get => _networkLatency;
        set => SetProperty(ref _networkLatency, value);
    }
    
    public DateTime CurrentTime
    {
        get => _currentTime;
        set => SetProperty(ref _currentTime, value);
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