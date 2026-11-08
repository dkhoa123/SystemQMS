using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class AddPurposeAndNewEmailToPasswordResetOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "revokedTokens",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "NewEmail",
                table: "passwordResetOtps",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "passwordResetOtps",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewEmail",
                table: "passwordResetOtps");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "passwordResetOtps");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "revokedTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }
    }
}
