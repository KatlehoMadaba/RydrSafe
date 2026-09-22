using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RydrSafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentDatePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "Day", not "". Every report written before this column existed carried an exact
            // date, so that is what they keep meaning — and an empty string would not parse back
            // into IncidentDatePrecision, breaking every read of the Reports table.
            migrationBuilder.AddColumn<string>(
                name: "IncidentDatePrecision",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "Day");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncidentDatePrecision",
                table: "Reports");
        }
    }
}
