using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowOnlineBookingToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "allowOnlineBooking",
                table: "services",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "allowOnlineBooking",
                table: "services");
        }
    }
}
