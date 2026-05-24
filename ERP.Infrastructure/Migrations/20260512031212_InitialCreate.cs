using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "cfg_modules",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_cfg_modules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cfg_roles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("pk_cfg_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hr_leave_types",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    max_days_per_year = table.Column<int>(type: "integer", nullable: false),
                    is_carry_over = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("pk_hr_leave_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sys_audit_logs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entity_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    old_values = table.Column<string>(type: "text", nullable: true),
                    new_values = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sys_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sys_users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    language_preference = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "en"),
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
                    table.PrimaryKey("pk_sys_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cfg_menus",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    module_id = table.Column<int>(type: "integer", nullable: false),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_cfg_menus", x => x.id);
                    table.ForeignKey(
                        name: "fk_cfg_menus_cfg_menus_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "public",
                        principalTable: "cfg_menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cfg_menus_cfg_modules_module_id",
                        column: x => x.module_id,
                        principalSchema: "public",
                        principalTable: "cfg_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hr_payroll_runs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    period_month = table.Column<int>(type: "integer", nullable: false),
                    period_year = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    processed_by = table.Column<int>(type: "integer", nullable: true),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hr_payroll_runs", x => x.id);
                    table.ForeignKey(
                        name: "fk_hr_payroll_runs_sys_users_processed_by",
                        column: x => x.processed_by,
                        principalSchema: "public",
                        principalTable: "sys_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sys_refresh_tokens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_by_ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sys_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_sys_refresh_tokens_sys_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "sys_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sys_user_roles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sys_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_sys_user_roles_cfg_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "public",
                        principalTable: "cfg_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sys_user_roles_sys_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "sys_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cfg_role_menu_permissions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    menu_id = table.Column<int>(type: "integer", nullable: false),
                    can_view = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_create = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_edit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_delete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cfg_role_menu_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_cfg_role_menu_permissions_cfg_menus_menu_id",
                        column: x => x.menu_id,
                        principalSchema: "public",
                        principalTable: "cfg_menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cfg_role_menu_permissions_cfg_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "public",
                        principalTable: "cfg_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hr_attendance_records",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    check_in = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    check_out = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hr_attendance_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hr_departments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    manager_id = table.Column<int>(type: "integer", nullable: true),
                    parent_department_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_hr_departments", x => x.id);
                    table.ForeignKey(
                        name: "fk_hr_departments_hr_departments_parent_department_id",
                        column: x => x.parent_department_id,
                        principalSchema: "public",
                        principalTable: "hr_departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hr_positions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_hr_positions", x => x.id);
                    table.ForeignKey(
                        name: "fk_hr_positions_hr_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "public",
                        principalTable: "hr_departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hr_employees",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    employee_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    position_id = table.Column<int>(type: "integer", nullable: false),
                    hire_date = table.Column<DateOnly>(type: "date", nullable: false),
                    termination_date = table.Column<DateOnly>(type: "date", nullable: true),
                    employment_status = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hr_employees", x => x.id);
                    table.ForeignKey(
                        name: "fk_hr_employees_hr_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "public",
                        principalTable: "hr_departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_employees_hr_positions_position_id",
                        column: x => x.position_id,
                        principalSchema: "public",
                        principalTable: "hr_positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_employees_sys_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "sys_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "hr_leave_requests",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    leave_type_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_days = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    approved_by = table.Column<int>(type: "integer", nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hr_leave_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_hr_leave_requests_hr_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "public",
                        principalTable: "hr_employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_leave_requests_hr_leave_types_leave_type_id",
                        column: x => x.leave_type_id,
                        principalSchema: "public",
                        principalTable: "hr_leave_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_leave_requests_sys_users_approved_by",
                        column: x => x.approved_by,
                        principalSchema: "public",
                        principalTable: "sys_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "hr_payroll_details",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    payroll_run_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    basic_salary = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    allowances = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    deductions = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    gross_salary = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    net_salary = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hr_payroll_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_hr_payroll_details_hr_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "public",
                        principalTable: "hr_employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_payroll_details_hr_payroll_runs_payroll_run_id",
                        column: x => x.payroll_run_id,
                        principalSchema: "public",
                        principalTable: "hr_payroll_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cfg_menus_created_at",
                schema: "public",
                table: "cfg_menus",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_cfg_menus_is_deleted",
                schema: "public",
                table: "cfg_menus",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_cfg_menus_module_id",
                schema: "public",
                table: "cfg_menus",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "ix_cfg_menus_parent_id",
                schema: "public",
                table: "cfg_menus",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_cfg_modules_code",
                schema: "public",
                table: "cfg_modules",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cfg_modules_created_at",
                schema: "public",
                table: "cfg_modules",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_cfg_modules_is_deleted",
                schema: "public",
                table: "cfg_modules",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_cfg_role_menu_permissions_menu_id",
                schema: "public",
                table: "cfg_role_menu_permissions",
                column: "menu_id");

            migrationBuilder.CreateIndex(
                name: "ix_cfg_role_menu_permissions_role_id",
                schema: "public",
                table: "cfg_role_menu_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_cfg_role_menu_permissions_role_id_menu_id",
                schema: "public",
                table: "cfg_role_menu_permissions",
                columns: new[] { "role_id", "menu_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cfg_roles_created_at",
                schema: "public",
                table: "cfg_roles",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_cfg_roles_is_deleted",
                schema: "public",
                table: "cfg_roles",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_cfg_roles_name",
                schema: "public",
                table: "cfg_roles",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_hr_attendance_records_created_at",
                schema: "public",
                table: "hr_attendance_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_hr_attendance_records_date",
                schema: "public",
                table: "hr_attendance_records",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_hr_attendance_records_employee_id",
                schema: "public",
                table: "hr_attendance_records",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_attendance_records_employee_id_date",
                schema: "public",
                table: "hr_attendance_records",
                columns: new[] { "employee_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_attendance_records_is_deleted",
                schema: "public",
                table: "hr_attendance_records",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_hr_attendance_records_status",
                schema: "public",
                table: "hr_attendance_records",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_hr_departments_code",
                schema: "public",
                table: "hr_departments",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_departments_created_at",
                schema: "public",
                table: "hr_departments",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_hr_departments_is_deleted",
                schema: "public",
                table: "hr_departments",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_hr_departments_manager_id",
                schema: "public",
                table: "hr_departments",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_departments_parent_department_id",
                schema: "public",
                table: "hr_departments",
                column: "parent_department_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employees_created_at",
                schema: "public",
                table: "hr_employees",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employees_department_id",
                schema: "public",
                table: "hr_employees",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employees_employee_code",
                schema: "public",
                table: "hr_employees",
                column: "employee_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_employees_employment_status",
                schema: "public",
                table: "hr_employees",
                column: "employment_status");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employees_is_deleted",
                schema: "public",
                table: "hr_employees",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employees_position_id",
                schema: "public",
                table: "hr_employees",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employees_user_id",
                schema: "public",
                table: "hr_employees",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_leave_requests_approved_by",
                schema: "public",
                table: "hr_leave_requests",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "ix_hr_leave_requests_created_at",
                schema: "public",
                table: "hr_leave_requests",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_hr_leave_requests_employee_id",
                schema: "public",
                table: "hr_leave_requests",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_leave_requests_is_deleted",
                schema: "public",
                table: "hr_leave_requests",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_hr_leave_requests_leave_type_id",
                schema: "public",
                table: "hr_leave_requests",
                column: "leave_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_leave_requests_status",
                schema: "public",
                table: "hr_leave_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_hr_leave_types_code",
                schema: "public",
                table: "hr_leave_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_leave_types_created_at",
                schema: "public",
                table: "hr_leave_types",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_hr_leave_types_is_deleted",
                schema: "public",
                table: "hr_leave_types",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_hr_payroll_details_created_at",
                schema: "public",
                table: "hr_payroll_details",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_hr_payroll_details_employee_id",
                schema: "public",
                table: "hr_payroll_details",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_payroll_details_is_deleted",
                schema: "public",
                table: "hr_payroll_details",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_hr_payroll_details_payroll_run_id",
                schema: "public",
                table: "hr_payroll_details",
                column: "payroll_run_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_payroll_details_payroll_run_id_employee_id",
                schema: "public",
                table: "hr_payroll_details",
                columns: new[] { "payroll_run_id", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_payroll_runs_created_at",
                schema: "public",
                table: "hr_payroll_runs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_hr_payroll_runs_is_deleted",
                schema: "public",
                table: "hr_payroll_runs",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_hr_payroll_runs_period_month_period_year",
                schema: "public",
                table: "hr_payroll_runs",
                columns: new[] { "period_month", "period_year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_payroll_runs_processed_by",
                schema: "public",
                table: "hr_payroll_runs",
                column: "processed_by");

            migrationBuilder.CreateIndex(
                name: "ix_hr_payroll_runs_status",
                schema: "public",
                table: "hr_payroll_runs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_hr_positions_code",
                schema: "public",
                table: "hr_positions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_positions_created_at",
                schema: "public",
                table: "hr_positions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_hr_positions_department_id",
                schema: "public",
                table: "hr_positions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_positions_is_deleted",
                schema: "public",
                table: "hr_positions",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_sys_audit_logs_action",
                schema: "public",
                table: "sys_audit_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "ix_sys_audit_logs_created_at",
                schema: "public",
                table: "sys_audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_sys_audit_logs_user_id",
                schema: "public",
                table: "sys_audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_sys_refresh_tokens_token",
                schema: "public",
                table: "sys_refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sys_refresh_tokens_user_id",
                schema: "public",
                table: "sys_refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_sys_user_roles_role_id",
                schema: "public",
                table: "sys_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_sys_user_roles_user_id",
                schema: "public",
                table: "sys_user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_sys_user_roles_user_id_role_id",
                schema: "public",
                table: "sys_user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sys_users_created_at",
                schema: "public",
                table: "sys_users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_sys_users_email",
                schema: "public",
                table: "sys_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sys_users_is_deleted",
                schema: "public",
                table: "sys_users",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_sys_users_username",
                schema: "public",
                table: "sys_users",
                column: "username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_hr_attendance_records_hr_employees_employee_id",
                schema: "public",
                table: "hr_attendance_records",
                column: "employee_id",
                principalSchema: "public",
                principalTable: "hr_employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_hr_departments_hr_employees_manager_id",
                schema: "public",
                table: "hr_departments",
                column: "manager_id",
                principalSchema: "public",
                principalTable: "hr_employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_hr_departments_hr_employees_manager_id",
                schema: "public",
                table: "hr_departments");

            migrationBuilder.DropTable(
                name: "cfg_role_menu_permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hr_attendance_records",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hr_leave_requests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hr_payroll_details",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sys_audit_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sys_refresh_tokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sys_user_roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "cfg_menus",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hr_leave_types",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hr_payroll_runs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "cfg_roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "cfg_modules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hr_employees",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hr_positions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sys_users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hr_departments",
                schema: "public");
        }
    }
}
