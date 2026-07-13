using System;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260713100000_AddGeneralApproval")]
public partial class AddGeneralApproval : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "apv_approval_templates",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                reference_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                approval_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                min_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                max_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                auto_approve_below = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                sla_hours = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                allow_delegation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                require_comment_on_reject = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                table.PrimaryKey("pk_apv_approval_templates", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "apv_delegations",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                delegator_user_id = table.Column<int>(type: "integer", nullable: false),
                delegate_user_id = table.Column<int>(type: "integer", nullable: false),
                template_id = table.Column<int>(type: "integer", nullable: true),
                start_date = table.Column<DateOnly>(type: "date", nullable: false),
                end_date = table.Column<DateOnly>(type: "date", nullable: false),
                reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                table.PrimaryKey("pk_apv_delegations", x => x.id);
                table.ForeignKey(
                    name: "fk_apv_delegations_sys_users_delegator_user_id",
                    column: x => x.delegator_user_id,
                    principalSchema: "public",
                    principalTable: "sys_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_apv_delegations_sys_users_delegate_user_id",
                    column: x => x.delegate_user_id,
                    principalSchema: "public",
                    principalTable: "sys_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_apv_delegations_apv_approval_templates_template_id",
                    column: x => x.template_id,
                    principalSchema: "public",
                    principalTable: "apv_approval_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "apv_approval_levels",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                template_id = table.Column<int>(type: "integer", nullable: false),
                level_order = table.Column<int>(type: "integer", nullable: false),
                level_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                approver_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                approver_role_id = table.Column<int>(type: "integer", nullable: true),
                approver_position_id = table.Column<int>(type: "integer", nullable: true),
                approver_user_id = table.Column<int>(type: "integer", nullable: true),
                min_approvers_required = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                escalation_hours = table.Column<int>(type: "integer", nullable: true),
                escalate_to_level_id = table.Column<int>(type: "integer", nullable: true),
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
                table.PrimaryKey("pk_apv_approval_levels", x => x.id);
                table.ForeignKey(
                    name: "fk_apv_approval_levels_apv_approval_templates_template_id",
                    column: x => x.template_id,
                    principalSchema: "public",
                    principalTable: "apv_approval_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_apv_approval_levels_cfg_roles_approver_role_id",
                    column: x => x.approver_role_id,
                    principalSchema: "public",
                    principalTable: "cfg_roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_apv_approval_levels_hr_positions_approver_position_id",
                    column: x => x.approver_position_id,
                    principalSchema: "public",
                    principalTable: "hr_positions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_apv_approval_levels_sys_users_approver_user_id",
                    column: x => x.approver_user_id,
                    principalSchema: "public",
                    principalTable: "sys_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_apv_approval_levels_apv_approval_levels_escalate_to_level_id",
                    column: x => x.escalate_to_level_id,
                    principalSchema: "public",
                    principalTable: "apv_approval_levels",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "apv_approval_requests",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                request_no = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                template_id = table.Column<int>(type: "integer", nullable: false),
                module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                reference_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                reference_id = table.Column<int>(type: "integer", nullable: false),
                subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                requested_by = table.Column<int>(type: "integer", nullable: false),
                requested_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                current_level_id = table.Column<int>(type: "integer", nullable: true),
                due_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                final_action_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                final_action_by = table.Column<int>(type: "integer", nullable: true),
                notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_apv_approval_requests", x => x.id);
                table.ForeignKey(
                    name: "fk_apv_approval_requests_apv_approval_templates_template_id",
                    column: x => x.template_id,
                    principalSchema: "public",
                    principalTable: "apv_approval_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_apv_approval_requests_apv_approval_levels_current_level_id",
                    column: x => x.current_level_id,
                    principalSchema: "public",
                    principalTable: "apv_approval_levels",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_apv_approval_requests_sys_users_requested_by",
                    column: x => x.requested_by,
                    principalSchema: "public",
                    principalTable: "sys_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_apv_approval_requests_sys_users_final_action_by",
                    column: x => x.final_action_by,
                    principalSchema: "public",
                    principalTable: "sys_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "apv_approval_steps",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                request_id = table.Column<int>(type: "integer", nullable: false),
                level_id = table.Column<int>(type: "integer", nullable: false),
                level_order = table.Column<int>(type: "integer", nullable: false),
                approver_user_id = table.Column<int>(type: "integer", nullable: false),
                is_delegated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                delegated_from_user_id = table.Column<int>(type: "integer", nullable: true),
                action = table.Column<int>(type: "integer", nullable: true),
                action_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                due_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                notified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                reminder_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                table.PrimaryKey("pk_apv_approval_steps", x => x.id);
                table.ForeignKey(
                    name: "fk_apv_approval_steps_apv_approval_requests_request_id",
                    column: x => x.request_id,
                    principalSchema: "public",
                    principalTable: "apv_approval_requests",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_apv_approval_steps_apv_approval_levels_level_id",
                    column: x => x.level_id,
                    principalSchema: "public",
                    principalTable: "apv_approval_levels",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_apv_approval_steps_sys_users_approver_user_id",
                    column: x => x.approver_user_id,
                    principalSchema: "public",
                    principalTable: "sys_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_apv_approval_steps_sys_users_delegated_from_user_id",
                    column: x => x.delegated_from_user_id,
                    principalSchema: "public",
                    principalTable: "sys_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "apv_notifications",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                request_id = table.Column<int>(type: "integer", nullable: false),
                step_id = table.Column<int>(type: "integer", nullable: true),
                recipient_user_id = table.Column<int>(type: "integer", nullable: false),
                notification_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                channel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                body = table.Column<string>(type: "text", nullable: false),
                is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                read_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                sent_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                failed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_apv_notifications", x => x.id);
                table.ForeignKey(
                    name: "fk_apv_notifications_apv_approval_requests_request_id",
                    column: x => x.request_id,
                    principalSchema: "public",
                    principalTable: "apv_approval_requests",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_apv_notifications_apv_approval_steps_step_id",
                    column: x => x.step_id,
                    principalSchema: "public",
                    principalTable: "apv_approval_steps",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_apv_notifications_sys_users_recipient_user_id",
                    column: x => x.recipient_user_id,
                    principalSchema: "public",
                    principalTable: "sys_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "apv_approval_audit_logs",
            schema: "public",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                request_id = table.Column<int>(type: "integer", nullable: false),
                step_id = table.Column<int>(type: "integer", nullable: true),
                actor_user_id = table.Column<int>(type: "integer", nullable: false),
                action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                old_status = table.Column<int>(type: "integer", nullable: true),
                new_status = table.Column<int>(type: "integer", nullable: true),
                ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_apv_approval_audit_logs", x => x.id);
                table.ForeignKey(
                    name: "fk_apv_approval_audit_logs_apv_approval_requests_request_id",
                    column: x => x.request_id,
                    principalSchema: "public",
                    principalTable: "apv_approval_requests",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_apv_approval_audit_logs_apv_approval_steps_step_id",
                    column: x => x.step_id,
                    principalSchema: "public",
                    principalTable: "apv_approval_steps",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Standard indexes
        migrationBuilder.CreateIndex(name: "ix_apv_approval_templates_code", schema: "public", table: "apv_approval_templates", column: "code", unique: true);
        migrationBuilder.CreateIndex(name: "ix_apv_approval_templates_reference_type", schema: "public", table: "apv_approval_templates", column: "reference_type");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_templates_created_at", schema: "public", table: "apv_approval_templates", column: "created_at");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_templates_is_deleted", schema: "public", table: "apv_approval_templates", column: "is_deleted");

        migrationBuilder.CreateIndex(name: "ix_apv_approval_levels_template_id_level_order", schema: "public", table: "apv_approval_levels", columns: new[] { "template_id", "level_order" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_apv_approval_levels_approver_role_id", schema: "public", table: "apv_approval_levels", column: "approver_role_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_levels_approver_position_id", schema: "public", table: "apv_approval_levels", column: "approver_position_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_levels_approver_user_id", schema: "public", table: "apv_approval_levels", column: "approver_user_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_levels_escalate_to_level_id", schema: "public", table: "apv_approval_levels", column: "escalate_to_level_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_levels_created_at", schema: "public", table: "apv_approval_levels", column: "created_at");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_levels_is_deleted", schema: "public", table: "apv_approval_levels", column: "is_deleted");

        migrationBuilder.CreateIndex(name: "ix_apv_delegations_delegator_user_id", schema: "public", table: "apv_delegations", column: "delegator_user_id");
        migrationBuilder.CreateIndex(name: "ix_apv_delegations_delegate_user_id", schema: "public", table: "apv_delegations", column: "delegate_user_id");
        migrationBuilder.CreateIndex(name: "ix_apv_delegations_template_id", schema: "public", table: "apv_delegations", column: "template_id");
        migrationBuilder.CreateIndex(name: "ix_apv_delegations_start_date_end_date", schema: "public", table: "apv_delegations", columns: new[] { "start_date", "end_date" });
        migrationBuilder.CreateIndex(name: "ix_apv_delegations_created_at", schema: "public", table: "apv_delegations", column: "created_at");
        migrationBuilder.CreateIndex(name: "ix_apv_delegations_is_deleted", schema: "public", table: "apv_delegations", column: "is_deleted");

        migrationBuilder.CreateIndex(name: "ix_apv_approval_requests_request_no", schema: "public", table: "apv_approval_requests", column: "request_no", unique: true);
        migrationBuilder.CreateIndex(name: "ix_apv_approval_requests_reference_type_reference_id", schema: "public", table: "apv_approval_requests", columns: new[] { "reference_type", "reference_id" });
        migrationBuilder.CreateIndex(name: "ix_apv_approval_requests_template_id", schema: "public", table: "apv_approval_requests", column: "template_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_requests_current_level_id", schema: "public", table: "apv_approval_requests", column: "current_level_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_requests_requested_by", schema: "public", table: "apv_approval_requests", column: "requested_by");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_requests_final_action_by", schema: "public", table: "apv_approval_requests", column: "final_action_by");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_requests_status", schema: "public", table: "apv_approval_requests", column: "status");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_requests_due_at", schema: "public", table: "apv_approval_requests", column: "due_at");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_requests_created_at", schema: "public", table: "apv_approval_requests", column: "created_at");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_requests_is_deleted", schema: "public", table: "apv_approval_requests", column: "is_deleted");

        migrationBuilder.CreateIndex(name: "ix_apv_approval_steps_request_id", schema: "public", table: "apv_approval_steps", column: "request_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_steps_level_id", schema: "public", table: "apv_approval_steps", column: "level_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_steps_approver_user_id", schema: "public", table: "apv_approval_steps", column: "approver_user_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_steps_delegated_from_user_id", schema: "public", table: "apv_approval_steps", column: "delegated_from_user_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_steps_is_active", schema: "public", table: "apv_approval_steps", column: "is_active");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_steps_due_at", schema: "public", table: "apv_approval_steps", column: "due_at");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_steps_created_at", schema: "public", table: "apv_approval_steps", column: "created_at");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_steps_is_deleted", schema: "public", table: "apv_approval_steps", column: "is_deleted");

        migrationBuilder.CreateIndex(name: "ix_apv_notifications_request_id", schema: "public", table: "apv_notifications", column: "request_id");
        migrationBuilder.CreateIndex(name: "ix_apv_notifications_step_id", schema: "public", table: "apv_notifications", column: "step_id");
        migrationBuilder.CreateIndex(name: "ix_apv_notifications_recipient_user_id", schema: "public", table: "apv_notifications", column: "recipient_user_id");
        migrationBuilder.CreateIndex(name: "ix_apv_notifications_is_read", schema: "public", table: "apv_notifications", column: "is_read");
        migrationBuilder.CreateIndex(name: "ix_apv_notifications_created_at", schema: "public", table: "apv_notifications", column: "created_at");
        migrationBuilder.CreateIndex(name: "ix_apv_notifications_is_deleted", schema: "public", table: "apv_notifications", column: "is_deleted");

        migrationBuilder.CreateIndex(name: "ix_apv_approval_audit_logs_request_id", schema: "public", table: "apv_approval_audit_logs", column: "request_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_audit_logs_step_id", schema: "public", table: "apv_approval_audit_logs", column: "step_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_audit_logs_actor_user_id", schema: "public", table: "apv_approval_audit_logs", column: "actor_user_id");
        migrationBuilder.CreateIndex(name: "ix_apv_approval_audit_logs_created_at", schema: "public", table: "apv_approval_audit_logs", column: "created_at");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "apv_approval_audit_logs", schema: "public");
        migrationBuilder.DropTable(name: "apv_notifications", schema: "public");
        migrationBuilder.DropTable(name: "apv_approval_steps", schema: "public");
        migrationBuilder.DropTable(name: "apv_approval_requests", schema: "public");
        migrationBuilder.DropTable(name: "apv_delegations", schema: "public");
        migrationBuilder.DropTable(name: "apv_approval_levels", schema: "public");
        migrationBuilder.DropTable(name: "apv_approval_templates", schema: "public");
    }
}
