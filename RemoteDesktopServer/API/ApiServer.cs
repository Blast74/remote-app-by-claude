using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RemoteDesktopServer.Configuration;
using RemoteDesktopServer.API.Controllers;
using RemoteDesktopServer.API.Hubs;

namespace RemoteDesktopServer.API;

public class ApiServer : BackgroundService
{
    private readonly ILogger<ApiServer> _logger;
    private readonly ApiSettings _apiSettings;
    private readonly IServiceProvider _serviceProvider;
    private IWebHost? _webHost;

    public ApiServer(
        ILogger<ApiServer> logger,
        IOptions<ApiSettings> apiSettings,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _apiSettings = apiSettings.Value;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_apiSettings.Enabled)
        {
            _logger.LogInformation("API Server is disabled");
            return;
        }

        try
        {
            var hostBuilder = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.ListenAnyIP(_apiSettings.Port, listenOptions =>
                    {
                        if (_apiSettings.UseHttps)
                        {
                            listenOptions.UseHttps();
                        }
                    });
                })
                .ConfigureServices(ConfigureApiServices)
                .Configure(ConfigureApiPipeline)
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                });

            _webHost = hostBuilder.Build();
            
            _logger.LogInformation("API Server starting on port {Port}", _apiSettings.Port);
            
            await _webHost.RunAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("API Server stopped");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in API Server");
            throw;
        }
    }

    private void ConfigureApiServices(IServiceCollection services)
    {
        // Add framework services
        services.AddControllers();
        services.AddSignalR();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Remote Desktop Server API",
                Version = "v1",
                Description = "API for managing Remote Desktop Server"
            });
        });

        // Add JWT authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSettings.JwtSecret))
                };
            });

        services.AddAuthorization();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        // Copy services from main container
        foreach (var service in _serviceProvider.GetServices<ServiceDescriptor>())
        {
            services.Add(service);
        }
    }

    private void ConfigureApiPipeline(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseCors();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Remote Desktop Server API V1");
            c.RoutePrefix = "";
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<ServerHub>("/serverHub");
        });
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping API Server...");
        
        if (_webHost != null)
        {
            await _webHost.StopAsync(cancellationToken);
            _webHost.Dispose();
        }
        
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("API Server stopped");
    }
}