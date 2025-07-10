using Microsoft.EntityFrameworkCore;
using RemoteDesktopServer.Models;

namespace RemoteDesktopServer.Data;

public class ServerDbContext : DbContext
{
    public ServerDbContext(DbContextOptions<ServerDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<ServerUser> Users { get; set; }
    public DbSet<ServerSession> Sessions { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<TwoFactorAuth> TwoFactorAuths { get; set; }
    
    // Application Management
    public DbSet<PublishedApplication> PublishedApplications { get; set; }
    public DbSet<ApplicationInstance> ApplicationInstances { get; set; }
    public DbSet<ApplicationLaunchLog> ApplicationLaunchLogs { get; set; }
    
    // Monitoring and Logging
    public DbSet<SessionActivity> SessionActivities { get; set; }
    public DbSet<PerformanceSnapshot> PerformanceSnapshots { get; set; }
    public DbSet<SecurityEvent> SecurityEvents { get; set; }
    
    // Configuration
    public DbSet<ServerConfiguration> ServerConfigurations { get; set; }
    public DbSet<ServerCertificate> ServerCertificates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ServerUser configuration
        modelBuilder.Entity<ServerUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Domain).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.HasIndex(e => new { e.Username, e.Domain }).IsUnique();
            
            entity.HasMany(e => e.Sessions)
                  .WithOne(s => s.User)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(e => e.SecurityEvents)
                  .WithOne(se => se.User)
                  .HasForeignKey(se => se.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.TwoFactorAuth)
                  .WithOne(tfa => tfa.ServerUser)
                  .HasForeignKey<TwoFactorAuth>(tfa => tfa.ServerUserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ServerSession configuration
        modelBuilder.Entity<ServerSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClientIP).IsRequired().HasMaxLength(45);
            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => e.State);
            
            entity.HasMany(e => e.Activities)
                  .WithOne(a => a.Session)
                  .HasForeignKey(a => a.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(e => e.PerformanceSnapshots)
                  .WithOne(ps => ps.Session)
                  .HasForeignKey(ps => ps.SessionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // PublishedApplication configuration
        modelBuilder.Entity<PublishedApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExecutablePath).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.Category);
            
            entity.HasMany(e => e.Instances)
                  .WithOne(i => i.Application)
                  .HasForeignKey(i => i.ApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(e => e.LaunchLogs)
                  .WithOne(ll => ll.Application)
                  .HasForeignKey(ll => ll.ApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserGroup configuration
        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            
            entity.HasMany(e => e.Users)
                  .WithMany(u => u.Groups)
                  .UsingEntity("UserGroupMembership");
                  
            entity.HasMany(e => e.Applications)
                  .WithMany(a => a.AuthorizedGroups)
                  .UsingEntity("GroupApplicationAccess");
        });

        // SessionActivity configuration
        modelBuilder.Entity<SessionActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.ActivityType);
        });

        // PerformanceSnapshot configuration
        modelBuilder.Entity<PerformanceSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
        });

        // SecurityEvent configuration
        modelBuilder.Entity<SecurityEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Severity);
        });

        // ApplicationInstance configuration
        modelBuilder.Entity<ApplicationInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InstanceId).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.InstanceId).IsUnique();
            entity.HasIndex(e => e.IsRunning);
        });

        // ApplicationLaunchLog configuration
        modelBuilder.Entity<ApplicationLaunchLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LaunchTime);
            entity.HasIndex(e => e.Success);
        });

        // ServerConfiguration configuration
        modelBuilder.Entity<ServerConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(2000);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.Category);
        });

        // ServerCertificate configuration
        modelBuilder.Entity<ServerCertificate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Thumbprint).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Thumbprint).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.ValidUntil);
        });

        // TwoFactorAuth configuration
        modelBuilder.Entity<TwoFactorAuth>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SecretKey).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BackupCodes).HasMaxLength(1000);
            entity.Property(e => e.QRCodePath).HasMaxLength(500);
            entity.Property(e => e.RecoveryEmail).HasMaxLength(255);
            entity.HasIndex(e => e.ServerUserId).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed default admin user
        modelBuilder.Entity<ServerUser>().HasData(new ServerUser
        {
            Id = 1,
            Username = "administrator",
            Domain = "LOCAL",
            FullName = "System Administrator",
            IsAdmin = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            MaxConcurrentSessions = 10,
            AllowRemoteApp = true,
            AllowDesktop = true
        });

        // Seed default admin group
        modelBuilder.Entity<UserGroup>().HasData(new UserGroup
        {
            Id = 1,
            Name = "Administrators",
            Description = "System Administrators with full access",
            IsActive = true,
            CanAccessDesktop = true,
            CanAccessRemoteApp = true,
            CanAccessAdmin = true,
            MaxConcurrentSessions = 10,
            SessionTimeoutMinutes = 480
        });

        // Seed default user group
        modelBuilder.Entity<UserGroup>().HasData(new UserGroup
        {
            Id = 2,
            Name = "Remote Desktop Users",
            Description = "Standard users with desktop access",
            IsActive = true,
            CanAccessDesktop = true,
            CanAccessRemoteApp = true,
            CanAccessAdmin = false,
            MaxConcurrentSessions = 2,
            SessionTimeoutMinutes = 240
        });

        // Seed basic server configuration
        modelBuilder.Entity<ServerConfiguration>().HasData(
            new ServerConfiguration
            {
                Id = 1,
                Key = "ServerName",
                Value = "RDP-SERVER-01",
                Category = "General",
                Description = "Server display name",
                UpdatedAt = DateTime.UtcNow
            },
            new ServerConfiguration
            {
                Id = 2,
                Key = "MaxConcurrentSessions",
                Value = "50",
                Category = "Performance",
                Description = "Maximum concurrent sessions allowed",
                UpdatedAt = DateTime.UtcNow
            },
            new ServerConfiguration
            {
                Id = 3,
                Key = "SessionTimeout",
                Value = "480",
                Category = "Security",
                Description = "Session timeout in minutes",
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}