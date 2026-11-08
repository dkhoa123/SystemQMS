using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class MakeUnitIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_units_unitId",
                table: "users");

            migrationBuilder.AlterColumn<int>(
                name: "unitId",
                table: "users",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_users_units_unitId",
                table: "users",
                column: "unitId",
                principalTable: "units",
                principalColumn: "unitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_units_unitId",
                table: "users");

            migrationBuilder.AlterColumn<int>(
                name: "unitId",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_users_units_unitId",
                table: "users",
                column: "unitId",
                principalTable: "units",
                principalColumn: "unitId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
