using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullUnitIdInGuest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guests_units_unitId",
                table: "guests");

            migrationBuilder.AlterColumn<int>(
                name: "unitId",
                table: "guests",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_guests_units_unitId",
                table: "guests",
                column: "unitId",
                principalTable: "units",
                principalColumn: "unitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guests_units_unitId",
                table: "guests");

            migrationBuilder.AlterColumn<int>(
                name: "unitId",
                table: "guests",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_guests_units_unitId",
                table: "guests",
                column: "unitId",
                principalTable: "units",
                principalColumn: "unitId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
