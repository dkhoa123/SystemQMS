using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUnitConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitConfig_evaluationTemplates_ActiveEvaluationTemplateId",
                table: "UnitConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitConfig_units_UnitId",
                table: "UnitConfig");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UnitConfig",
                table: "UnitConfig");

            migrationBuilder.RenameTable(
                name: "UnitConfig",
                newName: "unitConfigs");

            migrationBuilder.RenameIndex(
                name: "IX_UnitConfig_UnitId",
                table: "unitConfigs",
                newName: "IX_unitConfigs_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_UnitConfig_ActiveEvaluationTemplateId",
                table: "unitConfigs",
                newName: "IX_unitConfigs_ActiveEvaluationTemplateId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_unitConfigs",
                table: "unitConfigs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_unitConfigs_evaluationTemplates_ActiveEvaluationTemplateId",
                table: "unitConfigs",
                column: "ActiveEvaluationTemplateId",
                principalTable: "evaluationTemplates",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_unitConfigs_units_UnitId",
                table: "unitConfigs",
                column: "UnitId",
                principalTable: "units",
                principalColumn: "unitId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_unitConfigs_evaluationTemplates_ActiveEvaluationTemplateId",
                table: "unitConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_unitConfigs_units_UnitId",
                table: "unitConfigs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_unitConfigs",
                table: "unitConfigs");

            migrationBuilder.RenameTable(
                name: "unitConfigs",
                newName: "UnitConfig");

            migrationBuilder.RenameIndex(
                name: "IX_unitConfigs_UnitId",
                table: "UnitConfig",
                newName: "IX_UnitConfig_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_unitConfigs_ActiveEvaluationTemplateId",
                table: "UnitConfig",
                newName: "IX_UnitConfig_ActiveEvaluationTemplateId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UnitConfig",
                table: "UnitConfig",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UnitConfig_evaluationTemplates_ActiveEvaluationTemplateId",
                table: "UnitConfig",
                column: "ActiveEvaluationTemplateId",
                principalTable: "evaluationTemplates",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_UnitConfig_units_UnitId",
                table: "UnitConfig",
                column: "UnitId",
                principalTable: "units",
                principalColumn: "unitId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
