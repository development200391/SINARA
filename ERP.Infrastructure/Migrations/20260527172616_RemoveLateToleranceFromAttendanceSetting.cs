using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLateToleranceFromAttendanceSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "late_tolerance_minutes",
                schema: "public",
                table: "hr_attendance_settings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "late_tolerance_minutes",
                schema: "public",
                table: "hr_attendance_settings",
                type: "integer",
                nullable: false,
                defaultValue: 15);
        }
    }
}
