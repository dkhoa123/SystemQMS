using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemQMS.Migrations
{
    /// <inheritdoc />
    public partial class AddWebAppConfigAndNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdvanceBookingDays",
                table: "unitConfigs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowImmediateBooking",
                table: "unitConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AppliedScope",
                table: "unitConfigs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BookingMethod",
                table: "unitConfigs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionHome",
                table: "unitConfigs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Favicon",
                table: "unitConfigs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxBookingsPerUserDaily",
                table: "unitConfigs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxGuestsPerSlot",
                table: "unitConfigs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slogan",
                table: "unitConfigs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SlotDuration",
                table: "unitConfigs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleHome",
                table: "unitConfigs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "unitConfigs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "notifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PublishDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                    table.ForeignKey(
                        name: "FK_News_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "units",
                        principalColumn: "unitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_News_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "users",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_News_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "users",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_News_CreatedById",
                table: "News",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_News_UnitId",
                table: "News",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_News_UpdatedById",
                table: "News",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropColumn(
                name: "AdvanceBookingDays",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "AllowImmediateBooking",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "AppliedScope",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "BookingMethod",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "DescriptionHome",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "Favicon",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "MaxBookingsPerUserDaily",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "MaxGuestsPerSlot",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "Slogan",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "SlotDuration",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "TitleHome",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "unitConfigs");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "bookings");
        }
    }
}
