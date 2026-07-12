using System;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260712200000_SplitDocumentReferenceTypeConfigDetails")]
public partial class SplitDocumentReferenceTypeConfigDetails : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_multiple",
            schema: "public",
            table: "doc_reference_type_configs",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.Sql(
            "UPDATE public.doc_reference_type_configs SET is_multiple = TRUE WHERE max_file_count > 1;");

        migrationBuilder.CreateTable(
            name: "doc_reference_type_config_details",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                config_id = table.Column<int>(type: "integer", nullable: false),
                sort_order = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                max_file_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                allowed_extensions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_doc_reference_type_config_details", x => x.id);
                table.ForeignKey(
                    name: "fk_doc_reference_type_config_details_doc_reference_type_configs_config_id",
                    column: x => x.config_id,
                    principalSchema: "public",
                    principalTable: "doc_reference_type_configs",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_doc_reference_type_config_details_config_id",
            schema: "public",
            table: "doc_reference_type_config_details",
            column: "config_id");

        migrationBuilder.CreateIndex(
            name: "ix_doc_reference_type_config_details_created_at",
            schema: "public",
            table: "doc_reference_type_config_details",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_doc_reference_type_config_details_is_deleted",
            schema: "public",
            table: "doc_reference_type_config_details",
            column: "is_deleted");

        // Preserve currently-configured rules as detail rows before the flat
        // columns they came from are dropped below, so existing admin
        // configuration (e.g. hr_leave_requests) isn't silently discarded.
        migrationBuilder.Sql(@"
            INSERT INTO public.doc_reference_type_config_details
                (config_id, sort_order, name, max_file_size_bytes, is_required, is_active, allowed_extensions, created_by, created_at)
            SELECT
                c.id,
                gs.i - 1,
                CASE WHEN c.max_file_count = 1 THEN c.display_name ELSE c.display_name || ' ' || gs.i END,
                c.max_file_size_bytes,
                c.is_required,
                TRUE,
                c.allowed_extensions,
                'system',
                now()
            FROM public.doc_reference_type_configs c
            CROSS JOIN LATERAL generate_series(1, c.max_file_count) AS gs(i);");

        migrationBuilder.DropColumn(
            name: "is_required",
            schema: "public",
            table: "doc_reference_type_configs");

        migrationBuilder.DropColumn(
            name: "max_file_size_bytes",
            schema: "public",
            table: "doc_reference_type_configs");

        migrationBuilder.DropColumn(
            name: "allowed_extensions",
            schema: "public",
            table: "doc_reference_type_configs");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_required",
            schema: "public",
            table: "doc_reference_type_configs",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<long>(
            name: "max_file_size_bytes",
            schema: "public",
            table: "doc_reference_type_configs",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "allowed_extensions",
            schema: "public",
            table: "doc_reference_type_configs",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.DropTable(
            name: "doc_reference_type_config_details",
            schema: "public");

        migrationBuilder.DropColumn(
            name: "is_multiple",
            schema: "public",
            table: "doc_reference_type_configs");
    }
}
