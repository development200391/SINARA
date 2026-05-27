using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHrAttendanceSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hr_attendance_settings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    singleton_key = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "default"),
                    attendance_period = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    check_in_tolerance_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    late_tolerance_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    work_start = table.Column<TimeOnly>(type: "time", nullable: false),
                    work_end = table.Column<TimeOnly>(type: "time", nullable: false),
                    break_start = table.Column<TimeOnly>(type: "time", nullable: false),
                    break_end = table.Column<TimeOnly>(type: "time", nullable: false),
                    minimum_ot_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hr_attendance_settings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hr_attendance_settings_created_at",
                schema: "public",
                table: "hr_attendance_settings",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_hr_attendance_settings_is_deleted",
                schema: "public",
                table: "hr_attendance_settings",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_hr_attendance_settings_singleton_key",
                schema: "public",
                table: "hr_attendance_settings",
                column: "singleton_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hr_attendance_settings",
                schema: "public");
        }
    }
}
