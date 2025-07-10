using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace RemoteDesktopApp.Services;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logPath;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    public FileLoggerProvider(string logPath)
    {
        _logPath = logPath;
        Directory.CreateDirectory(_logPath);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _logPath));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public class FileLogger : ILogger
{
    private readonly string _name;
    private readonly string _logPath;
    private readonly object _lock = new object();

    public FileLogger(string name, string logPath)
    {
        _name = name;
        _logPath = logPath;
    }

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] [{_name}] {message}";
        
        if (exception != null)
        {
            logEntry += $"\n{exception}";
        }

        lock (_lock)
        {
            try
            {
                var logFile = Path.Combine(_logPath, $"app_{DateTime.Now:yyyy-MM-dd}.log");
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // Fail silently to avoid recursive logging errors
            }
        }
    }
}