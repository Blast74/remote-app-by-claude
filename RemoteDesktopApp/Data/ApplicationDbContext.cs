using Microsoft.EntityFrameworkCore;
using RemoteDesktopApp.Models;

namespace RemoteDesktopApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ConnectionProfile> ConnectionProfiles { get; set; }
    public DbSet<ConnectionSession> ConnectionSessions { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<SecurityAuditLog> SecurityAuditLogs { get; set; }
    public DbSet<PerformanceMetric> PerformanceMetrics { get; set; }
    public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
    public DbSet<RemoteApplication> RemoteApplications { get; set; }
    public DbSet<TwoFactorAuth> TwoFactorAuths { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ConnectionProfile configuration
        modelBuilder.Entity<ConnectionProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ServerAddress).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.EncryptedPassword).HasMaxLength(500);
            entity.Property(e => e.Domain).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasMany(e => e.Sessions).WithOne(s => s.ConnectionProfile).HasForeignKey(s => s.ConnectionProfileId);
        });

        // ConnectionSession configuration
        modelBuilder.Entity<ConnectionSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.ServerAddress).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.ClientInfo).HasMaxLength(500);
            entity.Property(e => e.DisconnectReason).HasMaxLength(500);
            entity.HasOne(e => e.ConnectionProfile).WithMany(p => p.Sessions).HasForeignKey(e => e.ConnectionProfileId);
        });

        // UserProfile configuration
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.EncryptedPassword).HasMaxLength(500);
            entity.Property(e => e.TwoFactorEnabled).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastLoginAt);
            entity.Property(e => e.FailedLoginAttempts).IsRequired();
            entity.Property(e => e.IsLockedOut).IsRequired();
            entity.HasMany(e => e.TwoFactorAuths).WithOne(t => t.UserProfile).HasForeignKey(t => t.UserProfileId);
        });

        // SecurityAuditLog configuration
        modelBuilder.Entity<SecurityAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IPAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Severity).IsRequired();
        });

        // PerformanceMetric configuration
        modelBuilder.Entity<PerformanceMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.MetricType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.ServerAddress).HasMaxLength(255);
        });

        // ApplicationSetting configuration
        modelBuilder.Entity<ApplicationSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsEncrypted).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasIndex(e => e.Key).IsUnique();
        });

        // RemoteApplication configuration
        modelBuilder.Entity<RemoteApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Arguments).HasMaxLength(1000);
            entity.Property(e => e.IconPath).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ServerAddress).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasOne(e => e.ConnectionProfile).WithMany(p => p.RemoteApplications).HasForeignKey(e => e.ConnectionProfileId);
        });

        // TwoFactorAuth configuration
        modelBuilder.Entity<TwoFactorAuth>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SecretKey).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BackupCodes).HasMaxLength(1000);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastUsedAt);
            entity.HasOne(e => e.UserProfile).WithMany(u => u.TwoFactorAuths).HasForeignKey(e => e.UserProfileId);
        });
    }
}