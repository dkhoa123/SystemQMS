using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class updateDB1107 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_counterChanges_counters_counterSenderId",
                table: "counterChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_counterChanges_counters_id",
                table: "counterChanges");

            migrationBuilder.DropIndex(
                name: "IX_counterChanges_counterSenderId",
                table: "counterChanges");

            migrationBuilder.DropColumn(
                name: "counterIdReceiver",
                table: "counterChanges");

            migrationBuilder.DropColumn(
                name: "counterSenderId",
                table: "counterChanges");

            migrationBuilder.AddColumn<bool>(
                name: "isPriority",
                table: "stickers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ticketNumber",
                table: "stickers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "counterChanges",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateTable(
                name: "Tickets",
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
                    table.PrimaryKey("PK_Tickets", x => x.id);
                    table.ForeignKey(
                        name: "FK_Tickets_counters_CounterId",
                        column: x => x.CounterId,
                        principalTable: "counters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_counterChanges_counterIdReceive",
                table: "counterChanges",
                column: "counterIdReceive");

            migrationBuilder.CreateIndex(
                name: "IX_counterChanges_counterIdSender",
                table: "counterChanges",
                column: "counterIdSender");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CounterId",
                table: "Tickets",
                column: "CounterId");

            migrationBuilder.AddForeignKey(
                name: "FK_counterChanges_counters_counterIdReceive",
                table: "counterChanges",
                column: "counterIdReceive",
                principalTable: "counters",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_counterChanges_counters_counterIdSender",
                table: "counterChanges",
                column: "counterIdSender",
                principalTable: "counters",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_counterChanges_counters_counterIdReceive",
                table: "counterChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_counterChanges_counters_counterIdSender",
                table: "counterChanges");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_counterChanges_counterIdReceive",
                table: "counterChanges");

            migrationBuilder.DropIndex(
                name: "IX_counterChanges_counterIdSender",
                table: "counterChanges");

            migrationBuilder.DropColumn(
                name: "isPriority",
                table: "stickers");

            migrationBuilder.DropColumn(
                name: "ticketNumber",
                table: "stickers");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "counterChanges",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "counterIdReceiver",
                table: "counterChanges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "counterSenderId",
                table: "counterChanges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_counterChanges_counterSenderId",
                table: "counterChanges",
                column: "counterSenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_counterChanges_counters_counterSenderId",
                table: "counterChanges",
                column: "counterSenderId",
                principalTable: "counters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_counterChanges_counters_id",
                table: "counterChanges",
                column: "id",
                principalTable: "counters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
