using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttendancePeriodToDayRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attendance_period",
                schema: "public",
                table: "hr_attendance_settings");

            migrationBuilder.AddColumn<int>(
                name: "attendance_period_end_day",
                schema: "public",
                table: "hr_attendance_settings",
                type: "integer",
                nullable: false,
                defaultValue: 25);

            migrationBuilder.AddColumn<int>(
                name: "attendance_period_start_day",
                schema: "public",
                table: "hr_attendance_settings",
                type: "integer",
                nullable: false,
                defaultValue: 26);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attendance_period_end_day",
                schema: "public",
                table: "hr_attendance_settings");

            migrationBuilder.DropColumn(
                name: "attendance_period_start_day",
                schema: "public",
                table: "hr_attendance_settings");

            migrationBuilder.AddColumn<string>(
                name: "attendance_period",
                schema: "public",
                table: "hr_attendance_settings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
