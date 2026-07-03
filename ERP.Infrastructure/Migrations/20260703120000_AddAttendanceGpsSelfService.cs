using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260703120000_AddAttendanceGpsSelfService")]
public partial class AddAttendanceGpsSelfService : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
ALTER TABLE public.hr_attendance_settings ADD COLUMN IF NOT EXISTS office_latitude numeric(9,6);
ALTER TABLE public.hr_attendance_settings ADD COLUMN IF NOT EXISTS office_longitude numeric(9,6);
ALTER TABLE public.hr_attendance_settings ADD COLUMN IF NOT EXISTS radius_meters integer NOT NULL DEFAULT 100;

ALTER TABLE public.hr_attendance_records ADD COLUMN IF NOT EXISTS check_in_latitude numeric(9,6);
ALTER TABLE public.hr_attendance_records ADD COLUMN IF NOT EXISTS check_in_longitude numeric(9,6);
ALTER TABLE public.hr_attendance_records ADD COLUMN IF NOT EXISTS check_out_latitude numeric(9,6);
ALTER TABLE public.hr_attendance_records ADD COLUMN IF NOT EXISTS check_out_longitude numeric(9,6);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'ck_hr_attendance_settings_radius_meters_positive'
    ) THEN
        ALTER TABLE public.hr_attendance_settings
            ADD CONSTRAINT ck_hr_attendance_settings_radius_meters_positive
            CHECK (radius_meters > 0);
    END IF;
END $$;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
ALTER TABLE public.hr_attendance_settings DROP CONSTRAINT IF EXISTS ck_hr_attendance_settings_radius_meters_positive;

ALTER TABLE public.hr_attendance_settings DROP COLUMN IF EXISTS office_latitude;
ALTER TABLE public.hr_attendance_settings DROP COLUMN IF EXISTS office_longitude;
ALTER TABLE public.hr_attendance_settings DROP COLUMN IF EXISTS radius_meters;

ALTER TABLE public.hr_attendance_records DROP COLUMN IF EXISTS check_in_latitude;
ALTER TABLE public.hr_attendance_records DROP COLUMN IF EXISTS check_in_longitude;
ALTER TABLE public.hr_attendance_records DROP COLUMN IF EXISTS check_out_latitude;
ALTER TABLE public.hr_attendance_records DROP COLUMN IF EXISTS check_out_longitude;
""");
    }
}
