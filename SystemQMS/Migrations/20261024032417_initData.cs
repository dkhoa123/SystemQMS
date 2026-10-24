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
                name: "devices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    deviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    deviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    deviceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    statusActive = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    statusConnect = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ipAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    volume = table.Column<int>(type: "integer", nullable: false),
                    show = table.Column<bool>(type: "boolean", nullable: false),
                    loginCodeHash = table.Column<string>(type: "text", nullable: true),
                    loginCodeExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sessionTokenHash = table.Column<string>(type: "text", nullable: true),
                    sessionExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    roleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    roleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.roleId);
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    unitId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    unitName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    showName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phoneHelp = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    emailHelp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    logo = table.Column<byte[]>(type: "bytea", nullable: true),
                    isBan = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.unitId);
                });

            migrationBuilder.CreateTable(
                name: "counters",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    counterId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    unitId = table.Column<int>(type: "integer", nullable: false),
                    counterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counters", x => x.id);
                    table.ForeignKey(
                        name: "FK_counters_units_unitId",
                        column: x => x.unitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evaluationTemplates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    unitId = table.Column<int>(type: "integer", nullable: false),
                    templateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    templateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    contentQuestion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluationTemplates", x => x.id);
                    table.ForeignKey(
                        name: "FK_evaluationTemplates_units_unitId",
                        column: x => x.unitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    unitId = table.Column<int>(type: "integer", nullable: false),
                    guestId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    guestName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phoneNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CCCD = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    registerTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guests", x => x.id);
                    table.ForeignKey(
                        name: "FK_guests_units_unitId",
                        column: x => x.unitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    unitId = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    form = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    timeSend = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_units_unitId",
                        column: x => x.unitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    unitId = table.Column<int>(type: "integer", nullable: false),
                    serviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    serviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    image = table.Column<byte[]>(type: "bytea", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    sharingLink = table.Column<string>(type: "text", nullable: false),
                    averageTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.id);
                    table.ForeignKey(
                        name: "FK_services_units_unitId",
                        column: x => x.unitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CounterDevice",
                columns: table => new
                {
                    Countersid = table.Column<int>(type: "integer", nullable: false),
                    Devicesid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterDevice", x => new { x.Countersid, x.Devicesid });
                    table.ForeignKey(
                        name: "FK_CounterDevice_counters_Countersid",
                        column: x => x.Countersid,
                        principalTable: "counters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CounterDevice_devices_Devicesid",
                        column: x => x.Devicesid,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    userId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    unitId = table.Column<int>(type: "integer", nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    numberPhone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    roleId = table.Column<int>(type: "integer", nullable: false),
                    avatar = table.Column<byte[]>(type: "bytea", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    counterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.userId);
                    table.ForeignKey(
                        name: "FK_users_counters_counterId",
                        column: x => x.counterId,
                        principalTable: "counters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_roles_roleId",
                        column: x => x.roleId,
                        principalTable: "roles",
                        principalColumn: "roleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_units_unitId",
                        column: x => x.unitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "histories",
                columns: table => new
                {
                    notiId = table.Column<int>(type: "integer", nullable: false),
                    guestId = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_histories", x => new { x.notiId, x.guestId });
                    table.ForeignKey(
                        name: "FK_histories_guests_guestId",
                        column: x => x.guestId,
                        principalTable: "guests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_histories_notifications_notiId",
                        column: x => x.notiId,
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CounterService",
                columns: table => new
                {
                    Servicesid = table.Column<int>(type: "integer", nullable: false),
                    countersid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterService", x => new { x.Servicesid, x.countersid });
                    table.ForeignKey(
                        name: "FK_CounterService_counters_countersid",
                        column: x => x.countersid,
                        principalTable: "counters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CounterService_services_Servicesid",
                        column: x => x.Servicesid,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceService",
                columns: table => new
                {
                    Devicesid = table.Column<int>(type: "integer", nullable: false),
                    Servicesid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceService", x => new { x.Devicesid, x.Servicesid });
                    table.ForeignKey(
                        name: "FK_DeviceService_devices_Devicesid",
                        column: x => x.Devicesid,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceService_services_Servicesid",
                        column: x => x.Servicesid,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "numberFomulars",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    serviceId = table.Column<int>(type: "integer", nullable: false),
                    numberType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    firstName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    lastName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    startNumber = table.Column<int>(type: "integer", nullable: false),
                    endNumber = table.Column<int>(type: "integer", nullable: false),
                    limitDay = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_numberFomulars", x => x.id);
                    table.ForeignKey(
                        name: "FK_numberFomulars_services_serviceId",
                        column: x => x.serviceId,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shedules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    serviceId = table.Column<int>(type: "integer", nullable: false),
                    wordShift = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    startTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    endTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_shedules_services_serviceId",
                        column: x => x.serviceId,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "diaries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    unitId = table.Column<int>(type: "integer", nullable: false),
                    userId = table.Column<int>(type: "integer", nullable: false),
                    moduleActive = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    typeActive = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    changeNumber = table.Column<int>(type: "integer", nullable: false),
                    time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diaries", x => x.id);
                    table.ForeignKey(
                        name: "FK_diaries_units_unitId",
                        column: x => x.unitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_diaries_users_userId",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stickers",
                columns: table => new
                {
                    sttId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    unitId = table.Column<int>(type: "integer", nullable: false),
                    serviceId = table.Column<int>(type: "integer", nullable: false),
                    counterId = table.Column<int>(type: "integer", nullable: false),
                    userId = table.Column<int>(type: "integer", nullable: false),
                    issuanceTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    useTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    stage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stickers", x => x.sttId);
                    table.ForeignKey(
                        name: "FK_stickers_counters_counterId",
                        column: x => x.counterId,
                        principalTable: "counters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stickers_services_serviceId",
                        column: x => x.serviceId,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stickers_units_unitId",
                        column: x => x.unitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stickers_users_userId",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guestId = table.Column<int>(type: "integer", nullable: false),
                    serviceId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    timeDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sttId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.id);
                    table.ForeignKey(
                        name: "FK_bookings_guests_guestId",
                        column: x => x.guestId,
                        principalTable: "guests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bookings_services_serviceId",
                        column: x => x.serviceId,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bookings_stickers_sttId",
                        column: x => x.sttId,
                        principalTable: "stickers",
                        principalColumn: "sttId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "counterChanges",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    sttId = table.Column<string>(type: "character varying(100)", nullable: false),
                    counterIdSender = table.Column<int>(type: "integer", nullable: false),
                    counterIdReceive = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    createTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    counterSenderId = table.Column<int>(type: "integer", nullable: false),
                    counterIdReceiver = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counterChanges", x => x.id);
                    table.ForeignKey(
                        name: "FK_counterChanges_counters_counterSenderId",
                        column: x => x.counterSenderId,
                        principalTable: "counters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_counterChanges_counters_id",
                        column: x => x.id,
                        principalTable: "counters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_counterChanges_stickers_sttId",
                        column: x => x.sttId,
                        principalTable: "stickers",
                        principalColumn: "sttId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evaluations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    unitId = table.Column<int>(type: "integer", nullable: false),
                    sttId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    serviceId = table.Column<int>(type: "integer", nullable: false),
                    counterId = table.Column<int>(type: "integer", nullable: false),
                    templateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    timeMark = table.Column<int>(type: "integer", nullable: false),
                    attitudeMark = table.Column<int>(type: "integer", nullable: false),
                    materialMark = table.Column<int>(type: "integer", nullable: false),
                    satisfedMark = table.Column<int>(type: "integer", nullable: false),
                    orther = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    guestPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    guestName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    evaluationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluations", x => x.id);
                    table.ForeignKey(
                        name: "FK_evaluations_counters_counterId",
                        column: x => x.counterId,
                        principalTable: "counters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evaluations_services_serviceId",
                        column: x => x.serviceId,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evaluations_stickers_sttId",
                        column: x => x.sttId,
                        principalTable: "stickers",
                        principalColumn: "sttId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evaluations_units_unitId",
                        column: x => x.unitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_guestId",
                table: "bookings",
                column: "guestId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_serviceId",
                table: "bookings",
                column: "serviceId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_sttId",
                table: "bookings",
                column: "sttId");

            migrationBuilder.CreateIndex(
                name: "IX_counterChanges_counterSenderId",
                table: "counterChanges",
                column: "counterSenderId");

            migrationBuilder.CreateIndex(
                name: "IX_counterChanges_sttId",
                table: "counterChanges",
                column: "sttId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterDevice_Devicesid",
                table: "CounterDevice",
                column: "Devicesid");

            migrationBuilder.CreateIndex(
                name: "IX_counters_unitId",
                table: "counters",
                column: "unitId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterService_countersid",
                table: "CounterService",
                column: "countersid");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceService_Servicesid",
                table: "DeviceService",
                column: "Servicesid");

            migrationBuilder.CreateIndex(
                name: "IX_diaries_unitId",
                table: "diaries",
                column: "unitId");

            migrationBuilder.CreateIndex(
                name: "IX_diaries_userId",
                table: "diaries",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_evaluations_counterId",
                table: "evaluations",
                column: "counterId");

            migrationBuilder.CreateIndex(
                name: "IX_evaluations_serviceId",
                table: "evaluations",
                column: "serviceId");

            migrationBuilder.CreateIndex(
                name: "IX_evaluations_sttId",
                table: "evaluations",
                column: "sttId");

            migrationBuilder.CreateIndex(
                name: "IX_evaluations_unitId",
                table: "evaluations",
                column: "unitId");

            migrationBuilder.CreateIndex(
                name: "IX_evaluationTemplates_unitId",
                table: "evaluationTemplates",
                column: "unitId");

            migrationBuilder.CreateIndex(
                name: "IX_guests_unitId",
                table: "guests",
                column: "unitId");

            migrationBuilder.CreateIndex(
                name: "IX_histories_guestId",
                table: "histories",
                column: "guestId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_unitId",
                table: "notifications",
                column: "unitId");

            migrationBuilder.CreateIndex(
                name: "IX_numberFomulars_serviceId",
                table: "numberFomulars",
                column: "serviceId");

            migrationBuilder.CreateIndex(
                name: "IX_services_unitId",
                table: "services",
                column: "unitId");

            migrationBuilder.CreateIndex(
                name: "IX_shedules_serviceId",
                table: "shedules",
                column: "serviceId");

            migrationBuilder.CreateIndex(
                name: "IX_stickers_counterId",
                table: "stickers",
                column: "counterId");

            migrationBuilder.CreateIndex(
                name: "IX_stickers_serviceId",
                table: "stickers",
                column: "serviceId");

            migrationBuilder.CreateIndex(
                name: "IX_stickers_unitId",
                table: "stickers",
                column: "unitId");

            migrationBuilder.CreateIndex(
                name: "IX_stickers_userId",
                table: "stickers",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_users_counterId",
                table: "users",
                column: "counterId");

            migrationBuilder.CreateIndex(
                name: "IX_users_roleId",
                table: "users",
                column: "roleId");

            migrationBuilder.CreateIndex(
                name: "IX_users_unitId",
                table: "users",
                column: "unitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "counterChanges");

            migrationBuilder.DropTable(
                name: "CounterDevice");

            migrationBuilder.DropTable(
                name: "CounterService");

            migrationBuilder.DropTable(
                name: "DeviceService");

            migrationBuilder.DropTable(
                name: "diaries");

            migrationBuilder.DropTable(
                name: "evaluations");

            migrationBuilder.DropTable(
                name: "evaluationTemplates");

            migrationBuilder.DropTable(
                name: "histories");

            migrationBuilder.DropTable(
                name: "numberFomulars");

            migrationBuilder.DropTable(
                name: "shedules");

            migrationBuilder.DropTable(
                name: "devices");

            migrationBuilder.DropTable(
                name: "stickers");

            migrationBuilder.DropTable(
                name: "guests");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "counters");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "units");
        }
    }
}
