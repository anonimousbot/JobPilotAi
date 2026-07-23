using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPilotAi_Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260612000000_AddSavedJobMatches")]
    public partial class AddSavedJobMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "saved_job_matches",
                schema: "applyai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_match_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    job_title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    company_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    location = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    match_score = table.Column<int>(type: "integer", nullable: false),
                    tags_json = table.Column<string>(type: "text", nullable: false),
                    saved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saved_job_matches", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_saved_job_matches_user_id",
                schema: "applyai",
                table: "saved_job_matches",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_saved_job_matches_user_id_job_match_id",
                schema: "applyai",
                table: "saved_job_matches",
                columns: ["user_id", "job_match_id"],
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "saved_job_matches",
                schema: "applyai");
        }
    }
}
