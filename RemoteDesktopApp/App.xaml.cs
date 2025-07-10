using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RemoteDesktopApp.Data;
using RemoteDesktopApp.Services;
using RemoteDesktopApp.ViewModels;
using RemoteDesktopApp.Security;
using RemoteDesktopApp.Configuration;

namespace RemoteDesktopApp
{
    public partial class App : Application
    {
        private IHost? _host;
        private IServiceProvider? _serviceProvider;
        private ILogger<App>? _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Create host builder
                _host = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                              .AddEnvironmentVariables();
                    })
                    .ConfigureServices(ConfigureServices)
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.AddDebug();
                        logging.AddProvider(new FileLoggerProvider("logs"));
                    })
                    .Build();

                _serviceProvider = _host.Services;
                _logger = _serviceProvider.GetRequiredService<ILogger<App>>();

                // Initialize database
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated();

                // Initialize application settings
                var settingsService = _serviceProvider.GetRequiredService<IApplicationSettingsService>();
                settingsService.InitializeAsync().Wait();

                // Create and show main window
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();

                _logger.LogInformation("Application started successfully");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Fatal error during application startup: {ex.Message}";
                File.WriteAllText("startup_error.log", $"{DateTime.Now}: {errorMessage}\n{ex.StackTrace}");
                MessageBox.Show(errorMessage, "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }

            base.OnStartup(e);
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;

            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            // Configuration
            services.Configure<ApplicationConfig>(configuration.GetSection("ApplicationSettings"));
            services.Configure<SecurityConfig>(configuration.GetSection("Security"));
            services.Configure<RdpConfig>(configuration.GetSection("RDP"));
            services.Configure<UIConfig>(configuration.GetSection("UI"));

            // Core Services
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddScoped<IApplicationSettingsService, ApplicationSettingsService>();
            services.AddScoped<IConnectionProfileService, ConnectionProfileService>();
            services.AddScoped<ISessionManagerService, SessionManagerService>();
            services.AddScoped<ISecurityService, SecurityService>();
            services.AddScoped<IPerformanceMonitorService, PerformanceMonitorService>();
            services.AddScoped<IRemoteApplicationService, RemoteApplicationService>();
            services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
            services.AddScoped<IThemeService, ThemeService>();

            // RDP Services
            services.AddScoped<IRdpConnectionService, RdpConnectionService>();
            services.AddScoped<IRdpSessionService, RdpSessionService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<HeaderBarViewModel>();
            services.AddTransient<SidePanelViewModel>();
            services.AddTransient<TabContainerViewModel>();
            services.AddTransient<StatusBarViewModel>();
            services.AddTransient<SettingsPanelViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ConnectionDialogViewModel>();

            // Views
            services.AddTransient<MainWindow>();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                _logger?.LogCritical(e.Exception, "Unhandled exception occurred");
                
                var securityService = _serviceProvider?.GetService<ISecurityService>();
                securityService?.LogSecurityEventAsync("UNHANDLED_EXCEPTION", $"Exception: {e.Exception.Message}", Models.AuditSeverity.Critical);

                MessageBox.Show("An unexpected error occurred. Please restart the application.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            catch
            {
                // Fallback logging
                File.WriteAllText("critical_error.log", $"{DateTime.Now}: {e.Exception}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _logger?.LogInformation("Application shutting down");
                _host?.Dispose();
            }
            catch (Exception ex)
            {
                File.WriteAllText("shutdown_error.log", $"{DateTime.Now}: {ex}");
            }

            base.OnExit(e);
        }
    }
}
