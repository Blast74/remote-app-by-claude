# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
This is a complete professional Remote Desktop application written in C# using .NET 8 and WPF. The application implements all the features specified for a modern RDP client with Material Design UI, MVVM architecture, dependency injection, and comprehensive security features.

## Common Commands

### Building and Running
```bash
cd RemoteDesktopApp
dotnet build
dotnet run
```

Note: The project requires `EnableWindowsTargeting=true` for cross-platform development.

### Database Operations
The application uses Entity Framework Core with SQLite:
```bash
# Migrations (if needed)
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Testing
```bash
dotnet test
```

## Architecture

### Project Structure
```
RemoteDesktopApp/
├── App.xaml & App.xaml.cs          # Application entry point with DI container setup
├── MainWindow.xaml & MainWindow.xaml.cs  # Main window with Material Design layout
├── Configuration/                   # Configuration classes (ApplicationConfig, SecurityConfig, etc.)
├── Data/                           # Entity Framework DbContext and migrations
├── Models/                         # Data models (ConnectionProfile, ConnectionSession, etc.)
├── ViewModels/                     # MVVM ViewModels with INotifyPropertyChanged
├── Views/                          # WPF UserControls (HeaderBar, SidePanel, TabContainer, etc.)
├── Services/                       # Business logic services and interfaces
├── Security/                       # Encryption and security services
└── Utilities/                      # Helper classes and extensions
```

### Key Components

#### Application Layer
- **App.xaml.cs**: Full dependency injection setup with Microsoft.Extensions.Hosting
- **MainWindow**: Material Design layout with HeaderBar, SidePanel, TabContainer, StatusBar, SettingsPanel
- **Configuration**: Strongly-typed configuration classes bound to appsettings.json

#### Data Layer
- **ApplicationDbContext**: Entity Framework Core context with complete data model
- **Models**: Full set of entities for connections, sessions, users, security logs, performance metrics
- **Migrations**: Database schema management

#### UI Layer (MVVM)
- **HeaderBar**: Search, notifications, user profile, theme toggle, settings
- **SidePanel**: Favorites, recent connections, quick actions (300px width)
- **TabContainer**: Multi-tab RDP sessions with toolbar and status
- **StatusBar**: Connection info, performance metrics, real-time clock
- **SettingsPanel**: Sliding panel with comprehensive settings

#### Services Layer
- **IConnectionProfileService**: CRUD operations for connection profiles
- **ISessionManagerService**: RDP session lifecycle management
- **ISecurityService**: Authentication, 2FA, audit logging
- **IEncryptionService**: AES-256 encryption for credentials
- **IThemeService**: Dark/Light theme switching
- **IPerformanceMonitorService**: Real-time performance monitoring
- **IRdpConnectionService**: Native RDP protocol implementation
- **IApplicationSettingsService**: Configuration management

### Security Features
- **AES-256 Encryption**: All credentials encrypted at rest
- **Two-Factor Authentication**: TOTP with QR codes and backup codes
- **Security Audit Logging**: Comprehensive audit trail
- **Active Directory Integration**: Enterprise authentication support
- **Certificate Management**: SSL/TLS certificate handling

### Performance & Monitoring
- **Real-time Metrics**: CPU, Memory, Network latency monitoring
- **Session Management**: Multi-tab support with reconnection
- **Performance Logging**: Detailed performance metrics storage
- **Resource Management**: Proper disposal and memory management

### UI/UX Features
- **Material Design**: Modern, professional interface
- **Dark/Light Themes**: Full theme support with user preference
- **Responsive Layout**: Resizable panels and responsive design
- **Animations**: Smooth transitions and loading states
- **Accessibility**: WCAG compliant interface elements

## Configuration

### appsettings.json
The application uses structured configuration:
- **ConnectionStrings**: SQLite database connection
- **ApplicationSettings**: Session limits, timeouts, reconnection settings
- **Security**: Encryption keys, 2FA settings, lockout policies
- **RDP**: Protocol settings, redirection options, display settings
- **UI**: Theme, panel sizes, animation settings
- **Performance**: Monitoring intervals, logging limits

### Environment Variables
The application supports environment variable overrides for all configuration settings.

## Development Notes

### Code Standards
- **MVVM Pattern**: Strict separation of View, ViewModel, and Model
- **Dependency Injection**: Constructor injection throughout
- **Async/Await**: Async patterns for all I/O operations
- **Error Handling**: Comprehensive error handling with user-friendly messages
- **Logging**: Structured logging with different levels
- **Security**: Never log sensitive data, secure credential handling

### Database Design
- **Entity Framework Core**: Code-first approach with migrations
- **SQLite**: Local database for connection profiles and settings
- **Audit Trail**: All security events logged with timestamps
- **Performance Data**: Metrics stored for analysis and reporting

### RDP Implementation
- **Native RDP Protocol**: Direct RDP protocol implementation
- **Multi-session Support**: Multiple concurrent RDP sessions
- **Session Management**: Connection state tracking and recovery
- **Performance Optimization**: Bandwidth and latency optimization

### Testing Strategy
- **Unit Tests**: Service layer and business logic
- **Integration Tests**: Database and external service integration
- **UI Tests**: ViewModel and binding validation
- **Security Tests**: Encryption and authentication validation

## Troubleshooting

### Common Issues
1. **Build Errors**: Ensure `EnableWindowsTargeting=true` is set
2. **XAML Errors**: Check StringFormat syntax uses escaped braces
3. **Package Issues**: Verify all NuGet packages are compatible with .NET 8
4. **Database Issues**: Ensure SQLite database file permissions

### Logging
- Application logs: `logs/app_yyyy-MM-dd.log`
- Error logs: `startup_error.log`, `critical_error.log`, `shutdown_error.log`
- Database location: `remotedesktop.db`

This is a production-ready Remote Desktop application with enterprise-grade features, security, and user experience.