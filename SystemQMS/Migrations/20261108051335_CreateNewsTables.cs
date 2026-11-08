using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "news",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    unitId = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    timeSend = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expiredTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news", x => x.Id);
                    table.ForeignKey(
                        name: "FK_news_units_unitId",
                        column: x => x.unitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "newsAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    newsId = table.Column<int>(type: "integer", nullable: false),
                    fileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    fileUrl = table.Column<string>(type: "text", nullable: false),
                    fileType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_newsAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_newsAttachments_news_newsId",
                        column: x => x.newsId,
                        principalTable: "news",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_news_unitId",
                table: "news",
                column: "unitId");

            migrationBuilder.CreateIndex(
                name: "IX_newsAttachments_newsId",
                table: "newsAttachments",
                column: "newsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "newsAttachments");

            migrationBuilder.DropTable(
                name: "news");
        }
    }
}
