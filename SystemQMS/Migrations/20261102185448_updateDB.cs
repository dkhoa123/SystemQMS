using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class updateDB : Migration
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
