namespace RemoteDesktopApp.Configuration;

public class ApplicationConfig
{
    public string DatabasePath { get; set; } = "remotedesktop.db";
    public string LogsPath { get; set; } = "logs";
    public string BackupPath { get; set; } = "backups";
    public int MaxConcurrentSessions { get; set; } = 10;
    public int SessionTimeout { get; set; } = 30;
    public int AutoReconnectAttempts { get; set; } = 5;
    public int AutoReconnectDelay { get; set; } = 5000;
}

public class SecurityConfig
{
    public string EncryptionKey { get; set; } = "RemoteDesktopApp2024SecureKey123!";
    public int TokenExpiration { get; set; } = 3600;
    public bool RequireTwoFactor { get; set; } = true;
    public int MaxFailedAttempts { get; set; } = 3;
    public int LockoutDuration { get; set; } = 900;
}

public class RdpConfig
{
    public int DefaultPort { get; set; } = 3389;
    public bool CompressionEnabled { get; set; } = true;
    public bool AudioRedirection { get; set; } = true;
    public bool PrinterRedirection { get; set; } = true;
    public bool DriveRedirection { get; set; } = false;
    public bool ClipboardRedirection { get; set; } = true;
    public int ColorDepth { get; set; } = 32;
    public int DesktopWidth { get; set; } = 1920;
    public int DesktopHeight { get; set; } = 1080;
    public bool ConnectToConsole { get; set; } = false;
    public bool EnableCredSSP { get; set; } = true;
}

public class UIConfig
{
    public string DefaultTheme { get; set; } = "Dark";
    public int SidePanelWidth { get; set; } = 300;
    public int HeaderBarHeight { get; set; } = 60;
    public int StatusBarHeight { get; set; } = 30;
    public int TabHeight { get; set; } = 35;
    public int AnimationDuration { get; set; } = 300;
    public int AutoSaveInterval { get; set; } = 30000;
}