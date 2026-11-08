using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class AddIsVerifiedToPasswordResetOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "passwordResetOtps",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "passwordResetOtps");
        }
    }
}
