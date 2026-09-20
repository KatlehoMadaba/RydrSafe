using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RydrSafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnonymousReportingAndAccountDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Reports",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReporterDeletedAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReporterKeyHash",
                table: "Reports",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReporterDeletedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReporterKeyHash",
                table: "Reports");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Reports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
