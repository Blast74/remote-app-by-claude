using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RemoteDesktopServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublishedApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ExecutablePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CommandLineArguments = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    WorkingDirectory = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IconPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Version = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Publisher = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLaunchedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LaunchCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxConcurrentInstances = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeoutMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    RequireAdminRights = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowFileAccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowPrinterAccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowClipboardAccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnvironmentVariables = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    FileAssociations = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    AutoStart = table.Column<bool>(type: "INTEGER", nullable: false),
                    MinimizeOnStart = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowedUsers = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    AllowedGroups = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    AllowedIPs = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AvailableFrom = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AvailableUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AvailableHours = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    MaxLicenses = table.Column<int>(type: "INTEGER", nullable: false),
                    UsedLicenses = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxMemoryMB = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxCpuPercent = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishedApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Thumbprint = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Issuer = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    Usage = table.Column<int>(type: "INTEGER", nullable: false),
                    CertificatePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerCertificates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsEncrypted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresRestart = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CanAccessDesktop = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanAccessRemoteApp = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanAccessAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxConcurrentSessions = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionTimeoutMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowedHours = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    AllowedIPs = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Domain = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    RequirePasswordChange = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordLastChanged = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorSecret = table.Column<string>(type: "TEXT", nullable: true),
                    MaxConcurrentSessions = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowRemoteApp = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowDesktop = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProfilePath = table.Column<string>(type: "TEXT", nullable: true),
                    HomeDirectory = table.Column<string>(type: "TEXT", nullable: true),
                    AllowedHours = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    AllowedIPs = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AccountExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupApplicationAccess",
                columns: table => new
                {
                    ApplicationsId = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthorizedGroupsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupApplicationAccess", x => new { x.ApplicationsId, x.AuthorizedGroupsId });
                    table.ForeignKey(
                        name: "FK_GroupApplicationAccess_PublishedApplications_ApplicationsId",
                        column: x => x.ApplicationsId,
                        principalTable: "PublishedApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupApplicationAccess_UserGroups_AuthorizedGroupsId",
                        column: x => x.AuthorizedGroupsId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublishedApplicationServerUser",
                columns: table => new
                {
                    AuthorizedUsersId = table.Column<int>(type: "INTEGER", nullable: false),
                    PublishedApplicationsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishedApplicationServerUser", x => new { x.AuthorizedUsersId, x.PublishedApplicationsId });
                    table.ForeignKey(
                        name: "FK_PublishedApplicationServerUser_PublishedApplications_PublishedApplicationsId",
                        column: x => x.PublishedApplicationsId,
                        principalTable: "PublishedApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PublishedApplicationServerUser_Users_AuthorizedUsersId",
                        column: x => x.AuthorizedUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SourceIP = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Details = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Domain = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ClientIP = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    ClientInfo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    DesktopWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    DesktopHeight = table.Column<int>(type: "INTEGER", nullable: false),
                    ColorDepth = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    PrinterRedirectionEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    DriveRedirectionEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClipboardRedirectionEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    BytesTransferred = table.Column<long>(type: "INTEGER", nullable: false),
                    BytesReceived = table.Column<long>(type: "INTEGER", nullable: false),
                    AverageLatency = table.Column<double>(type: "REAL", nullable: false),
                    CpuUsage = table.Column<float>(type: "REAL", nullable: false),
                    MemoryUsageMB = table.Column<float>(type: "REAL", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DisconnectReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsRecorded = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecordingPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TwoFactorAuths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SecretKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BackupCodes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    QRCodePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RecoveryEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ServerUserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwoFactorAuths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwoFactorAuths_Users_ServerUserId",
                        column: x => x.ServerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupMembership",
                columns: table => new
                {
                    GroupsId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupMembership", x => new { x.GroupsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserGroupMembership_UserGroups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroupMembership_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    InstanceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProcessId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsRunning = table.Column<bool>(type: "INTEGER", nullable: false),
                    CpuUsage = table.Column<double>(type: "REAL", nullable: false),
                    MemoryUsageMB = table.Column<long>(type: "INTEGER", nullable: false),
                    ExitReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ExitCode = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationInstances_PublishedApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "PublishedApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationInstances_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLaunchLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    LaunchTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExitTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ClientIP = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLaunchLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationLaunchLogs_PublishedApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "PublishedApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationLaunchLogs_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationLaunchLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CpuUsagePercent = table.Column<double>(type: "REAL", nullable: false),
                    MemoryUsagePercent = table.Column<double>(type: "REAL", nullable: false),
                    MemoryUsageMB = table.Column<long>(type: "INTEGER", nullable: false),
                    DiskUsagePercent = table.Column<double>(type: "REAL", nullable: false),
                    NetworkInKbps = table.Column<double>(type: "REAL", nullable: false),
                    NetworkOutKbps = table.Column<double>(type: "REAL", nullable: false),
                    ActiveSessions = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnectedUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageResponseTime = table.Column<double>(type: "REAL", nullable: false),
                    SystemLoad = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceSnapshots_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SessionActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActivityType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ResourceAccessed = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionActivities_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ServerConfigurations",
                columns: new[] { "Id", "Category", "Description", "IsEncrypted", "Key", "RequiresRestart", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 1, "General", "Server display name", false, "ServerName", false, new DateTime(2025, 7, 10, 13, 0, 57, 632, DateTimeKind.Utc).AddTicks(2546), null, "RDP-SERVER-01" },
                    { 2, "Performance", "Maximum concurrent sessions allowed", false, "MaxConcurrentSessions", false, new DateTime(2025, 7, 10, 13, 0, 57, 632, DateTimeKind.Utc).AddTicks(2547), null, "50" },
                    { 3, "Security", "Session timeout in minutes", false, "SessionTimeout", false, new DateTime(2025, 7, 10, 13, 0, 57, 632, DateTimeKind.Utc).AddTicks(2549), null, "480" }
                });

            migrationBuilder.InsertData(
                table: "UserGroups",
                columns: new[] { "Id", "AllowedHours", "AllowedIPs", "CanAccessAdmin", "CanAccessDesktop", "CanAccessRemoteApp", "CreatedAt", "Description", "IsActive", "MaxConcurrentSessions", "Name", "SessionTimeoutMinutes" },
                values: new object[,]
                {
                    { 1, null, null, true, true, true, new DateTime(2025, 7, 10, 13, 0, 57, 632, DateTimeKind.Utc).AddTicks(2499), "System Administrators with full access", true, 10, "Administrators", 480 },
                    { 2, null, null, false, true, true, new DateTime(2025, 7, 10, 13, 0, 57, 632, DateTimeKind.Utc).AddTicks(2521), "Standard users with desktop access", true, 2, "Remote Desktop Users", 240 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountExpirationDate", "AllowDesktop", "AllowRemoteApp", "AllowedHours", "AllowedIPs", "CreatedAt", "Domain", "Email", "FailedLoginAttempts", "FullName", "HomeDirectory", "IsActive", "IsAdmin", "IsLocked", "LastLoginAt", "LockoutEndTime", "MaxConcurrentSessions", "PasswordLastChanged", "ProfilePath", "RequirePasswordChange", "TwoFactorEnabled", "TwoFactorSecret", "Username" },
                values: new object[] { 1, null, true, true, null, null, new DateTime(2025, 7, 10, 13, 0, 57, 632, DateTimeKind.Utc).AddTicks(2354), "LOCAL", null, 0, "System Administrator", null, true, true, false, null, null, 10, null, null, false, false, null, "administrator" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationInstances_ApplicationId",
                table: "ApplicationInstances",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationInstances_InstanceId",
                table: "ApplicationInstances",
                column: "InstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationInstances_IsRunning",
                table: "ApplicationInstances",
                column: "IsRunning");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationInstances_SessionId",
                table: "ApplicationInstances",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLaunchLogs_ApplicationId",
                table: "ApplicationLaunchLogs",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLaunchLogs_LaunchTime",
                table: "ApplicationLaunchLogs",
                column: "LaunchTime");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLaunchLogs_SessionId",
                table: "ApplicationLaunchLogs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLaunchLogs_Success",
                table: "ApplicationLaunchLogs",
                column: "Success");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLaunchLogs_UserId",
                table: "ApplicationLaunchLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupApplicationAccess_AuthorizedGroupsId",
                table: "GroupApplicationAccess",
                column: "AuthorizedGroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceSnapshots_SessionId",
                table: "PerformanceSnapshots",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceSnapshots_Timestamp",
                table: "PerformanceSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_PublishedApplications_Category",
                table: "PublishedApplications",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_PublishedApplications_IsEnabled",
                table: "PublishedApplications",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_PublishedApplications_Name",
                table: "PublishedApplications",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublishedApplicationServerUser_PublishedApplicationsId",
                table: "PublishedApplicationServerUser",
                column: "PublishedApplicationsId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_EventType",
                table: "SecurityEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_Severity",
                table: "SecurityEvents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_Timestamp",
                table: "SecurityEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_UserId",
                table: "SecurityEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerCertificates_IsActive",
                table: "ServerCertificates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ServerCertificates_Thumbprint",
                table: "ServerCertificates",
                column: "Thumbprint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServerCertificates_ValidUntil",
                table: "ServerCertificates",
                column: "ValidUntil");

            migrationBuilder.CreateIndex(
                name: "IX_ServerConfigurations_Category",
                table: "ServerConfigurations",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ServerConfigurations_Key",
                table: "ServerConfigurations",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionActivities_ActivityType",
                table: "SessionActivities",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_SessionActivities_SessionId",
                table: "SessionActivities",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionActivities_Timestamp",
                table: "SessionActivities",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionId",
                table: "Sessions",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_StartTime",
                table: "Sessions",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_State",
                table: "Sessions",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorAuths_IsActive",
                table: "TwoFactorAuths",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorAuths_ServerUserId",
                table: "TwoFactorAuths",
                column: "ServerUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMembership_UsersId",
                table: "UserGroupMembership",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_Name",
                table: "UserGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username_Domain",
                table: "Users",
                columns: new[] { "Username", "Domain" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationInstances");

            migrationBuilder.DropTable(
                name: "ApplicationLaunchLogs");

            migrationBuilder.DropTable(
                name: "GroupApplicationAccess");

            migrationBuilder.DropTable(
                name: "PerformanceSnapshots");

            migrationBuilder.DropTable(
                name: "PublishedApplicationServerUser");

            migrationBuilder.DropTable(
                name: "SecurityEvents");

            migrationBuilder.DropTable(
                name: "ServerCertificates");

            migrationBuilder.DropTable(
                name: "ServerConfigurations");

            migrationBuilder.DropTable(
                name: "SessionActivities");

            migrationBuilder.DropTable(
                name: "TwoFactorAuths");

            migrationBuilder.DropTable(
                name: "UserGroupMembership");

            migrationBuilder.DropTable(
                name: "PublishedApplications");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
