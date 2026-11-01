using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class AddCounterSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stickers_counters_counterId",
                table: "stickers");

            migrationBuilder.DropForeignKey(
                name: "FK_users_counters_counterId",
                table: "users");

            migrationBuilder.CreateTable(
                name: "counterSessions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    counterId = table.Column<int>(type: "integer", nullable: false),
                    userId = table.Column<int>(type: "integer", nullable: false),
                    deviceId = table.Column<int>(type: "integer", nullable: false),
                    startedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    endedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counterSessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_counterSessions_counters_counterId",
                        column: x => x.counterId,
                        principalTable: "counters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_counterSessions_devices_deviceId",
                        column: x => x.deviceId,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_counterSessions_users_userId",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_counterSessions_counterId",
                table: "counterSessions",
                column: "counterId");

            migrationBuilder.CreateIndex(
                name: "IX_counterSessions_deviceId",
                table: "counterSessions",
                column: "deviceId");

            migrationBuilder.CreateIndex(
                name: "IX_counterSessions_userId",
                table: "counterSessions",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_stickers_counters_counterId",
                table: "stickers",
                column: "counterId",
                principalTable: "counters",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_counters_counterId",
                table: "users",
                column: "counterId",
                principalTable: "counters",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stickers_counters_counterId",
                table: "stickers");

            migrationBuilder.DropForeignKey(
                name: "FK_users_counters_counterId",
                table: "users");

            migrationBuilder.DropTable(
                name: "counterSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_stickers_counters_counterId",
                table: "stickers",
                column: "counterId",
                principalTable: "counters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_counters_counterId",
                table: "users",
                column: "counterId",
                principalTable: "counters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
