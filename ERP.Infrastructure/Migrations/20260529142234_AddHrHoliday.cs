using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHrHoliday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.holiday_type_enum", "national,company,joint_leave");

            migrationBuilder.CreateTable(
                name: "hr_holiday",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    holiday_date = table.Column<DateOnly>(type: "date", nullable: false),
                    holiday_type = table.Column<string>(type: "holiday_type_enum", nullable: false, defaultValueSql: "'national'::holiday_type_enum"),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    applies_to = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "all"),
                    year = table.Column<short>(type: "smallint", nullable: false, computedColumnSql: "EXTRACT(YEAR FROM holiday_date)::smallint", stored: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hr_holiday", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hr_holiday_created_at",
                schema: "public",
                table: "hr_holiday",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_hr_holiday_holiday_date",
                schema: "public",
                table: "hr_holiday",
                column: "holiday_date");

            migrationBuilder.CreateIndex(
                name: "ix_hr_holiday_holiday_type",
                schema: "public",
                table: "hr_holiday",
                column: "holiday_type");

            migrationBuilder.CreateIndex(
                name: "ix_hr_holiday_is_active",
                schema: "public",
                table: "hr_holiday",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_hr_holiday_is_deleted",
                schema: "public",
                table: "hr_holiday",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_hr_holiday_name_holiday_date",
                schema: "public",
                table: "hr_holiday",
                columns: new[] { "name", "holiday_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_holiday_year",
                schema: "public",
                table: "hr_holiday",
                column: "year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hr_holiday",
                schema: "public");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:public.holiday_type_enum", "national,company,joint_leave");
        }
    }
}
