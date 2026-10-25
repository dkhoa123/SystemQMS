using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class AddSequenceModeAndDailyLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "voice",
                table: "devices");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "counters",
                newName: "status");

            migrationBuilder.AddColumn<string>(
                name: "sequenceMode",
                table: "numberFomulars",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ipAddress",
                table: "devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "volume",
                table: "devices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_CounterDevice_Devicesid",
                table: "CounterDevice",
                column: "Devicesid");

            migrationBuilder.CreateIndex(
                name: "IX_CounterService_countersid",
                table: "CounterService",
                column: "countersid");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceService_Servicesid",
                table: "DeviceService",
                column: "Servicesid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CounterDevice");

            migrationBuilder.DropTable(
                name: "CounterService");

            migrationBuilder.DropTable(
                name: "DeviceService");

            migrationBuilder.DropColumn(
                name: "sequenceMode",
                table: "numberFomulars");

            migrationBuilder.DropColumn(
                name: "volume",
                table: "devices");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "counters",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "ipAddress",
                table: "devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "voice",
                table: "devices",
                type: "bytea",
                nullable: true);
        }
    }
}
