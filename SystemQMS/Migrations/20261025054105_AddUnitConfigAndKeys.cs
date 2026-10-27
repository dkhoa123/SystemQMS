using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitConfigAndKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnitConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    ActiveEvaluationTemplateId = table.Column<int>(type: "integer", nullable: true),
                    BackgroundImage = table.Column<string>(type: "text", nullable: false),
                    BannerText = table.Column<string>(type: "text", nullable: false),
                    ToneColorHex = table.Column<string>(type: "text", nullable: false),
                    ActiveAudioTemplateId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitConfig_evaluationTemplates_ActiveEvaluationTemplateId",
                        column: x => x.ActiveEvaluationTemplateId,
                        principalTable: "evaluationTemplates",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_UnitConfig_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitConfig_ActiveEvaluationTemplateId",
                table: "UnitConfig",
                column: "ActiveEvaluationTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitConfig_UnitId",
                table: "UnitConfig",
                column: "UnitId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnitConfig");
        }
    }
}
