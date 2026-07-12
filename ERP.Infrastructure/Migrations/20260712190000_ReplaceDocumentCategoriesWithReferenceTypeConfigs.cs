using System;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260712190000_ReplaceDocumentCategoriesWithReferenceTypeConfigs")]
public partial class ReplaceDocumentCategoriesWithReferenceTypeConfigs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_doc_documents_doc_document_categories_category_id",
            schema: "public",
            table: "doc_documents");

        migrationBuilder.DropIndex(
            name: "ix_doc_documents_category_id",
            schema: "public",
            table: "doc_documents");

        migrationBuilder.DropColumn(
            name: "category_id",
            schema: "public",
            table: "doc_documents");

        migrationBuilder.DropTable(
            name: "doc_document_categories",
            schema: "public");

        migrationBuilder.CreateTable(
            name: "doc_reference_type_configs",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                reference_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                display_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                max_file_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                max_file_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                allowed_extensions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_doc_reference_type_configs", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_doc_reference_type_configs_reference_type",
            schema: "public",
            table: "doc_reference_type_configs",
            column: "reference_type",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "doc_reference_type_configs",
            schema: "public");

        migrationBuilder.CreateTable(
            name: "doc_document_categories",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_doc_document_categories", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_doc_document_categories_code",
            schema: "public",
            table: "doc_document_categories",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_doc_document_categories_created_at",
            schema: "public",
            table: "doc_document_categories",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_doc_document_categories_is_deleted",
            schema: "public",
            table: "doc_document_categories",
            column: "is_deleted");

        migrationBuilder.AddColumn<int>(
            name: "category_id",
            schema: "public",
            table: "doc_documents",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_doc_documents_category_id",
            schema: "public",
            table: "doc_documents",
            column: "category_id");

        migrationBuilder.AddForeignKey(
            name: "fk_doc_documents_doc_document_categories_category_id",
            schema: "public",
            table: "doc_documents",
            column: "category_id",
            principalSchema: "public",
            principalTable: "doc_document_categories",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);
    }
}
