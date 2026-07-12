using System;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260712180000_AddGeneralDocument")]
public partial class AddGeneralDocument : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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

        migrationBuilder.CreateTable(
            name: "doc_documents",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                reference_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                reference_id = table.Column<int>(type: "integer", nullable: false),
                category_id = table.Column<int>(type: "integer", nullable: true),
                original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                stored_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                file_extension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                storage_path = table.Column<string>(type: "text", nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                uploaded_by = table.Column<int>(type: "integer", nullable: false),
                uploaded_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_doc_documents", x => x.id);
                table.ForeignKey(
                    name: "fk_doc_documents_doc_document_categories_category_id",
                    column: x => x.category_id,
                    principalSchema: "public",
                    principalTable: "doc_document_categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_doc_documents_sys_users_uploaded_by",
                    column: x => x.uploaded_by,
                    principalSchema: "public",
                    principalTable: "sys_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
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

        migrationBuilder.CreateIndex(
            name: "ix_doc_documents_category_id",
            schema: "public",
            table: "doc_documents",
            column: "category_id");

        migrationBuilder.CreateIndex(
            name: "ix_doc_documents_created_at",
            schema: "public",
            table: "doc_documents",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_doc_documents_is_deleted",
            schema: "public",
            table: "doc_documents",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_doc_documents_reference_type_reference_id",
            schema: "public",
            table: "doc_documents",
            columns: new[] { "reference_type", "reference_id" });

        migrationBuilder.CreateIndex(
            name: "ix_doc_documents_uploaded_by",
            schema: "public",
            table: "doc_documents",
            column: "uploaded_by");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "doc_documents",
            schema: "public");

        migrationBuilder.DropTable(
            name: "doc_document_categories",
            schema: "public");
    }
}
