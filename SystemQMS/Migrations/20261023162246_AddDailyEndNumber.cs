using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyEndNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "dailyEndNumber",
                table: "numberFomulars",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDailyLimitEnabled",
                table: "numberFomulars",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dailyEndNumber",
                table: "numberFomulars");

            migrationBuilder.DropColumn(
                name: "isDailyLimitEnabled",
                table: "numberFomulars");
        }
    }
}
