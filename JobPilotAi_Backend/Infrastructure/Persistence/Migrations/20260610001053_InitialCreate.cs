using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPilotAi_Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "applyai");

            migrationBuilder.CreateTable(
                name: "admin_logs",
                schema: "applyai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    target_entity = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata_json = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cover_letters",
                schema: "applyai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resume_id = table.Column<Guid>(type: "uuid", nullable: true),
                    job_title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    company_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    job_description = table.Column<string>(type: "text", nullable: false),
                    tone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    original_content = table.Column<string>(type: "text", nullable: false),
                    edited_content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cover_letters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_events",
                schema: "applyai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    provider_reference = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    status = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                schema: "applyai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    location = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    linked_in_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    portfolio_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    target_role = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    career_level = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "applyai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resume_analyses",
                schema: "applyai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resume_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ats_score = table.Column<int>(type: "integer", nullable: false),
                    keyword_match_score = table.Column<int>(type: "integer", nullable: false),
                    skills_coverage_score = table.Column<int>(type: "integer", nullable: false),
                    formatting_score = table.Column<int>(type: "integer", nullable: false),
                    experience_score = table.Column<int>(type: "integer", nullable: false),
                    education_score = table.Column<int>(type: "integer", nullable: false),
                    weaknesses_json = table.Column<string>(type: "text", nullable: false),
                    recommendations_json = table.Column<string>(type: "text", nullable: false),
                    missing_keywords_json = table.Column<string>(type: "text", nullable: false),
                    strengths_json = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resume_analyses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resumes",
                schema: "applyai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    file_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    storage_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    content_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    extracted_text = table.Column<string>(type: "text", nullable: true),
                    skills_json = table.Column<string>(type: "text", nullable: false),
                    experience_json = table.Column<string>(type: "text", nullable: false),
                    education_json = table.Column<string>(type: "text", nullable: false),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resumes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                schema: "applyai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    billing_cycle = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usage_records",
                schema: "applyai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    tokens_used = table.Column<int>(type: "integer", nullable: true),
                    processing_time_ms = table.Column<int>(type: "integer", nullable: true),
                    estimated_cost = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: true),
                    period_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usage_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "applyai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_logs_admin_id",
                schema: "applyai",
                table: "admin_logs",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_cover_letters_resume_id",
                schema: "applyai",
                table: "cover_letters",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "IX_cover_letters_user_id",
                schema: "applyai",
                table: "cover_letters",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_events_subscription_id",
                schema: "applyai",
                table: "payment_events",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_user_id",
                schema: "applyai",
                table: "profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token_hash",
                schema: "applyai",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                schema: "applyai",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_resume_analyses_resume_id",
                schema: "applyai",
                table: "resume_analyses",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "IX_resume_analyses_user_id",
                schema: "applyai",
                table: "resume_analyses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_resumes_user_id",
                schema: "applyai",
                table: "resumes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_resumes_user_id_content_hash",
                schema: "applyai",
                table: "resumes",
                columns: new[] { "user_id", "content_hash" });

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_user_id",
                schema: "applyai",
                table: "subscriptions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_user_id_action_type_period_start_period_end",
                schema: "applyai",
                table: "usage_records",
                columns: new[] { "user_id", "action_type", "period_start", "period_end" });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "applyai",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_logs",
                schema: "applyai");

            migrationBuilder.DropTable(
                name: "cover_letters",
                schema: "applyai");

            migrationBuilder.DropTable(
                name: "payment_events",
                schema: "applyai");

            migrationBuilder.DropTable(
                name: "profiles",
                schema: "applyai");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "applyai");

            migrationBuilder.DropTable(
                name: "resume_analyses",
                schema: "applyai");

            migrationBuilder.DropTable(
                name: "resumes",
                schema: "applyai");

            migrationBuilder.DropTable(
                name: "subscriptions",
                schema: "applyai");

            migrationBuilder.DropTable(
                name: "usage_records",
                schema: "applyai");

            migrationBuilder.DropTable(
                name: "users",
                schema: "applyai");
        }
    }
}
