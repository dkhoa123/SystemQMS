using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class initData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShowName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneHelp = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    EmailHelp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Logo = table.Column<byte[]>(type: "bytea", nullable: true),
                    IsBan = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.UnitId);
                });

            migrationBuilder.CreateTable(
                name: "Counters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CounterId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    CounterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Counters_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StatusActive = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StatusConnect = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Volume = table.Column<int>(type: "integer", nullable: false),
                    Show = table.Column<bool>(type: "boolean", nullable: false),
                    TemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LoginCodeHash = table.Column<string>(type: "text", nullable: true),
                    LoginCodeExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SessionTokenHash = table.Column<string>(type: "text", nullable: true),
                    SessionExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    TemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContentQuestion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationTemplates_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Guests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    GuestId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GuestName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CCCD = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RegisterTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guests_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Form = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TimeSend = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignmentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Image = table.Column<byte[]>(type: "bytea", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SharingLink = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AverageTime = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ticketNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    serviceId = table.Column<int>(type: "integer", nullable: false),
                    CounterId = table.Column<int>(type: "integer", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    handledBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.id);
                    table.ForeignKey(
                        name: "FK_Ticket_Counters_CounterId",
                        column: x => x.CounterId,
                        principalTable: "Counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumberPhone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    Avatar = table.Column<byte[]>(type: "bytea", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CounterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Counters_CounterId",
                        column: x => x.CounterId,
                        principalTable: "Counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CounterDevice",
                columns: table => new
                {
                    CountersId = table.Column<int>(type: "integer", nullable: false),
                    DevicesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterDevice", x => new { x.CountersId, x.DevicesId });
                    table.ForeignKey(
                        name: "FK_CounterDevice_Counters_CountersId",
                        column: x => x.CountersId,
                        principalTable: "Counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CounterDevice_Devices_DevicesId",
                        column: x => x.DevicesId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnitConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    ActiveEvaluationTemplateId = table.Column<int>(type: "integer", nullable: true),
                    BackgroundImage = table.Column<string>(type: "text", nullable: false),
                    BannerText = table.Column<string>(type: "text", nullable: false),
                    ToneColorHex = table.Column<string>(type: "text", nullable: false),
                    ActiveAudioTemplateId = table.Column<int>(type: "integer", nullable: true),
                    Favicon = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Slogan = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Website = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TitleHome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionHome = table.Column<string>(type: "text", nullable: false),
                    BookingMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SlotDuration = table.Column<int>(type: "integer", nullable: true),
                    MaxGuestsPerSlot = table.Column<int>(type: "integer", nullable: true),
                    AdvanceBookingDays = table.Column<int>(type: "integer", nullable: true),
                    AllowImmediateBooking = table.Column<bool>(type: "boolean", nullable: false),
                    MaxBookingsPerUserDaily = table.Column<int>(type: "integer", nullable: true),
                    AppliedScope = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitConfigs_EvaluationTemplates_ActiveEvaluationTemplateId",
                        column: x => x.ActiveEvaluationTemplateId,
                        principalTable: "EvaluationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UnitConfigs_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    NotiId = table.Column<int>(type: "integer", nullable: false),
                    GuestId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histories", x => new { x.NotiId, x.GuestId });
                    table.ForeignKey(
                        name: "FK_Histories_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Histories_Notifications_NotiId",
                        column: x => x.NotiId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CounterService",
                columns: table => new
                {
                    CountersId = table.Column<int>(type: "integer", nullable: false),
                    ServicesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterService", x => new { x.CountersId, x.ServicesId });
                    table.ForeignKey(
                        name: "FK_CounterService_Counters_CountersId",
                        column: x => x.CountersId,
                        principalTable: "Counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CounterService_Services_ServicesId",
                        column: x => x.ServicesId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceService",
                columns: table => new
                {
                    DevicesId = table.Column<int>(type: "integer", nullable: false),
                    ServicesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceService", x => new { x.DevicesId, x.ServicesId });
                    table.ForeignKey(
                        name: "FK_DeviceService_Devices_DevicesId",
                        column: x => x.DevicesId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceService_Services_ServicesId",
                        column: x => x.ServicesId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NumberFomulars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    NumberType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartNumber = table.Column<int>(type: "integer", nullable: false),
                    EndNumber = table.Column<int>(type: "integer", nullable: false),
                    IsDailyLimitEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DailyEndNumber = table.Column<int>(type: "integer", nullable: true),
                    LimitDay = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SequenceMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberFomulars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NumberFomulars_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    DayOfWeek = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceSchedules_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    WordShift = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shedules_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Diaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ModuleActive = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TypeActive = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChangeNumber = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diaries_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Diaries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PublishDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                    table.ForeignKey(
                        name: "FK_News_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_News_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_News_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    SttId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    CounterId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TicketNumber = table.Column<int>(type: "integer", nullable: false),
                    IssuanceTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UseTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CallTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RecallCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Stage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsPriority = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.SttId);
                    table.ForeignKey(
                        name: "FK_Tickets_Counters_CounterId",
                        column: x => x.CounterId,
                        principalTable: "Counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceScheduleShifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    SheduleActiveId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceScheduleShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceScheduleShifts_ServiceSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "ServiceSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceScheduleShifts_Shedules_SheduleActiveId",
                        column: x => x.SheduleActiveId,
                        principalTable: "Shedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuestId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TimeDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SttId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Tickets_SttId",
                        column: x => x.SttId,
                        principalTable: "Tickets",
                        principalColumn: "SttId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CounterChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SttId = table.Column<string>(type: "character varying(100)", nullable: false),
                    CounterIdSender = table.Column<int>(type: "integer", nullable: false),
                    CounterIdReceive = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CounterChanges_Counters_CounterIdReceive",
                        column: x => x.CounterIdReceive,
                        principalTable: "Counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CounterChanges_Counters_CounterIdSender",
                        column: x => x.CounterIdSender,
                        principalTable: "Counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CounterChanges_Tickets_SttId",
                        column: x => x.SttId,
                        principalTable: "Tickets",
                        principalColumn: "SttId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    SttId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    CounterId = table.Column<int>(type: "integer", nullable: false),
                    TemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TimeMark = table.Column<int>(type: "integer", nullable: false),
                    AttitudeMark = table.Column<int>(type: "integer", nullable: false),
                    MaterialMark = table.Column<int>(type: "integer", nullable: false),
                    SatisfedMark = table.Column<int>(type: "integer", nullable: false),
                    Orther = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GuestPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GuestName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EvaluationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluations_Counters_CounterId",
                        column: x => x.CounterId,
                        principalTable: "Counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Evaluations_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Evaluations_Tickets_SttId",
                        column: x => x.SttId,
                        principalTable: "Tickets",
                        principalColumn: "SttId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Evaluations_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_GuestId",
                table: "Bookings",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ServiceId",
                table: "Bookings",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SttId",
                table: "Bookings",
                column: "SttId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterChanges_CounterIdReceive",
                table: "CounterChanges",
                column: "CounterIdReceive");

            migrationBuilder.CreateIndex(
                name: "IX_CounterChanges_CounterIdSender",
                table: "CounterChanges",
                column: "CounterIdSender");

            migrationBuilder.CreateIndex(
                name: "IX_CounterChanges_SttId",
                table: "CounterChanges",
                column: "SttId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterDevice_DevicesId",
                table: "CounterDevice",
                column: "DevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_Counters_CounterId",
                table: "Counters",
                column: "CounterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Counters_UnitId",
                table: "Counters",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterService_ServicesId",
                table: "CounterService",
                column: "ServicesId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceId",
                table: "Devices",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UnitId",
                table: "Devices",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceService_ServicesId",
                table: "DeviceService",
                column: "ServicesId");

            migrationBuilder.CreateIndex(
                name: "IX_Diaries_UnitId",
                table: "Diaries",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Diaries_UserId",
                table: "Diaries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_CounterId",
                table: "Evaluations",
                column: "CounterId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ServiceId",
                table: "Evaluations",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_SttId",
                table: "Evaluations",
                column: "SttId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_UnitId",
                table: "Evaluations",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationTemplates_UnitId",
                table: "EvaluationTemplates",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Guests_UnitId",
                table: "Guests",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_GuestId",
                table: "Histories",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_News_CreatedById",
                table: "News",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_News_UnitId",
                table: "News",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_News_UpdatedById",
                table: "News",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UnitId",
                table: "Notifications",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_NumberFomulars_ServiceId",
                table: "NumberFomulars",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceId",
                table: "Services",
                column: "ServiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_UnitId",
                table: "Services",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSchedules_ServiceId_DayOfWeek",
                table: "ServiceSchedules",
                columns: new[] { "ServiceId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceScheduleShifts_ScheduleId_SheduleActiveId",
                table: "ServiceScheduleShifts",
                columns: new[] { "ScheduleId", "SheduleActiveId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceScheduleShifts_SheduleActiveId",
                table: "ServiceScheduleShifts",
                column: "SheduleActiveId");

            migrationBuilder.CreateIndex(
                name: "IX_Shedules_ServiceId",
                table: "Shedules",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_CounterId",
                table: "Ticket",
                column: "CounterId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CounterId",
                table: "Tickets",
                column: "CounterId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ServiceId",
                table: "Tickets",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UnitId",
                table: "Tickets",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitConfigs_ActiveEvaluationTemplateId",
                table: "UnitConfigs",
                column: "ActiveEvaluationTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitConfigs_UnitId",
                table: "UnitConfigs",
                column: "UnitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CounterId",
                table: "Users",
                column: "CounterId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UnitId",
                table: "Users",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "CounterChanges");

            migrationBuilder.DropTable(
                name: "CounterDevice");

            migrationBuilder.DropTable(
                name: "CounterService");

            migrationBuilder.DropTable(
                name: "DeviceService");

            migrationBuilder.DropTable(
                name: "Diaries");

            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "Histories");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "NumberFomulars");

            migrationBuilder.DropTable(
                name: "ServiceScheduleShifts");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "UnitConfigs");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Guests");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ServiceSchedules");

            migrationBuilder.DropTable(
                name: "Shedules");

            migrationBuilder.DropTable(
                name: "EvaluationTemplates");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Counters");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Units");
        }
    }
}
